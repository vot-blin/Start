<template>
  <header class="header">
    <div class="left">
      <router-link to="/" class="logo">MireaStore</router-link>
      <span class="tag">мини-магазин</span>
    </div>

    <nav class="nav">
      <router-link to="/">Каталог</router-link>
      <router-link to="/search">Поиск</router-link>
      <router-link to="/ai-search">AI поиск</router-link>
      <router-link to="/cart">Корзина <span class="count">{{ cart.totalCount }}</span></router-link>
      <router-link v-if="auth.isAuthenticated" to="/profile">Профиль</router-link>
      <router-link v-else to="/login">Войти</router-link>
    </nav>
  </header>
</template>

<script setup>
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'

const cart = useCartStore()
const auth = useAuthStore()
</script>

<style scoped>
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 18px;
  padding: 16px 24px;
  margin: 16px auto 0;
  max-width: 1240px;
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(124, 58, 237, 0.10);
  border-radius: 22px;
  box-shadow: 0 12px 28px rgba(73, 29, 129, 0.08);
}

.left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.logo {
  font-size: 24px;
  font-weight: 800;
  text-decoration: none;
  background: linear-gradient(135deg, #7c3aed 0%, #ec4899 100%);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.tag {
  font-size: 12px;
  color: #7c3aed;
  background: rgba(124, 58, 237, 0.10);
  padding: 6px 10px;
  border-radius: 999px;
}

.nav {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.nav a {
  text-decoration: none;
  color: #3a3550;
  padding: 10px 14px;
  border-radius: 999px;
  transition: background 0.2s ease, color 0.2s ease;
}

.nav a:hover,
.nav .router-link-active {
  background: rgba(124, 58, 237, 0.10);
  color: #7c3aed;
}

.count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 22px;
  height: 22px;
  margin-left: 6px;
  padding: 0 6px;
  border-radius: 999px;
  background: linear-gradient(135deg, #7c3aed 0%, #ec4899 100%);
  color: white;
  font-size: 12px;
  font-weight: 700;
}
</style>