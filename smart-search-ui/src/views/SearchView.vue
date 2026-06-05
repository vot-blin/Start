<template>
  <div class="page">
    <div class="section-box">
      <h1 class="page-title">Обычный поиск</h1>
      <p class="text">Ищи товары по названию, категории или описанию.</p>

      <SearchSwitch mode="normal" />

      <div class="search-row">
        <input v-model="query" @keyup.enter="search" class="input-base" placeholder="Искать товары..." />
        <button class="button-primary" @click="search" :disabled="loading">
          {{ loading ? 'Поиск...' : 'Найти' }}
        </button>
      </div>
    </div>

    <div v-if="results.length" class="grid-products">
      <ProductCard
        v-for="product in results"
        :key="product.id"
        :product="product"
        @add-to-cart="cart.addToCart"
      />
    </div>

    <div v-if="!loading && searched && results.length === 0" class="empty-state">
      Ничего не найдено
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import SearchSwitch from '@/components/SearchSwitch.vue'
import ProductCard from '@/components/ProductCard.vue'
import { productsApi } from '@/api/products'
import { useCartStore } from '@/stores/cart'

const query = ref('')
const results = ref([])
const loading = ref(false)
const searched = ref(false)
const cart = useCartStore()

async function search() {
  if (!query.value.trim()) return
  loading.value = true
  searched.value = true
  try {
    const res = await productsApi.search(query.value)
    results.value = res.data.products || []
  } catch {
    results.value = []
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.page {
  display: grid;
  gap: 20px;
}

.text {
  color: #6e6782;
  margin-bottom: 14px;
}

.search-row {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.search-row input {
  flex: 1;
  min-width: 260px;
}
</style>