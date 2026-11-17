import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { carregarMorador, limparMorador, salvarMorador } from '../services/storage';
import type { Morador } from '../types/models';

interface MoradorContextValue {
  morador: Morador | null;
  carregando: boolean;
  entrar: (morador: Morador) => Promise<void>;
  sair: () => Promise<void>;
}

const MoradorContext = createContext<MoradorContextValue | undefined>(undefined);

export function MoradorProvider({ children }: { children: ReactNode }) {
  const [morador, setMorador] = useState<Morador | null>(null);
  const [carregando, setCarregando] = useState(true);

  // Ao abrir o app, recupera a identificação salva (o morador não precisa se identificar de novo).
  useEffect(() => {
    let ativo = true;
    carregarMorador()
      .catch(() => null)
      .then((salvo) => {
        if (ativo) setMorador(salvo);
      })
      .finally(() => {
        if (ativo) setCarregando(false);
      });
    return () => {
      ativo = false;
    };
  }, []);

  const entrar = useCallback(async (novo: Morador) => {
    await salvarMorador(novo);
    setMorador(novo);
  }, []);

  const sair = useCallback(async () => {
    await limparMorador();
    setMorador(null);
  }, []);

  const valor = useMemo(() => ({ morador, carregando, entrar, sair }), [morador, carregando, entrar, sair]);

  return <MoradorContext.Provider value={valor}>{children}</MoradorContext.Provider>;
}

export function useMorador(): MoradorContextValue {
  const contexto = useContext(MoradorContext);
  if (!contexto) throw new Error('useMorador precisa estar dentro de <MoradorProvider>.');
  return contexto;
}
