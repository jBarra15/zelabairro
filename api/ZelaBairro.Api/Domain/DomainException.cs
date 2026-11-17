namespace ZelaBairro.Api.Domain;

/// <summary>Violação de regra de negócio. A API devolve 400 com a mensagem.</summary>
public class DomainException(string mensagem) : Exception(mensagem);
