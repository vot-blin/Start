import http from './http'

export const aiApi = {
  search(query) {
    return http.post('/ai/search', { query })
  },
}