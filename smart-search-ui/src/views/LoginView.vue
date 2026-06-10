<template>
  <div class="page">
    <div class="section-box">
      <h1 class="page-title">Вход</h1>
      <p class="text">Войдите, чтобы получить доступ к профилю и AI-поиску.</p>

      <div class="form">
        <label>
          Email
          <input v-model="email" class="input-base" type="email" placeholder="email@example.com" @keyup.enter="onSubmit" />
        </label>

        <label>
          Пароль
          <input v-model="password" class="input-base" type="password" placeholder="Введите пароль" @keyup.enter="onSubmit" />
        </label>

        <div v-if="errorMessage" class="error-msg">{{ errorMessage }}</div>

        <button class="button-primary" @click="onSubmit" :disabled="loading">
          {{ loading ? 'Вход...' : 'Войти' }}
        </button>
      </div>

      <div class="links">
        <router-link to="/register">Нет аккаунта? Зарегистрироваться</router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/auth'

const email = ref('')
const password = ref('')
const errorMessage = ref('')
const loading = ref(false)
const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

async function onSubmit() {
  if (!email.value || !password.value) {
    errorMessage.value = 'Заполните все поля'
    return
  }
  errorMessage.value = ''
  loading.value = true

  try {
    const data = await authApi.login({ email: email.value, password: password.value })
    auth.login(data)
    const redirect = route.query.redirect || '/'
    router.push(redirect)
  } catch (err) {
    errorMessage.value = err.message || 'Ошибка входа'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.page {
  max-width: 520px;
  margin: 40px auto 0;
}

.form {
  display: grid;
  gap: 12px;
}

.form label {
  display: grid;
  gap: 6px;
  color: #3a3550;
  font-weight: 500;
}

.text {
  color: #6e6782;
  margin-bottom: 14px;
}

.error-msg {
  padding: 10px 14px;
  border-radius: 10px;
  background: rgba(239, 68, 68, 0.08);
  color: var(--danger);
  font-size: 14px;
}

.links {
  margin-top: 14px;
}
</style>
