/** Cores com contraste mínimo AA (WCAG) para leitura por idosos e sob sol. */
export const cores = {
  primaria: '#0B5D3B',
  primariaClara: '#E3F2EA',
  textoSobrePrimaria: '#FFFFFF',
  fundo: '#F5F7F6',
  superficie: '#FFFFFF',
  texto: '#1B1F1D',
  textoSuave: '#4F5853',
  borda: '#CBD3CE',
  erro: '#B3261E',
  erroFundo: '#FDECEA',
  alerta: '#7A4F00',
  alertaFundo: '#FFF4D6',
} as const;

export const espacos = {
  pequeno: 8,
  medio: 16,
  grande: 24,
} as const;

/** Nada abaixo de 14: o cidadão real não é especialista e pode ter baixa visão. */
export const fontes = {
  pequena: 14,
  normal: 16,
  grande: 20,
  titulo: 24,
} as const;
