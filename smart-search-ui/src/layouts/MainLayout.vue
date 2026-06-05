<template>
  <div class="app">
    <AppHeader />
    <main class="main">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import AppHeader from '@/components/AppHeader.vue'
</script>

<style scoped>
.app {
  min-height: 100vh;
  background: #f6f6f8;
}
.main {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}
</style>