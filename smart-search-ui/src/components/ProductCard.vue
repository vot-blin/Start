<template>
  <div class="card">
    <div class="img-wrap">
      <img :src="product.image" :alt="product.title" class="image" />
    </div>

    <div class="content">
      <div class="top">
        <span class="category">{{ product.category }}</span>
        <strong class="price">{{ product.price }} ₽</strong>
      </div>

      <h3 class="title">{{ product.title }}</h3>
      <p class="desc">{{ product.description }}</p>

      <div class="actions">
        <router-link :to="{ name: 'product', params: { id: product.id } }" class="btn ghost">
          Открыть
        </router-link>
        <button class="btn primary" @click="$emit('add-to-cart', product)">
          В корзину
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({ product: Object })
defineEmits(['add-to-cart'])
</script>

<style scoped>
.card {
  overflow: hidden;
  background: rgba(255, 255, 255, 0.92);
  border: 1px solid rgba(124, 58, 237, 0.10);
  border-radius: 20px;
  box-shadow: 0 12px 28px rgba(73, 29, 129, 0.08);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.card:hover {
  transform: translateY(-4px);
  box-shadow: 0 18px 34px rgba(73, 29, 129, 0.14);
}

.img-wrap {
  aspect-ratio: 1 / 1;
  background: linear-gradient(135deg, rgba(124, 58, 237, 0.08), rgba(236, 72, 153, 0.08));
}

.image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.content {
  padding: 16px;
}

.top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
  margin-bottom: 8px;
}

.category {
  font-size: 12px;
  color: #7c3aed;
  background: rgba(124, 58, 237, 0.10);
  padding: 6px 10px;
  border-radius: 999px;
}

.price {
  font-size: 16px;
  color: #1f1b2e;
}

.title {
  margin: 0 0 8px;
  font-size: 17px;
}

.desc {
  margin: 0 0 14px;
  color: #6e6782;
  font-size: 14px;
  line-height: 1.45;
  min-height: 40px;
}

.actions {
  display: flex;
  gap: 10px;
}

.btn {
  flex: 1;
  border: none;
  border-radius: 14px;
  padding: 10px 12px;
  text-decoration: none;
  text-align: center;
  cursor: pointer;
}

.ghost {
  background: #fff;
  color: #3a3550;
  border: 1px solid rgba(124, 58, 237, 0.14);
}

.primary {
  background: linear-gradient(135deg, #7c3aed 0%, #ec4899 100%);
  color: white;
}
</style>