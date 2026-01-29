<script setup lang="ts">
import { ref } from 'vue'
import LoginForm from './components/LoginForm.vue'

const isLoggedIn = ref(false)
const username = ref('')

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
  <v-app>
    <v-app-bar color="primary" prominent>
      <v-app-bar-title>MyMoltbot</v-app-bar-title>
      <v-spacer></v-spacer>
      <template v-if="isLoggedIn">
        <v-chip class="mr-4" color="white" variant="outlined">
          <v-icon start>mdi-account</v-icon>
          {{ username }}
        </v-chip>
        <v-btn variant="outlined" @click="handleLogout">登出</v-btn>
      </template>
    </v-app-bar>

    <v-main>
      <v-container>
        <template v-if="!isLoggedIn">
          <LoginForm @login="handleLogin" />
        </template>
        <template v-else>
          <v-card class="mx-auto mt-8" max-width="600">
            <v-card-title class="text-h4">
              <v-icon class="mr-2">mdi-check-circle</v-icon>
              歡迎回來！
            </v-card-title>
            <v-card-text>
              <p class="text-h6">你好，{{ username }}！</p>
              <p class="text-body-1 mt-4">
                你已成功登入 MyMoltbot。這是你的個人工具起點。
              </p>
            </v-card-text>
          </v-card>
        </template>
      </v-container>
    </v-main>
  </v-app>
</template>
