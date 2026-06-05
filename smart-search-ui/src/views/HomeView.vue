<template>
  <div class="page">
    <div class="hero section-box">
      <div>
        <h1 class="page-title">Каталог товаров</h1>
        <p class="subtitle">Красивый мини-магазин с корзиной, поиском и AI-режимом.</p>
      </div>
    </div>

    <div class="grid-products">
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
import ProductCard from '@/components/ProductCard.vue'
import { useProductsStore } from '@/stores/products'
import { useCartStore } from '@/stores/cart'

const products = useProductsStore()
const cart = useCartStore()
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
</style>