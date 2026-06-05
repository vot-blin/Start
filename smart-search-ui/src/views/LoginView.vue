<template>
  <div class="page">
    <div class="section-box">
      <h1 class="page-title">Вход</h1>
      <p class="text">Войдите, чтобы получить доступ к корзине, профилю и AI-поиску.</p>

      <div class="form">
        <label>
          Email
          <input v-model="email" class="input-base" type="email" placeholder="email@example.com" />
        </label>

        <label>
          Пароль
          <input v-model="password" class="input-base" type="password" placeholder="Введите пароль" />
        </label>

        <button class="button-primary" @click="onSubmit">Войти</button>
      </div>

      <div class="links">
        <router-link to="/register">Нет аккаунта? Регистрация</router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/auth'

const email = ref('')
const password = ref('')
const errorMessage = ref('')
const router = useRouter()
const auth = useAuthStore()

async function onSubmit() {
  errorMessage.value = ''

  try {
    const data = await authApi.login({
      email: email.value,
      password: password.value
    })

    console.log('УСПЕХ! Ответ сервера:', data)
    router.push('/')
  } catch (err) {
    errorMessage.value = err.message || 'Ошибка входа'
    console.error('ОШИБКА!', err.message)
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

.links {
  margin-top: 14px;
}
</style>