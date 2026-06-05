<template>
  <div v-if="product" class="product">
    <img :src="product.image" :alt="product.title" class="image" />
    <div class="info">
      <h1>{{ product.title }}</h1>
      <p>{{ product.description }}</p>
      <p class="category">{{ product.category }}</p>
      <h2>{{ product.price }} ₽</h2>
      <button @click="cart.addToCart(product)">Добавить в корзину</button>
    </div>
  </div>
  <div v-else>Товар не найден</div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useProductsStore } from '@/stores/products'
import { useCartStore } from '@/stores/cart'

const route = useRoute()
const products = useProductsStore()
const cart = useCartStore()

onMounted(() => {
  products.fetchAll()
})

const product = computed(() => products.getById(route.params.id))
</script>