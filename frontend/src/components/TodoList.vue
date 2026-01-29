<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'

interface Todo {
  id: number
  title: string
  description?: string
  isCompleted: boolean
  priority: number
  createdAt: string
  completedAt?: string
  dueDate?: string
}

const todos = ref<Todo[]>([])
const loading = ref(false)
const dialog = ref(false)
const editMode = ref(false)
const currentTodo = ref<Partial<Todo>>({})
const error = ref('')

const newTodo = ref({
  title: '',
  description: '',
  priority: 3,
  dueDate: ''
})

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${localStorage.getItem('token')}`
})

const priorityLabels = ['', '低軌道', '近地', '月球', '火星', '星際']
const priorityColors = ['', 'grey', 'blue', 'green', 'orange', 'red']
const priorityIcons = ['', 'mdi-satellite-variant', 'mdi-earth', 'mdi-moon-waning-crescent', 'mdi-planet-mars', 'mdi-star-shooting']

const sortedTodos = computed(() => {
  return [...todos.value].sort((a, b) => {
    if (a.isCompleted !== b.isCompleted) return a.isCompleted ? 1 : -1
    return b.priority - a.priority
  })
})

const stats = computed(() => ({
  total: todos.value.length,
  completed: todos.value.filter(t => t.isCompleted).length,
  active: todos.value.filter(t => !t.isCompleted).length,
  highPriority: todos.value.filter(t => !t.isCompleted && t.priority >= 4).length
}))

const fetchTodos = async () => {
  loading.value = true
  error.value = ''
  try {
    const res = await fetch(`${API_URL}/todo`, { headers: getAuthHeaders() })
    if (res.ok) {
      todos.value = await res.json()
    } else {
      error.value = `載入失敗: ${res.status}`
    }
  } catch (e) {
    error.value = '網路錯誤'
    console.error('Fetch todos error:', e)
  } finally {
    loading.value = false
  }
}

const createTodo = async () => {
  if (!newTodo.value.title.trim()) return
  
  try {
    const res = await fetch(`${API_URL}/todo`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({
        title: newTodo.value.title,
        description: newTodo.value.description || null,
        priority: newTodo.value.priority,
        dueDate: newTodo.value.dueDate || null
      })
    })
    
    if (!res.ok) {
      const data = await res.json().catch(() => ({}))
      error.value = data.message || `建立失敗: ${res.status}`
      return
    }
    
    newTodo.value = { title: '', description: '', priority: 3, dueDate: '' }
    dialog.value = false
    await fetchTodos()
  } catch (e) {
    error.value = '網路錯誤，請重試'
    console.error('Create todo error:', e)
  }
}

const toggleComplete = async (todo: Todo) => {
  try {
    await fetch(`${API_URL}/todo/${todo.id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify({ isCompleted: !todo.isCompleted })
    })
    await fetchTodos()
  } catch (e) {
    console.error('Toggle error:', e)
  }
}

const deleteTodo = async (id: number) => {
  try {
    await fetch(`${API_URL}/todo/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    })
    await fetchTodos()
  } catch (e) {
    console.error('Delete error:', e)
  }
}

const openEdit = (todo: Todo) => {
  currentTodo.value = { ...todo }
  editMode.value = true
  dialog.value = true
}

const openNew = () => {
  editMode.value = false
  newTodo.value = { title: '', description: '', priority: 3, dueDate: '' }
  dialog.value = true
}

const updateTodo = async () => {
  if (!currentTodo.value.id) return
  
  try {
    await fetch(`${API_URL}/todo/${currentTodo.value.id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify({
        title: currentTodo.value.title,
        description: currentTodo.value.description,
        priority: currentTodo.value.priority,
        dueDate: currentTodo.value.dueDate
      })
    })
    dialog.value = false
    await fetchTodos()
  } catch (e) {
    console.error('Update error:', e)
  }
}

const formatDate = (date?: string) => {
  if (!date) return ''
  return new Date(date).toLocaleDateString('zh-TW')
}

onMounted(fetchTodos)
</script>

<template>
  <div class="space-container">
    <!-- 星空背景 -->
    <div class="stars"></div>
    <div class="stars2"></div>

    <!-- 控制台標題 -->
    <div class="console-header">
      <div class="header-top">
        <div class="logo-section">
          <v-icon class="logo-icon">mdi-rocket-launch</v-icon>
          <div>
            <h1 class="title">任務控制台</h1>
            <p class="subtitle">Mission Control</p>
          </div>
        </div>
      </div>
      
      <!-- 狀態面板 -->
      <div class="stats-row">
        <div class="stat-box">
          <div class="stat-value text-cyan">{{ stats.total }}</div>
          <div class="stat-label">總任務</div>
        </div>
        <div class="stat-box">
          <div class="stat-value text-green">{{ stats.completed }}</div>
          <div class="stat-label">完成</div>
        </div>
        <div class="stat-box">
          <div class="stat-value text-orange">{{ stats.highPriority }}</div>
          <div class="stat-label">緊急</div>
        </div>
      </div>
    </div>

    <!-- 錯誤提示 -->
    <v-alert v-if="error" type="error" class="mx-auto mb-4 error-alert" max-width="600" closable @click:close="error = ''">
      {{ error }}
    </v-alert>

    <!-- 主面板 -->
    <v-card class="console-panel mx-auto">
      <v-card-title class="panel-header">
        <span class="panel-title">
          <v-icon class="mr-1" color="cyan" size="20">mdi-format-list-checks</v-icon>
          任務序列
        </span>
        <v-btn color="cyan" variant="flat" size="small" @click="openNew" class="glow-button">
          <v-icon start size="18">mdi-plus</v-icon>
          新增
        </v-btn>
      </v-card-title>

      <v-divider color="cyan" opacity="0.3"></v-divider>

      <v-card-text class="pa-0">
        <v-progress-linear v-if="loading" indeterminate color="cyan"></v-progress-linear>

        <v-list class="todo-list" bg-color="transparent">
          <TransitionGroup name="list">
            <v-list-item
              v-for="todo in sortedTodos"
              :key="todo.id"
              :class="['todo-item', { completed: todo.isCompleted }]"
            >
              <template #prepend>
                <v-checkbox-btn
                  :model-value="todo.isCompleted"
                  color="cyan"
                  @update:model-value="toggleComplete(todo)"
                ></v-checkbox-btn>
              </template>

              <div class="todo-content">
                <div class="todo-title" :class="{ done: todo.isCompleted }">
                  {{ todo.title }}
                </div>
                <div v-if="todo.description" class="todo-desc">
                  {{ todo.description }}
                </div>
                <div class="todo-meta">
                  <v-chip
                    :color="priorityColors[todo.priority]"
                    size="x-small"
                    variant="flat"
                  >
                    <v-icon start size="12">{{ priorityIcons[todo.priority] }}</v-icon>
                    {{ priorityLabels[todo.priority] }}
                  </v-chip>
                  <v-chip v-if="todo.dueDate" size="x-small" color="purple" variant="outlined">
                    {{ formatDate(todo.dueDate) }}
                  </v-chip>
                </div>
              </div>

              <template #append>
                <div class="todo-actions">
                  <v-btn icon size="x-small" variant="text" color="cyan" @click="openEdit(todo)">
                    <v-icon size="16">mdi-pencil</v-icon>
                  </v-btn>
                  <v-btn icon size="x-small" variant="text" color="red" @click="deleteTodo(todo.id)">
                    <v-icon size="16">mdi-delete</v-icon>
                  </v-btn>
                </div>
              </template>
            </v-list-item>
          </TransitionGroup>

          <v-list-item v-if="!loading && todos.length === 0" class="empty-state">
            <div>
              <v-icon size="48" color="grey-darken-1">mdi-satellite-variant</v-icon>
              <p class="mt-2">軌道清空</p>
              <p class="text-caption text-grey">沒有待處理的任務</p>
            </div>
          </v-list-item>
        </v-list>
      </v-card-text>
    </v-card>

    <!-- 新增/編輯對話框 -->
    <v-dialog v-model="dialog" max-width="400" class="space-dialog">
      <v-card class="dialog-card">
        <v-card-title class="dialog-header">
          <v-icon class="mr-2" size="20">{{ editMode ? 'mdi-pencil' : 'mdi-rocket' }}</v-icon>
          {{ editMode ? '編輯任務' : '發射新任務' }}
        </v-card-title>

        <v-card-text class="pa-4">
          <template v-if="!editMode">
            <v-text-field
              v-model="newTodo.title"
              label="任務名稱"
              variant="outlined"
              color="cyan"
              density="compact"
              class="mb-3"
            ></v-text-field>

            <v-textarea
              v-model="newTodo.description"
              label="描述（選填）"
              variant="outlined"
              color="cyan"
              rows="2"
              density="compact"
              class="mb-3"
            ></v-textarea>

            <v-select
              v-model="newTodo.priority"
              :items="[1, 2, 3, 4, 5]"
              label="優先級"
              variant="outlined"
              color="cyan"
              density="compact"
              class="mb-3"
            >
              <template #item="{ item, props }">
                <v-list-item v-bind="props">
                  <template #prepend>
                    <v-icon :color="priorityColors[item.value]" size="18">{{ priorityIcons[item.value] }}</v-icon>
                  </template>
                  <v-list-item-title>{{ priorityLabels[item.value] }}</v-list-item-title>
                </v-list-item>
              </template>
              <template #selection="{ item }">
                <v-icon :color="priorityColors[item.value]" class="mr-2" size="18">{{ priorityIcons[item.value] }}</v-icon>
                {{ priorityLabels[item.value] }}
              </template>
            </v-select>

            <v-text-field
              v-model="newTodo.dueDate"
              label="截止日期"
              type="date"
              variant="outlined"
              color="cyan"
              density="compact"
            ></v-text-field>
          </template>

          <template v-else>
            <v-text-field
              v-model="currentTodo.title"
              label="任務名稱"
              variant="outlined"
              color="cyan"
              density="compact"
              class="mb-3"
            ></v-text-field>

            <v-textarea
              v-model="currentTodo.description"
              label="描述（選填）"
              variant="outlined"
              color="cyan"
              rows="2"
              density="compact"
              class="mb-3"
            ></v-textarea>

            <v-select
              v-model="currentTodo.priority"
              :items="[1, 2, 3, 4, 5]"
              label="優先級"
              variant="outlined"
              color="cyan"
              density="compact"
              class="mb-3"
            >
              <template #item="{ item, props }">
                <v-list-item v-bind="props">
                  <template #prepend>
                    <v-icon :color="priorityColors[item.value]" size="18">{{ priorityIcons[item.value] }}</v-icon>
                  </template>
                  <v-list-item-title>{{ priorityLabels[item.value] }}</v-list-item-title>
                </v-list-item>
              </template>
              <template #selection="{ item }">
                <v-icon :color="priorityColors[item.value]" class="mr-2" size="18">{{ priorityIcons[item.value] }}</v-icon>
                {{ priorityLabels[item.value] }}
              </template>
            </v-select>

            <v-text-field
              v-model="currentTodo.dueDate"
              label="截止日期"
              type="date"
              variant="outlined"
              color="cyan"
              density="compact"
            ></v-text-field>
          </template>
        </v-card-text>

        <v-card-actions class="pa-4 pt-0">
          <v-spacer></v-spacer>
          <v-btn variant="text" size="small" @click="dialog = false">取消</v-btn>
          <v-btn
            color="cyan"
            variant="flat"
            size="small"
            @click="editMode ? updateTodo() : createTodo()"
            class="glow-button"
          >
            {{ editMode ? '更新' : '發射！' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<style scoped>
.space-container {
  min-height: 100vh;
  background: linear-gradient(to bottom, #0a0a1a 0%, #1a1a3a 50%, #0d0d2b 100%);
  padding: 1rem;
  position: relative;
  overflow-x: hidden;
}

@media (min-width: 600px) {
  .space-container {
    padding: 2rem;
  }
}

/* 星星動畫 */
.stars, .stars2 {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
}

.stars {
  background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='400'%3E%3Ccircle cx='50' cy='50' r='1' fill='white'/%3E%3Ccircle cx='150' cy='100' r='0.5' fill='white'/%3E%3Ccircle cx='300' cy='200' r='1.5' fill='white'/%3E%3Ccircle cx='100' cy='300' r='0.8' fill='white'/%3E%3Ccircle cx='350' cy='350' r='1' fill='white'/%3E%3C/svg%3E");
  animation: drift 60s linear infinite;
}

.stars2 {
  background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Ccircle cx='80' cy='120' r='0.8' fill='%2300bcd4'/%3E%3Ccircle cx='220' cy='80' r='0.5' fill='%2300bcd4'/%3E%3Ccircle cx='150' cy='250' r='1' fill='%2300bcd4'/%3E%3C/svg%3E");
  animation: drift 90s linear infinite;
  opacity: 0.6;
}

@keyframes drift {
  from { transform: translateY(0); }
  to { transform: translateY(-100%); }
}

/* Header */
.console-header {
  max-width: 600px;
  margin: 0 auto 1rem;
  position: relative;
  z-index: 1;
}

.header-top {
  margin-bottom: 1rem;
}

.logo-section {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.logo-icon {
  font-size: 2rem;
  color: #00e5ff;
  text-shadow: 0 0 10px #00e5ff;
}

.title {
  font-size: 1.25rem;
  font-weight: bold;
  color: #00e5ff;
  text-shadow: 0 0 10px #00e5ff;
  margin: 0;
}

.subtitle {
  font-size: 0.7rem;
  color: #4dd0e1;
  margin: 0;
}

@media (min-width: 600px) {
  .logo-icon {
    font-size: 2.5rem;
  }
  .title {
    font-size: 1.5rem;
  }
}

/* Stats */
.stats-row {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-start;
}

.stat-box {
  background: rgba(0, 229, 255, 0.1);
  border: 1px solid rgba(0, 229, 255, 0.3);
  border-radius: 8px;
  padding: 0.4rem 0.75rem;
  text-align: center;
  min-width: 60px;
}

.stat-value {
  font-size: 1.25rem;
  font-weight: bold;
  font-family: 'Courier New', monospace;
}

.stat-label {
  font-size: 0.6rem;
  color: #90a4ae;
  text-transform: uppercase;
}

/* Main Panel */
.console-panel {
  max-width: 600px;
  background: rgba(10, 20, 40, 0.95) !important;
  border: 1px solid rgba(0, 229, 255, 0.3);
  border-radius: 12px;
  position: relative;
  z-index: 1;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
  background: linear-gradient(90deg, rgba(0, 229, 255, 0.1) 0%, transparent 100%);
}

.panel-title {
  font-size: 0.9rem;
  display: flex;
  align-items: center;
}

/* Todo List */
.todo-list {
  max-height: 60vh;
  overflow-y: auto;
}

.todo-item {
  border-bottom: 1px solid rgba(0, 229, 255, 0.1);
  padding: 0.5rem 0.75rem !important;
  min-height: auto !important;
}

.todo-item.completed {
  opacity: 0.5;
}

.todo-content {
  flex: 1;
  min-width: 0;
}

.todo-title {
  font-size: 0.9rem;
  color: #fff;
  word-break: break-word;
}

.todo-title.done {
  text-decoration: line-through;
  color: #666;
}

.todo-desc {
  font-size: 0.75rem;
  color: #90a4ae;
  margin-top: 2px;
}

.todo-meta {
  display: flex;
  gap: 0.25rem;
  margin-top: 0.25rem;
  flex-wrap: wrap;
}

.todo-actions {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

@media (min-width: 600px) {
  .todo-actions {
    flex-direction: row;
  }
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 2rem !important;
  color: #666;
}

/* Dialog */
.dialog-card {
  background: rgba(10, 20, 40, 0.98) !important;
  border: 1px solid rgba(0, 229, 255, 0.3);
}

.dialog-header {
  font-size: 1rem;
  padding: 1rem;
  background: linear-gradient(90deg, rgba(0, 229, 255, 0.2) 0%, transparent 100%);
  color: #00e5ff;
}

/* Glow Button */
.glow-button {
  box-shadow: 0 0 8px rgba(0, 229, 255, 0.5);
}

/* Error Alert */
.error-alert {
  position: relative;
  z-index: 1;
}

/* List Animation */
.list-enter-active,
.list-leave-active {
  transition: all 0.3s ease;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}

/* Scrollbar */
.todo-list::-webkit-scrollbar {
  width: 4px;
}

.todo-list::-webkit-scrollbar-track {
  background: rgba(0, 229, 255, 0.1);
}

.todo-list::-webkit-scrollbar-thumb {
  background: rgba(0, 229, 255, 0.3);
  border-radius: 2px;
}
</style>
