import type { Prioridade, StatusOcorrencia } from '../types/models';

/**
 * A API devolve datas UTC às vezes sem o "Z" e com 7 casas de fração de segundo.
 * Normalizamos antes de converter para não errar o fuso (3h a mais/menos).
 */
export function formatarData(iso: string): string {
  const semExcessoDeFracao = iso.replace(/(\.\d{3})\d+/, '$1');
  const temFuso = /([zZ]|[+-]\d{2}:\d{2})$/.test(semExcessoDeFracao);
  const data = new Date(temFuso ? semExcessoDeFracao : `${semExcessoDeFracao}Z`);
  if (Number.isNaN(data.getTime())) return iso;

  const doisDigitos = (n: number): string => String(n).padStart(2, '0');
  return `${doisDigitos(data.getDate())}/${doisDigitos(data.getMonth() + 1)}/${data.getFullYear()} às ${doisDigitos(data.getHours())}:${doisDigitos(data.getMinutes())}`;
}

const ROTULOS_STATUS: Record<StatusOcorrencia, string> = {
  Aberta: 'Aberta',
  EmAtendimento: 'Em atendimento',
  Resolvida: 'Resolvida',
  Cancelada: 'Cancelada',
};

const ROTULOS_PRIORIDADE: Record<Prioridade, string> = {
  NaoDefinida: 'Em análise',
  Baixa: 'Baixa',
  Media: 'Média',
  Alta: 'Alta',
};

export const rotuloStatus = (status: StatusOcorrencia): string => ROTULOS_STATUS[status];
export const rotuloPrioridade = (prioridade: Prioridade): string => ROTULOS_PRIORIDADE[prioridade];
