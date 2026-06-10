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

      <div class="filters">
        <div class="filter-group">
          <label class="filter-label">Категория</label>
          <select v-model="category" class="input-base select">
            <option value="">Все категории</option>
            <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
          </select>
        </div>

        <div class="filter-group">
          <label class="filter-label">Цена от, ₽</label>
          <input v-model.number="minPrice" type="number" min="0" class="input-base" placeholder="0" />
        </div>

        <div class="filter-group">
          <label class="filter-label">Цена до, ₽</label>
          <input v-model.number="maxPrice" type="number" min="0" class="input-base" placeholder="∞" />
        </div>

        <button class="button-secondary reset-btn" @click="resetFilters">Сбросить</button>
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
import { ref, onMounted } from 'vue'
import SearchSwitch from '@/components/SearchSwitch.vue'
import ProductCard from '@/components/ProductCard.vue'
import { productsApi } from '@/api/products'
import { useCartStore } from '@/stores/cart'

const query = ref('')
const category = ref('')
const minPrice = ref(null)
const maxPrice = ref(null)
const results = ref([])
const categories = ref([])
const loading = ref(false)
const searched = ref(false)
const cart = useCartStore()

onMounted(async () => {
  try {
    const res = await productsApi.getCategories()
    categories.value = res.data.categories || []
  } catch {
    categories.value = []
  }
})

async function search() {
  loading.value = true
  searched.value = true
  try {
    const res = await productsApi.search(query.value, {
      category: category.value || undefined,
      minPrice: minPrice.value || undefined,
      maxPrice: maxPrice.value || undefined
    })
    results.value = res.data.products || []
  } catch {
    results.value = []
  } finally {
    loading.value = false
  }
}

function resetFilters() {
  category.value = ''
  minPrice.value = null
  maxPrice.value = null
  if (searched.value) search()
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
  margin-bottom: 16px;
}

.search-row input {
  flex: 1;
  min-width: 260px;
}

.filters {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  align-items: flex-end;
  padding-top: 14px;
  border-top: 1px solid rgba(124, 58, 237, 0.08);
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
  flex: 1;
  min-width: 140px;
}

.filter-label {
  font-size: 13px;
  font-weight: 500;
  color: #3a3550;
}

.select {
  cursor: pointer;
}

.reset-btn {
  align-self: flex-end;
  white-space: nowrap;
}
</style>
