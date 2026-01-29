<script setup lang="ts">
import { ref, computed } from 'vue'

const emit = defineEmits<{
  login: [username: string]
}>()

const isRegister = ref(false)
const loading = ref(false)
const error = ref('')

const form = ref({
  username: '',
  password: '',
  email: ''
})

const isValid = computed(() => {
  return form.value.username.length >= 3 && form.value.password.length >= 6
})

const API_URL = 'http://localhost:5000/api'

const handleSubmit = async () => {
  if (!isValid.value) return
  
  loading.value = true
  error.value = ''
  
  try {
    const endpoint = isRegister.value ? 'auth/register' : 'auth/login'
    const body = isRegister.value 
      ? { username: form.value.username, password: form.value.password, email: form.value.email || null }
      : { username: form.value.username, password: form.value.password }
    
    const response = await fetch(`${API_URL}/${endpoint}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    })
    
    const data = await response.json()
    
    if (!response.ok) {
      throw new Error(data.message || '操作失敗')
    }
    
    localStorage.setItem('token', data.token)
    emit('login', data.username)
  } catch (e) {
    error.value = e instanceof Error ? e.message : '發生錯誤'
  } finally {
    loading.value = false
  }
}

const toggleMode = () => {
  isRegister.value = !isRegister.value
  error.value = ''
}
</script>

<template>
  <v-card class="mx-auto mt-8" max-width="450">
    <v-card-title class="text-h5 text-center pt-6">
      <v-icon class="mr-2" size="32">mdi-robot</v-icon>
      {{ isRegister ? '註冊帳號' : '登入' }}
    </v-card-title>
    
    <v-card-text>
      <v-alert v-if="error" type="error" class="mb-4" closable @click:close="error = ''">
        {{ error }}
      </v-alert>
      
      <v-form @submit.prevent="handleSubmit">
        <v-text-field
          v-model="form.username"
          label="使用者名稱"
          prepend-inner-icon="mdi-account"
          variant="outlined"
          :rules="[v => v.length >= 3 || '至少需要 3 個字元']"
          class="mb-2"
        />
        
        <v-text-field
          v-model="form.password"
          label="密碼"
          type="password"
          prepend-inner-icon="mdi-lock"
          variant="outlined"
          :rules="[v => v.length >= 6 || '至少需要 6 個字元']"
          class="mb-2"
        />
        
        <v-text-field
          v-if="isRegister"
          v-model="form.email"
          label="Email（選填）"
          type="email"
          prepend-inner-icon="mdi-email"
          variant="outlined"
          class="mb-2"
        />
        
        <v-btn
          type="submit"
          color="primary"
          size="large"
          block
          :loading="loading"
          :disabled="!isValid"
        >
          {{ isRegister ? '註冊' : '登入' }}
        </v-btn>
      </v-form>
    </v-card-text>
    
    <v-card-actions class="justify-center pb-4">
      <span class="text-body-2">
        {{ isRegister ? '已經有帳號？' : '還沒有帳號？' }}
      </span>
      <v-btn variant="text" color="primary" @click="toggleMode">
        {{ isRegister ? '登入' : '註冊' }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>
