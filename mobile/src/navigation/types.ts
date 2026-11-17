import type { NativeStackScreenProps } from '@react-navigation/native-stack';

export type RootStackParamList = {
  Identificacao: undefined;
  Ocorrencias: undefined;
  Detalhe: { id: number };
  NovaOcorrencia: undefined;
};

export type ScreenProps<T extends keyof RootStackParamList> = NativeStackScreenProps<RootStackParamList, T>;
