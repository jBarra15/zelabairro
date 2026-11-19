import { API_URL, REQUEST_TIMEOUT_MS } from '../config';

/** Erro vindo da API (ou da falta dela), já com mensagem pronta para o cidadão ler. */
export class ApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

type HttpMethod = 'GET' | 'POST' | 'PATCH' | 'DELETE';

interface RequestOptions {
  method?: HttpMethod;
  body?: unknown;
  formData?: FormData;
}

/** A API devolve { erro } nas regras de negócio e { errors: { Campo: [...] } } na validação automática. */
function extrairMensagem(dados: unknown, status: number): string {
  if (typeof dados === 'object' && dados !== null) {
    const registro = dados as Record<string, unknown>;
    if (typeof registro.erro === 'string') return registro.erro;

    if (typeof registro.errors === 'object' && registro.errors !== null) {
      const mensagens = Object.values(registro.errors as Record<string, unknown>)
        .flatMap((valor) => (Array.isArray(valor) ? valor : []))
        .filter((valor): valor is string => typeof valor === 'string');
      if (mensagens.length > 0) return mensagens[0];
    }
  }
  if (status === 404) return 'Não encontrado.';
  if (status >= 500) return 'O servidor está com problemas. Tente novamente em instantes.';
  return 'Não foi possível concluir a operação. Tente novamente.';
}

function lerJson(texto: string): unknown {
  if (!texto) return null;
  try {
    const dados: unknown = JSON.parse(texto);
    return dados;
  } catch {
    return null;
  }
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const controller = new AbortController();
  // Upload de foto demora mais que uma consulta comum: 30 s em vez de 10 s.
  const limite = options.formData ? REQUEST_TIMEOUT_MS * 3 : REQUEST_TIMEOUT_MS;
  const timer = setTimeout(() => controller.abort(), limite);

  const headers: Record<string, string> = { Accept: 'application/json' };
  let body: string | FormData | undefined;

  if (options.formData) {
    // Sem Content-Type manual: o fetch monta o multipart com o boundary correto.
    body = options.formData;
  } else if (options.body !== undefined) {
    headers['Content-Type'] = 'application/json';
    body = JSON.stringify(options.body);
  }

  let response: Response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      method: options.method ?? 'GET',
      headers,
      body,
      signal: controller.signal,
    });
  } catch {
    throw new ApiError('Sem conexão com o servidor. Verifique sua internet e tente novamente.', 0);
  } finally {
    clearTimeout(timer);
  }

  const dados = lerJson(await response.text());
  if (!response.ok) throw new ApiError(extrairMensagem(dados, response.status), response.status);
  return dados as T;
}

/**
 * Envio de arquivo (multipart) usando XMLHttpRequest.
 * O XHR do React Native entende o formato { uri, name, type } dentro do FormData
 * em qualquer versão do Expo, o que torna o upload da foto mais confiável que o fetch.
 */
export function enviarArquivo<T>(path: string, formData: FormData): Promise<T> {
  return new Promise<T>((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open('POST', `${API_URL}${path}`);
    xhr.setRequestHeader('Accept', 'application/json');
    xhr.timeout = REQUEST_TIMEOUT_MS * 3;

    xhr.onload = () => {
      const dados = lerJson(xhr.responseText);
      if (xhr.status >= 200 && xhr.status < 300) resolve(dados as T);
      else reject(new ApiError(extrairMensagem(dados, xhr.status), xhr.status));
    };
    xhr.onerror = () =>
      reject(new ApiError('Sem conexão com o servidor. Verifique sua internet e tente novamente.', 0));
    xhr.ontimeout = () =>
      reject(new ApiError('O envio da foto demorou demais. Tente novamente.', 0));

    xhr.send(formData);
  });
}

export function mensagemDeErro(erro: unknown): string {
  if (erro instanceof ApiError) return erro.message;
  return 'Algo deu errado. Tente novamente.';
}

export function urlFotoOcorrencia(id: number): string {
  return `${API_URL}/api/ocorrencias/${id}/foto`;
}
