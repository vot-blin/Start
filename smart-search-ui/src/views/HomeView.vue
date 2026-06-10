<template>
  <div class="page">
    <div class="hero section-box">
      <div>
        <h1 class="page-title">Каталог товаров</h1>
        <p class="subtitle">Мини-магазин с корзиной, поиском и AI-режимом.</p>
      </div>
    </div>

    <div v-if="products.loading" class="empty-state">Загрузка...</div>
    <div v-else-if="products.error" class="empty-state error">{{ products.error }}</div>
    <div v-else-if="products.products.length === 0" class="empty-state">Товары не найдены</div>

    <div v-else class="grid-products">
      <ProductCard
        v-for="product in products.products"
        :key="product.id"
        :product="product"
        @add-to-cart="cart.addToCart"
      />
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import ProductCard from '@/components/ProductCard.vue'
import { useProductsStore } from '@/stores/products'
import { useCartStore } from '@/stores/cart'

const products = useProductsStore()
const cart = useCartStore()

onMounted(() => products.fetchall())
</script>

<style scoped>
.page {
  display: grid;
  gap: 20px;
}

.hero {
  padding: 22px;
}

.subtitle {
  margin: 0;
  color: #6e6782;
}

.error {
  color: var(--danger);
  border-color: rgba(239, 68, 68, 0.2);
}
</style>
