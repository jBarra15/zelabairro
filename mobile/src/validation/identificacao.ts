/**
 * Validação pura: sem React, sem rede, sem estado.
 * Recebe os dados do formulário e devolve as mensagens de erro por campo.
 */

export interface IdentificacaoForm {
  nome: string;
  email: string;
  bairroId: number | null;
}

export type ErrosIdentificacao = Partial<Record<keyof IdentificacaoForm, string>>;

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function validarIdentificacao(form: IdentificacaoForm): ErrosIdentificacao {
  const erros: ErrosIdentificacao = {};
  const nome = form.nome.trim();
  const email = form.email.trim();

  if (nome.length < 2) erros.nome = 'Informe seu nome (mínimo de 2 letras).';
  else if (nome.length > 80) erros.nome = 'O nome pode ter no máximo 80 caracteres.';

  if (email.length === 0) erros.email = 'Informe seu e-mail.';
  else if (email.length > 120 || !EMAIL_REGEX.test(email)) erros.email = 'Esse e-mail não parece válido.';

  if (form.bairroId === null) erros.bairroId = 'Escolha o seu bairro.';

  return erros;
}
