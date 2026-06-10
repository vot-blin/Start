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
        <input 
          v-model="query" 
          @keyup.enter="search" 
          class="input-base" 
          placeholder="Например: зимняя куртка серая с мехом" 
        />
        <button class="button-primary" @click="search" :disabled="loading">
          {{ loading ? 'Поиск...' : 'Найти с ИИ' }}
        </button>
      </div>
    </div>

    <!-- Динамический анализ запроса -->
    <div v-if="analysis" class="section-box">
      <h3>Анализ запроса</h3>
      <div class="analysis-grid">
        <div v-if="analysis.productType" class="analysis-item">
          <strong>Тип товара:</strong> <span>{{ analysis.productType }}</span>
        </div>
        <div v-if="analysis.color" class="analysis-item">
          <strong>Цвет:</strong> <span>{{ analysis.color }}</span>
        </div>
        <div v-if="analysis.size" class="analysis-item">
          <strong>Размер:</strong> <span>{{ analysis.size }}</span>
        </div>
        <div v-if="analysis.gender" class="analysis-item">
          <strong>Пол:</strong> <span>{{ formatGender(analysis.gender) }}</span>
        </div>
        <div v-if="analysis.material" class="analysis-item">
          <strong>Материал:</strong> <span>{{ analysis.material }}</span>
        </div>
        <div v-if="analysis.season" class="analysis-item">
          <strong>Сезон:</strong> <span>{{ analysis.season }}</span>
        </div>
        <div v-if="analysis.features?.length" class="analysis-item">
          <strong>Особенности:</strong> <span>{{ analysis.features.join(', ') }}</span>
        </div>
        <div v-if="analysis.normalizedQueryRu" class="analysis-item">
          <strong>Нормализованный запрос:</strong> <span>{{ analysis.normalizedQueryRu }}</span>
        </div>
        <div v-if="analysis.normalizedQueryEn" class="analysis-item">
          <strong>Поисковый запрос (EN):</strong> <span>{{ analysis.normalizedQueryEn }}</span>
        </div>
      </div>
    </div>

    <!-- Результаты поиска -->
    <div v-if="results.length" class="results-section">
      <h3>Результаты поиска ({{ results.length }} товаров)</h3>
      <div class="grid-products">
        <ProductCard
          v-for="product in results"
          :key="product.id"
          :product="product"
          @add-to-cart="cart.addToCart"
        />
      </div>
    </div>

    <!-- Загрузка -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Поиск с ИИ...</p>
    </div>

    <!-- Пустой результат -->
    <div v-if="!loading && searched && results.length === 0 && !analysis" class="empty-state">
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

function formatGender(gender) {
  const genders = {
    male: 'Мужской',
    female: 'Женский',
    unisex: 'Унисекс',
    kids: 'Детский'
  }
  return genders[gender] || gender
}

async function search() {
  if (!query.value.trim()) return
  
  loading.value = true
  searched.value = true
  results.value = []
  analysis.value = null
  
  try {
    const res = await aiApi.search(query.value)
    
    console.log('API Response:', res.data)
    
    // Сохраняем анализ и продукты
    analysis.value = res.data.analysis || null
    results.value = res.data.products || []
    
    console.log('Products count:', results.value.length)
    if (results.value.length > 0) {
      console.log('First product:', results.value[0])
    }
    
  } catch (error) {
    console.error('Search error:', error)
    if (error.response) {
      console.error('Error details:', error.response.data)
    }
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

.analysis-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 12px;
  margin-top: 16px;
}

.analysis-item {
  padding: 10px 14px;
  background: #f5f3f8;
  border-radius: 10px;
  font-size: 14px;
}

.analysis-item strong {
  color: #4a3a6e;
  margin-right: 8px;
}

.results-section {
  margin-top: 8px;
}

.results-section h3 {
  margin-bottom: 16px;
  color: #3a3550;
}

.grid-products {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;
}

.loading-state {
  text-align: center;
  padding: 40px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #e2e8f0;
  border-top-color: #7c3aed;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 12px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.empty-state {
  text-align: center;
  padding: 40px;
  color: #6e6782;
}
</style>