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

const newTodo = ref({
  title: '',
  description: '',
  priority: 3,
  dueDate: ''
})

const API_URL = 'http://localhost:5000/api'

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${localStorage.getItem('token')}`
})

const priorityLabels = ['', '低軌道', '近地軌道', '月球任務', '火星任務', '星際任務']
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
  try {
    const res = await fetch(`${API_URL}/todo`, { headers: getAuthHeaders() })
    if (res.ok) todos.value = await res.json()
  } finally {
    loading.value = false
  }
}

const createTodo = async () => {
  if (!newTodo.value.title.trim()) return
  
  await fetch(`${API_URL}/todo`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify({
      title: newTodo.value.title,
      description: newTodo.value.description || null,
      priority: newTodo.value.priority,
      dueDate: newTodo.value.dueDate || null
    })
  })
  
  newTodo.value = { title: '', description: '', priority: 3, dueDate: '' }
  dialog.value = false
  await fetchTodos()
}

const toggleComplete = async (todo: Todo) => {
  await fetch(`${API_URL}/todo/${todo.id}`, {
    method: 'PUT',
    headers: getAuthHeaders(),
    body: JSON.stringify({ isCompleted: !todo.isCompleted })
  })
  await fetchTodos()
}

const deleteTodo = async (id: number) => {
  await fetch(`${API_URL}/todo/${id}`, {
    method: 'DELETE',
    headers: getAuthHeaders()
  })
  await fetchTodos()
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
    <div class="stars3"></div>

    <!-- 控制台標題 -->
    <v-card class="console-header mx-auto mb-6" max-width="900" color="transparent" flat>
      <div class="d-flex align-center justify-space-between flex-wrap ga-4">
        <div>
          <h1 class="text-h4 font-weight-bold neon-text">
            <v-icon class="mr-2">mdi-rocket-launch</v-icon>
            任務控制台
          </h1>
          <p class="text-subtitle-1 text-cyan-lighten-3 mt-1">Mission Control Center</p>
        </div>
        
        <!-- 狀態面板 -->
        <div class="d-flex ga-4">
          <div class="stat-box">
            <div class="stat-value text-cyan">{{ stats.total }}</div>
            <div class="stat-label">總任務</div>
          </div>
          <div class="stat-box">
            <div class="stat-value text-green">{{ stats.completed }}</div>
            <div class="stat-label">已完成</div>
          </div>
          <div class="stat-box">
            <div class="stat-value text-orange">{{ stats.highPriority }}</div>
            <div class="stat-label">緊急</div>
          </div>
        </div>
      </div>
    </v-card>

    <!-- 主面板 -->
    <v-card class="console-panel mx-auto" max-width="900">
      <v-card-title class="d-flex justify-space-between align-center py-4 px-6 panel-header">
        <span class="text-h6">
          <v-icon class="mr-2" color="cyan">mdi-format-list-checks</v-icon>
          活躍任務序列
        </span>
        <v-btn
          color="cyan"
          variant="flat"
          @click="openNew"
          prepend-icon="mdi-plus-circle"
          class="glow-button"
        >
          新增任務
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

              <v-list-item-title :class="{ 'text-decoration-line-through text-grey': todo.isCompleted }">
                {{ todo.title }}
              </v-list-item-title>
              
              <v-list-item-subtitle v-if="todo.description" class="text-grey-lighten-1">
                {{ todo.description }}
              </v-list-item-subtitle>

              <template #append>
                <div class="d-flex align-center ga-2">
                  <v-chip
                    :color="priorityColors[todo.priority]"
                    size="small"
                    variant="flat"
                    class="priority-chip"
                  >
                    <v-icon start size="14">{{ priorityIcons[todo.priority] }}</v-icon>
                    {{ priorityLabels[todo.priority] }}
                  </v-chip>
                  
                  <v-chip v-if="todo.dueDate" size="small" color="purple" variant="outlined">
                    <v-icon start size="14">mdi-calendar</v-icon>
                    {{ formatDate(todo.dueDate) }}
                  </v-chip>

                  <v-btn icon="mdi-pencil" size="small" variant="text" color="cyan" @click="openEdit(todo)"></v-btn>
                  <v-btn icon="mdi-delete" size="small" variant="text" color="red" @click="deleteTodo(todo.id)"></v-btn>
                </div>
              </template>
            </v-list-item>
          </TransitionGroup>

          <v-list-item v-if="!loading && todos.length === 0" class="text-center py-8">
            <div class="text-grey">
              <v-icon size="64" color="grey-darken-1">mdi-satellite-variant</v-icon>
              <p class="mt-4 text-h6">軌道清空</p>
              <p class="text-body-2">沒有待處理的任務</p>
            </div>
          </v-list-item>
        </v-list>
      </v-card-text>
    </v-card>

    <!-- 新增/編輯對話框 -->
    <v-dialog v-model="dialog" max-width="500" class="space-dialog">
      <v-card class="dialog-card">
        <v-card-title class="text-h5 pa-6 dialog-header">
          <v-icon class="mr-2">{{ editMode ? 'mdi-pencil' : 'mdi-rocket' }}</v-icon>
          {{ editMode ? '編輯任務' : '發射新任務' }}
        </v-card-title>

        <v-card-text class="pa-6">
          <v-text-field
            v-model="editMode ? currentTodo.title : newTodo.title"
            label="任務名稱"
            variant="outlined"
            color="cyan"
            prepend-inner-icon="mdi-flag"
            class="mb-4"
          ></v-text-field>

          <v-textarea
            v-model="editMode ? currentTodo.description : newTodo.description"
            label="任務描述"
            variant="outlined"
            color="cyan"
            rows="2"
            prepend-inner-icon="mdi-text"
            class="mb-4"
          ></v-textarea>

          <v-select
            v-model="editMode ? currentTodo.priority : newTodo.priority"
            :items="[1, 2, 3, 4, 5]"
            label="優先等級"
            variant="outlined"
            color="cyan"
            prepend-inner-icon="mdi-alert-circle"
            class="mb-4"
          >
            <template #item="{ item, props }">
              <v-list-item v-bind="props">
                <template #prepend>
                  <v-icon :color="priorityColors[item.value]">{{ priorityIcons[item.value] }}</v-icon>
                </template>
                <v-list-item-title>{{ priorityLabels[item.value] }}</v-list-item-title>
              </v-list-item>
            </template>
            <template #selection="{ item }">
              <v-icon :color="priorityColors[item.value]" class="mr-2">{{ priorityIcons[item.value] }}</v-icon>
              {{ priorityLabels[item.value] }}
            </template>
          </v-select>

          <v-text-field
            v-model="editMode ? currentTodo.dueDate : newTodo.dueDate"
            label="截止日期"
            type="date"
            variant="outlined"
            color="cyan"
            prepend-inner-icon="mdi-calendar"
          ></v-text-field>
        </v-card-text>

        <v-card-actions class="pa-6 pt-0">
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="dialog = false">取消</v-btn>
          <v-btn
            color="cyan"
            variant="flat"
            @click="editMode ? updateTodo() : createTodo()"
            class="glow-button"
          >
            <v-icon start>mdi-rocket-launch</v-icon>
            {{ editMode ? '更新任務' : '發射！' }}
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
  padding: 2rem;
  position: relative;
  overflow: hidden;
}

/* 星星動畫 */
.stars, .stars2, .stars3 {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
}

.stars {
  background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='400'%3E%3Ccircle cx='50' cy='50' r='1' fill='white'/%3E%3Ccircle cx='150' cy='100' r='0.5' fill='white'/%3E%3Ccircle cx='300' cy='200' r='1.5' fill='white'/%3E%3Ccircle cx='100' cy='300' r='0.8' fill='white'/%3E%3Ccircle cx='350' cy='350' r='1' fill='white'/%3E%3Ccircle cx='200' cy='50' r='0.6' fill='white'/%3E%3Ccircle cx='250' cy='280' r='1.2' fill='white'/%3E%3C/svg%3E");
  animation: drift 60s linear infinite;
}

.stars2 {
  background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Ccircle cx='80' cy='120' r='0.8' fill='%2300bcd4'/%3E%3Ccircle cx='220' cy='80' r='0.5' fill='%2300bcd4'/%3E%3Ccircle cx='150' cy='250' r='1' fill='%2300bcd4'/%3E%3C/svg%3E");
  animation: drift 90s linear infinite;
  opacity: 0.6;
}

.stars3 {
  background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='500' height='500'%3E%3Ccircle cx='100' cy='400' r='2' fill='%23ff4081'/%3E%3Ccircle cx='400' cy='100' r='1.5' fill='%23ffeb3b'/%3E%3C/svg%3E");
  animation: drift 120s linear infinite;
  opacity: 0.4;
}

@keyframes drift {
  from { transform: translateY(0); }
  to { transform: translateY(-100%); }
}

/* 霓虹文字 */
.neon-text {
  color: #00e5ff;
  text-shadow: 
    0 0 5px #00e5ff,
    0 0 10px #00e5ff,
    0 0 20px #00e5ff,
    0 0 40px #00bcd4;
}

/* 狀態面板 */
.stat-box {
  background: rgba(0, 229, 255, 0.1);
  border: 1px solid rgba(0, 229, 255, 0.3);
  border-radius: 8px;
  padding: 0.5rem 1rem;
  text-align: center;
  min-width: 70px;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: bold;
  font-family: 'Courier New', monospace;
}

.stat-label {
  font-size: 0.7rem;
  color: #90a4ae;
  text-transform: uppercase;
}

/* 控制台面板 */
.console-panel {
  background: rgba(10, 20, 40, 0.9) !important;
  border: 1px solid rgba(0, 229, 255, 0.3);
  border-radius: 12px;
  backdrop-filter: blur(10px);
  box-shadow: 
    0 0 20px rgba(0, 229, 255, 0.1),
    inset 0 0 60px rgba(0, 229, 255, 0.05);
}

.panel-header {
  background: linear-gradient(90deg, rgba(0, 229, 255, 0.1) 0%, transparent 100%);
}

/* TODO 項目 */
.todo-list {
  max-height: 500px;
  overflow-y: auto;
}

.todo-item {
  border-bottom: 1px solid rgba(0, 229, 255, 0.1);
  transition: all 0.3s ease;
}

.todo-item:hover {
  background: rgba(0, 229, 255, 0.05);
}

.todo-item.completed {
  opacity: 0.5;
}

.priority-chip {
  font-size: 0.7rem;
}

/* 發光按鈕 */
.glow-button {
  box-shadow: 0 0 10px rgba(0, 229, 255, 0.5);
  transition: box-shadow 0.3s ease;
}

.glow-button:hover {
  box-shadow: 0 0 20px rgba(0, 229, 255, 0.8);
}

/* 對話框 */
.dialog-card {
  background: rgba(10, 20, 40, 0.95) !important;
  border: 1px solid rgba(0, 229, 255, 0.3);
}

.dialog-header {
  background: linear-gradient(90deg, rgba(0, 229, 255, 0.2) 0%, transparent 100%);
  color: #00e5ff;
}

/* 列表動畫 */
.list-enter-active,
.list-leave-active {
  transition: all 0.3s ease;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

/* 滾動條 */
.todo-list::-webkit-scrollbar {
  width: 6px;
}

.todo-list::-webkit-scrollbar-track {
  background: rgba(0, 229, 255, 0.1);
}

.todo-list::-webkit-scrollbar-thumb {
  background: rgba(0, 229, 255, 0.3);
  border-radius: 3px;
}
</style>
