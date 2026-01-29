<script setup lang="ts">
import { ref, onMounted } from 'vue'
import LoginForm from './components/LoginForm.vue'
import TodoList from './components/TodoList.vue'

const isLoggedIn = ref(false)
const username = ref('')
const darkMode = ref(true)

onMounted(() => {
  const token = localStorage.getItem('token')
  if (token) {
    // 簡單驗證 token 是否存在
    try {
      const payload = JSON.parse(atob(token.split('.')[1]))
      if (payload.exp * 1000 > Date.now()) {
        isLoggedIn.value = true
        username.value = payload.unique_name || 'User'
      } else {
        localStorage.removeItem('token')
      }
    } catch {
      localStorage.removeItem('token')
    }
  }
})

const handleLogin = (user: string) => {
  isLoggedIn.value = true
  username.value = user
}

const handleLogout = () => {
  isLoggedIn.value = false
  username.value = ''
  localStorage.removeItem('token')
}
</script>

<template>
  <v-app :theme="darkMode ? 'dark' : 'light'">
    <!-- 未登入顯示登入頁面 -->
    <template v-if="!isLoggedIn">
      <v-app-bar color="transparent" flat class="px-4">
        <v-app-bar-title class="text-cyan font-weight-bold">
          <v-icon class="mr-2">mdi-rocket-launch</v-icon>
          MyMoltbot
        </v-app-bar-title>
      </v-app-bar>
      
      <v-main class="login-background">
        <LoginForm @login="handleLogin" />
      </v-main>
    </template>

    <!-- 已登入顯示 TODO List -->
    <template v-else>
      <!-- 浮動控制按鈕 -->
      <div class="floating-controls">
        <v-btn
          icon
          size="small"
          color="cyan"
          variant="outlined"
          class="mr-2"
          @click="darkMode = !darkMode"
        >
          <v-icon>{{ darkMode ? 'mdi-weather-sunny' : 'mdi-weather-night' }}</v-icon>
        </v-btn>
        
        <v-chip color="cyan" variant="outlined" class="mr-2">
          <v-icon start>mdi-account</v-icon>
          {{ username }}
        </v-chip>
        
        <v-btn
          size="small"
          color="red"
          variant="outlined"
          @click="handleLogout"
        >
          <v-icon start>mdi-logout</v-icon>
          登出
        </v-btn>
      </div>

      <v-main class="pa-0">
        <TodoList />
      </v-main>
    </template>
  </v-app>
</template>

<style>
.login-background {
  background: linear-gradient(135deg, #0a0a1a 0%, #1a1a3a 50%, #0d0d2b 100%);
  min-height: 100vh;
}

.floating-controls {
  position: fixed;
  top: 1rem;
  right: 1rem;
  z-index: 1000;
  display: flex;
  align-items: center;
}
</style>
