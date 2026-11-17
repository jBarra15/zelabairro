/** Um formulário é válido quando nenhuma regra devolveu mensagem de erro. */
export function formularioValido(erros: Record<string, string | undefined>): boolean {
  return Object.values(erros).every((mensagem) => mensagem === undefined);
}
