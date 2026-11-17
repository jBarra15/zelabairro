/** Espelham os DTOs da API ZelaBairro (enums chegam como texto). */

export type StatusOcorrencia = 'Aberta' | 'EmAtendimento' | 'Resolvida' | 'Cancelada';
export type Prioridade = 'NaoDefinida' | 'Baixa' | 'Media' | 'Alta';
export type OrigemOcorrencia = 'App' | 'Sensor' | 'Terminal';

export interface Bairro {
  id: number;
  nome: string;
  cidade: string;
}

export interface Categoria {
  id: number;
  nome: string;
  descricao: string;
  prazoAtendimentoDias: number;
}

export interface Morador {
  id: number;
  nome: string;
  email: string;
  bairroId: number;
  criadoEm: string;
}

export interface Ocorrencia {
  id: number;
  titulo: string;
  descricao: string;
  status: StatusOcorrencia;
  prioridade: Prioridade;
  origem: OrigemOcorrencia;
  confirmacoes: number;
  categoriaId: number;
  categoria: string;
  bairroId: number;
  bairro: string;
  moradorId: number | null;
  latitude: number;
  longitude: number;
  endereco: string | null;
  temFoto: boolean;
  atrasada: boolean;
  criadaEm: string;
  resolvidaEm: string | null;
}

export interface IdentificacaoInput {
  nome: string;
  email: string;
  bairroId: number;
}

export interface NovaOcorrenciaInput {
  titulo: string;
  descricao: string;
  categoriaId: number;
  bairroId: number;
  moradorId: number;
  latitude: number;
  longitude: number;
  endereco: string | null;
  origem: 'App';
}

export interface FiltroOcorrencias {
  bairroId?: number;
  moradorId?: number;
}

export type EstadoCarga = 'carregando' | 'erro' | 'pronto';
