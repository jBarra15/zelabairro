import type { IdentificacaoInput, Morador } from '../types/models';
import { request } from './api';

/** Cadastra o morador ou, se o e-mail já existir, atualiza nome e bairro. */
export function identificarMorador(dados: IdentificacaoInput): Promise<Morador> {
  return request<Morador>('/api/moradores/identificar', { method: 'POST', body: dados });
}
