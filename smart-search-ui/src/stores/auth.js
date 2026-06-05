import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
    const user = ref(JSON.parse(localStorage.getItem('user') || null))
    const token = ref(localStorage.getItem('token') || '')

    const isAuthenticated = computed(() => !!token.value)

    function login(payload) {
        user.value = payload.user
        token.value = payload.token
        localStorage.setItem('user', JSON.stringify(payload.user))
        localStorage.setItem('token', payload.token)
    }

    function logout() {
        user.value = null
        token.value = ''
        localStorage.removeItem('user')
        localStorage.removeItem('token')
    }

    return { user, token, isAuthenticated, login, logout }
})