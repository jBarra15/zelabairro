import AsyncStorage from '@react-native-async-storage/async-storage';
import type { Morador } from '../types/models';

const CHAVE_MORADOR = '@zelabairro/morador';

function ehMorador(valor: unknown): valor is Morador {
  if (typeof valor !== 'object' || valor === null) return false;
  const v = valor as Record<string, unknown>;
  return (
    typeof v.id === 'number' &&
    typeof v.nome === 'string' &&
    typeof v.email === 'string' &&
    typeof v.bairroId === 'number' &&
    typeof v.criadoEm === 'string'
  );
}

/** Guarda a identificação para o app lembrar do morador ao reabrir. */
export async function salvarMorador(morador: Morador): Promise<void> {
  await AsyncStorage.setItem(CHAVE_MORADOR, JSON.stringify(morador));
}

export async function carregarMorador(): Promise<Morador | null> {
  const bruto = await AsyncStorage.getItem(CHAVE_MORADOR);
  if (!bruto) return null;
  try {
    const dados: unknown = JSON.parse(bruto);
    return ehMorador(dados) ? dados : null;
  } catch {
    return null;
  }
}

export async function limparMorador(): Promise<void> {
  await AsyncStorage.removeItem(CHAVE_MORADOR);
}
