import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { productsApi } from '@/api/products'

export const useProductsStore = defineStore('products', () => {
    const products = ref([])
    const loading = ref(false)
    const error = ref('')
    const loaded = ref(false)
    const query = ('')

    const filteredProducts = computed(() => {
        if(!query.value.trim()) return products.value
        const q = query.value.toLowerCase()
        return products.value.filter(p =>
            (p.title || '').toLowerCase().includes(q) ||
            (p.description || '').toLowerCase().includes(q) ||
            (p.category || '').toLowerCase().includes(q) 
        )
    })

    const getById = computed(() => {
        return (id) => products.value.find(p => String(p.id) === String(id))
    })

    async function fetchall(force = false) {
        if (loaded.value && !force) return products.value

        loading.value = true
        error.value = ''
        try {
            const res = await productsApi.getAll()
            products.value = res.data.products || res.data || []
            loaded.value = true
            return products.value
        }
        catch(e) {
            error.value = 'Не удалось загрузить каталог'
            products.value = []
            return []
        }
        finally {
            loading.value = false
        }
    }

    async function search(q) {
        query.value = q
        loading.value = true
        error.value = ''
        try {
            const res = await productsApi.search(q)
            products.value = res.data.products || res.data || []
            loaded.value = true
            return products.value
        } catch (e) {
            error.value = 'Ошибка поиска'
            products.value = []
        } finally {
            loading.value = false
        }
    }

    function setProducts(list) {
        products.value = Array.isArray(list) ? list : []
        loaded.value = true
    }

    return {
        products,
        loading,
        error,
        loaded,
        query,
        filteredProducts,
        getById,
        fetchall,
        search,
        setProducts,
    }
})