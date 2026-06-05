import http from './http'

export const productsApi = {
  getAll() {
    return http.get('/products')
  },
  getById(id) {
    return http.get(`/products/${id}`)
  },
  search(q) {
    return http.get('/products/search', { params: { q } })
  },
}