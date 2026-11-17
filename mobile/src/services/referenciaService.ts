import type { Bairro, Categoria } from '../types/models';
import { request } from './api';

export function listarBairros(): Promise<Bairro[]> {
  return request<Bairro[]>('/api/bairros');
}

export function listarCategorias(): Promise<Categoria[]> {
  return request<Categoria[]>('/api/categorias');
}
