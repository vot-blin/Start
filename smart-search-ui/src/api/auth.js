// src/api/auth.js
import http from './http'

async function login(payload) {
  try {
    const { data } = await http.post('/auth/login', payload)
    return data
  } catch (error) {
    const message =
      error.response?.data?.message ||
      error.message ||
      'Ошибка при входе'
    throw new Error(message)
  }
}

async function register(payload) {
  try {
    const response = await http.post('/auth/register', payload)
    return response.data
  } catch (error) {
    const message =
      error.response?.data?.message ||
      'Ошибка при регистрации'
    throw new Error(message)
  }
}

async function me() {
  return http.get('/auth/me')
}

export const authApi = {
  login,
  register,
  me
}