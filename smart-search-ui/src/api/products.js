import http from './http'

export const productsApi = {
  getAll(page = 1, pageSize = 100) {
    return http.get('/products', { params: { page, pageSize } })
  },
  getById(id) {
    return http.get(`/products/${id}`)
  },
  search(q, { category, minPrice, maxPrice } = {}) {
    return http.get('/products/search', { params: { q, category, minPrice, maxPrice } })
  },
  getCategories() {
    return http.get('/products/categories')
  },
}
