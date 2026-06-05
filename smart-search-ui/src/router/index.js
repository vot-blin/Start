import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import HomeView from '@/views/HomeView.vue'
import ProductView from '@/views/ProductView.vue'
import CartView from '@/views/CartView.vue'
import LoginView from '@/views/LoginView.vue'
import RegisterView from '@/views/RegisterView.vue'
import ProfileView from '@/views/ProfileView.vue'
import SearchView from '@/views/SearchView.vue'
import AiSearchView from '@/views/AiSearchView.vue'

const routes = [
    { path: '/', name: 'home', component: HomeView},
    { path: '/prodcut/:id', name: 'product', component: ProductView, props: true },
    { path: '/cart', name: 'cart', component: CartView},
    { path: '/login', name: 'login', component: LoginView},
    { path: '/register', name: 'register', component: RegisterView},
    { path: '/profile', name: 'profile', component: ProfileView, meta: { requiresAuth: true }},
    { path: '/search', name: 'search', component: SearchView},
    { path: '/ai-search', name: 'ai-search', component: AiSearchView, meta: { requiresAuth: true }},
]

const router = createRouter({
    history: createWebHistory(),
    routes,
})

router.beforeEach((to) => {
    const auth = useAuthStore()
    if (to.meta.requiresAuth && !auth.isAuthenticated) {
        return { name: 'login', query: { redirect: to.fullPath }}
    }
})

export default router