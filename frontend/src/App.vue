<script setup lang="ts">
import { ref, onMounted } from 'vue'
import LoginForm from './components/LoginForm.vue'
import TodoList from './components/TodoList.vue'

const isLoggedIn = ref(false)
const username = ref('')

onMounted(() => {
  const token = localStorage.getItem('token')
  if (token) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]))
      if (payload.exp * 1000 > Date.now()) {
        isLoggedIn.value = true
        username.value = payload.unique_name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'User'
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
  <v-app theme="dark">
    <!-- 未登入 -->
    <template v-if="!isLoggedIn">
      <v-main class="login-bg">
        <LoginForm @login="handleLogin" />
      </v-main>
    </template>

    <!-- 已登入 -->
    <template v-else>
      <!-- 頂部用戶欄 -->
      <div class="user-bar">
        <div class="user-info">
          <v-icon size="16" class="mr-1">mdi-account-circle</v-icon>
          <span>{{ username }}</span>
        </div>
        <v-btn
          size="x-small"
          color="red"
          variant="text"
          @click="handleLogout"
        >
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
html, body {
  overflow-x: hidden;
}

.login-bg {
  background: linear-gradient(135deg, #0a0a1a 0%, #1a1a3a 50%, #0d0d2b 100%);
  min-height: 100vh;
}

.user-bar {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem 1rem;
  background: rgba(10, 20, 40, 0.9);
  border-bottom: 1px solid rgba(0, 229, 255, 0.2);
  backdrop-filter: blur(10px);
}

.user-info {
  display: flex;
  align-items: center;
  color: #4dd0e1;
  font-size: 0.85rem;
}

/* 給 main 留出頂部空間 */
.v-main {
  padding-top: 44px !important;
}
</style>
