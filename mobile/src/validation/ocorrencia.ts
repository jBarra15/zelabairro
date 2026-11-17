/** Validação pura do formulário de nova ocorrência (mesmas regras do backend). */

export interface OcorrenciaForm {
  titulo: string;
  descricao: string;
  categoriaId: number | null;
  latitude: number | null;
  longitude: number | null;
  endereco: string;
}

export type ErrosOcorrencia = Partial<Record<'titulo' | 'descricao' | 'categoriaId' | 'local', string>>;

export function validarOcorrencia(form: OcorrenciaForm): ErrosOcorrencia {
  const erros: ErrosOcorrencia = {};
  const titulo = form.titulo.trim();
  const endereco = form.endereco.trim();
  const temCoordenadas = form.latitude !== null && form.longitude !== null;

  if (titulo.length < 3) erros.titulo = 'Descreva o problema em poucas palavras (mínimo 3 letras).';
  else if (titulo.length > 100) erros.titulo = 'O título pode ter no máximo 100 caracteres.';

  if (form.descricao.trim().length > 500) erros.descricao = 'A descrição pode ter no máximo 500 caracteres.';

  if (form.categoriaId === null) erros.categoriaId = 'Escolha o tipo de problema.';

  if (!temCoordenadas && endereco.length < 5) {
    erros.local = 'Use sua localização ou digite o endereço do problema.';
  } else if (endereco.length > 200) {
    erros.local = 'O endereço pode ter no máximo 200 caracteres.';
  }

  return erros;
}
