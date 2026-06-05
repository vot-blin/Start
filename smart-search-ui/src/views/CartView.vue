<template>
  <div class="page">
    <div class="section-box">
      <h1 class="page-title">Корзина</h1>
      <p class="subtitle">Проверяй товары и изменяй количество прямо здесь.</p>
    </div>

    <div v-if="cart.items.length === 0" class="empty-state">
      Корзина пуста
    </div>

    <div v-else class="cart-grid">
      <div class="items">
        <div v-for="item in cart.items" :key="item.id" class="item card-base">
          <img :src="item.image" :alt="item.title" class="thumb" />
          <div class="meta">
            <h3>{{ item.title }}</h3>
            <p>{{ item.price }} ₽</p>
          </div>
          <div class="controls">
            <button class="button-secondary" @click="cart.updateQuantity(item.id, item.quantity - 1)">-</button>
            <span>{{ item.quantity }}</span>
            <button class="button-secondary" @click="cart.updateQuantity(item.id, item.quantity + 1)">+</button>
            <button class="button-danger" @click="cart.removeFromCart(item.id)">Удалить</button>
          </div>
        </div>
      </div>

      <aside class="summary section-box">
        <h3>Итого</h3>
        <p>Товаров: {{ cart.totalCount }}</p>
        <p><strong>{{ cart.totalPrice }} ₽</strong></p>
        <button class="button-primary full" @click="cart.clearCart">Очистить корзину</button>
      </aside>
    </div>
  </div>
</template>

<script setup>
import { useCartStore } from '@/stores/cart'
const cart = useCartStore()
</script>

<style scoped>
.page {
  display: grid;
  gap: 20px;
}

.subtitle {
  margin: 0;
  color: #6e6782;
}

.cart-grid {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 20px;
  align-items: start;
}

.items {
  display: grid;
  gap: 14px;
}

.item {
  display: grid;
  grid-template-columns: 84px 1fr auto;
  gap: 16px;
  align-items: center;
  padding: 14px;
}

.thumb {
  width: 84px;
  height: 84px;
  border-radius: 14px;
  object-fit: cover;
}

.controls {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.summary {
  position: sticky;
  top: 100px;
}

.full {
  width: 100%;
}
</style>