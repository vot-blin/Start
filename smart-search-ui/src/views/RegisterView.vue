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
          <input v-model="password" class="input-base" type="password" placeholder="Придумайте пароль" />
        </label>

        <button class="button-primary" @click="register">Создать аккаунт</button>
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

const name = ref('')
const email = ref('')
const password = ref('')
const router = useRouter()

async function register() {
  try {
    const data = await authApi.register({ 
      name: name.value,
      email: email.value, 
      password: password.value 
    })
    
    console.log('УСПЕХ! Ответ сервера:', data)
    router.push('/login')
    
  } catch (err) {
    console.error('ОШИБКА!', err)
    console.error('Статус:', err.response?.status)
    console.error('Данные:', err.response?.data)
    alert(err.message || 'Ошибка регистрации')
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