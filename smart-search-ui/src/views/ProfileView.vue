<template>
  <div class="page">
    <h1>Профиль</h1>
    <div class="card">
      <p><strong>Имя:</strong> {{ auth.user?.name }}</p>
      <p><strong>Email:</strong> {{ auth.user?.email }}</p>
      <button @click="logout">Выйти</button>
    </div>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

function logout() {
  auth.logout()
  router.push('/login')
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