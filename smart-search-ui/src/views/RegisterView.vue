<template>
  <div class="page">
    <div class="section-box">
      <h1 class="page-title">Регистрация</h1>
      <p class="text">Создайте аккаунт, чтобы пользоваться AI-поиском.</p>

      <div class="form">
        <label>
          Имя
          <input v-model="name" class="input-base" type="text" placeholder="Ваше имя" />
        </label>

        <label>
          Email
          <input v-model="email" class="input-base" type="email" placeholder="email@example.com" />
        </label>

        <label>
          Пароль
          <input v-model="password" class="input-base" type="password" placeholder="Минимум 6 символов" @keyup.enter="register" />
        </label>

        <div v-if="errorMessage" class="error-msg">{{ errorMessage }}</div>

        <button class="button-primary" @click="register" :disabled="loading">
          {{ loading ? 'Создание...' : 'Создать аккаунт' }}
        </button>
      </div>

      <div class="links">
        <router-link to="/login">Уже есть аккаунт? Войти</router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/auth'

const name = ref('')
const email = ref('')
const password = ref('')
const errorMessage = ref('')
const loading = ref(false)
const router = useRouter()
const auth = useAuthStore()

async function register() {
  if (!name.value || !email.value || !password.value) {
    errorMessage.value = 'Заполните все поля'
    return
  }
  errorMessage.value = ''
  loading.value = true

  try {
    const data = await authApi.register({ name: name.value, email: email.value, password: password.value })
    // Backend returns { token, user } — log the user in immediately
    auth.login(data)
    router.push('/')
  } catch (err) {
    errorMessage.value = err.message || 'Ошибка регистрации'
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
