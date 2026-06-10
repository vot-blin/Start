<template>
  <div class="page">
    <div class="section-box profile-card">
      <h1 class="page-title">Профиль</h1>

      <div class="fields">
        <div class="field">
          <span class="field-label">Имя</span>
          <span class="field-value">{{ auth.user?.name }}</span>
        </div>
        <div class="field">
          <span class="field-label">Email</span>
          <span class="field-value">{{ auth.user?.email }}</span>
        </div>
      </div>

      <button class="button-danger logout-btn" @click="logout">Выйти из аккаунта</button>
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

.profile-card {
  padding: 28px;
  display: grid;
  gap: 24px;
}

.fields {
  display: grid;
  gap: 14px;
}

.field {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid rgba(124, 58, 237, 0.08);
}

.field-label {
  font-size: 14px;
  color: #6e6782;
}

.field-value {
  font-weight: 600;
  color: #1f1b2e;
}

.logout-btn {
  width: fit-content;
}
</style>
