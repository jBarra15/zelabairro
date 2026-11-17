import type { FiltroOcorrencias, NovaOcorrenciaInput, Ocorrencia } from '../types/models';
import { request } from './api';

export function listarOcorrencias(filtro: FiltroOcorrencias): Promise<Ocorrencia[]> {
  const partes: string[] = [];
  if (filtro.bairroId !== undefined) partes.push(`bairroId=${filtro.bairroId}`);
  if (filtro.moradorId !== undefined) partes.push(`moradorId=${filtro.moradorId}`);
  const query = partes.length > 0 ? `?${partes.join('&')}` : '';
  return request<Ocorrencia[]>(`/api/ocorrencias${query}`);
}

export function obterOcorrencia(id: number): Promise<Ocorrencia> {
  return request<Ocorrencia>(`/api/ocorrencias/${id}`);
}

export function criarOcorrencia(dados: NovaOcorrenciaInput): Promise<Ocorrencia> {
  return request<Ocorrencia>('/api/ocorrencias', { method: 'POST', body: dados });
}

export function confirmarOcorrencia(id: number): Promise<Ocorrencia> {
  return request<Ocorrencia>(`/api/ocorrencias/${id}/confirmar`, { method: 'POST' });
}

/** Envia a foto tirada pela câmera. A API aceita só JPG/PNG de até 5 MB. */
export function enviarFoto(id: number, uri: string): Promise<Ocorrencia> {
  const ultimoTrecho = uri.split('/').pop() ?? '';
  const ehPng = ultimoTrecho.toLowerCase().endsWith('.png');
  const temExtensaoValida = /\.(jpe?g|png)$/i.test(ultimoTrecho);
  const nome = temExtensaoValida ? ultimoTrecho : 'foto.jpg';

  const formData = new FormData();
  // No React Native, um arquivo entra no FormData como { uri, name, type }.
  formData.append('foto', { uri, name: nome, type: ehPng ? 'image/png' : 'image/jpeg' } as unknown as Blob);

  return request<Ocorrencia>(`/api/ocorrencias/${id}/foto`, { method: 'POST', formData });
}
