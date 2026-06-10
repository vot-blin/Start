<template>
  <div v-if="products.loading" class="empty-state">Загрузка...</div>
  <div v-else-if="product" class="product section-box">
    <img :src="product.image" :alt="product.title" class="image" />
    <div class="info">
      <span class="category-tag">{{ product.category }}</span>
      <h1 class="title">{{ product.title }}</h1>
      <p class="desc">{{ product.description }}</p>
      <p class="price">{{ product.price }} ₽</p>
      <button class="button-primary add-btn" @click="addAndNotify">В корзину</button>
      <p v-if="added" class="added-msg">Добавлено в корзину!</p>
    </div>
  </div>
  <div v-else class="empty-state">Товар не найден</div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useProductsStore } from '@/stores/products'
import { useCartStore } from '@/stores/cart'

const route = useRoute()
const products = useProductsStore()
const cart = useCartStore()
const added = ref(false)

onMounted(() => products.fetchall())

const product = computed(() => products.getById(route.params.id))

function addAndNotify() {
  if (!product.value) return
  cart.addToCart(product.value)
  added.value = true
  setTimeout(() => { added.value = false }, 2000)
}
</script>

<style scoped>
.product {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 32px;
  padding: 24px;
  max-width: 900px;
  margin: 0 auto;
}

.image {
  width: 100%;
  aspect-ratio: 1 / 1;
  object-fit: cover;
  border-radius: 16px;
}

.info {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.category-tag {
  display: inline-block;
  font-size: 12px;
  color: #7c3aed;
  background: rgba(124, 58, 237, 0.10);
  padding: 6px 10px;
  border-radius: 999px;
  width: fit-content;
}

.title {
  margin: 0;
  font-size: 26px;
  line-height: 1.2;
}

.desc {
  margin: 0;
  color: #6e6782;
  line-height: 1.6;
}

.price {
  margin: 0;
  font-size: 28px;
  font-weight: 700;
  color: #1f1b2e;
}

.add-btn {
  width: fit-content;
  padding: 14px 28px;
  font-size: 16px;
}

.added-msg {
  margin: 0;
  color: var(--success);
  font-weight: 500;
}

@media (max-width: 640px) {
  .product {
    grid-template-columns: 1fr;
  }
}
</style>
