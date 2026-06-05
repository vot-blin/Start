<template>
  <div class="page">
    <div class="section-box">
      <h1 class="page-title">AI поиск</h1>
      <p class="text">Доступен только авторизованным пользователям.</p>

      <SearchSwitch mode="ai" />

      <div v-if="!auth.isAuthenticated" class="locked">
        Для AI-поиска нужно войти в аккаунт.
      </div>

      <div v-else class="search-row">
        <input v-model="query" @keyup.enter="search" class="input-base" placeholder="Например: зимняя куртка серая с мехом" />
        <button class="button-primary" @click="search" :disabled="loading">
          {{ loading ? 'Поиск...' : 'Найти с ИИ' }}
        </button>
      </div>
    </div>

    <div v-if="analysis" class="section-box">
      <h3>Анализ запроса</h3>
      <p><strong>Тип товара:</strong> {{ analysis.productType }}</p>
      <p><strong>Цвет:</strong> {{ analysis.color }}</p>
      <p><strong>Размер:</strong> {{ analysis.size }}</p>
      <p><strong>Особенности:</strong> {{ analysis.features?.join(', ') || 'нет' }}</p>
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
import { aiApi } from '@/api/ai'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'

const query = ref('')
const results = ref([])
const analysis = ref(null)
const loading = ref(false)
const searched = ref(false)
const cart = useCartStore()
const auth = useAuthStore()

async function search() {
  if (!query.value.trim()) return
  loading.value = true
  searched.value = true
  try {
    const res = await aiApi.search(query.value)
    analysis.value = res.data.analysis || null
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

.locked {
  padding: 14px 16px;
  border-radius: 14px;
  background: rgba(124, 58, 237, 0.08);
  color: #7c3aed;
}
</style>