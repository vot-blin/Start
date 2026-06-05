<template>
  <div class="switcher">
    <router-link :to="normalPath" class="item" :class="{ active: mode === 'normal' }">
      Обычный поиск
    </router-link>
    <router-link :to="aiPath" class="item" :class="{ active: mode === 'ai' }">
      AI поиск
    </router-link>
  </div>
</template>

<script setup>
defineProps({
  mode: String,
  normalPath: { type: String, default: '/search' },
  aiPath: { type: String, default: '/ai-search' },
})
</script>

<style scoped>
.switcher {
  display: flex;
  gap: 10px;
  margin: 18px 0 22px;
  flex-wrap: wrap;
}

.item {
  padding: 10px 14px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.92);
  text-decoration: none;
  color: #3a3550;
  border: 1px solid rgba(124, 58, 237, 0.12);
}

.active {
  background: linear-gradient(135deg, #7c3aed 0%, #ec4899 100%);
  color: white;
  border-color: transparent;
}
</style>