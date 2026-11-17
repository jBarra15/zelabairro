import { StyleSheet, Text, View } from 'react-native';
import type { StatusOcorrencia } from '../types/models';
import { fontes } from '../theme';
import { rotuloStatus } from '../utils/formatacao';

const CORES: Record<StatusOcorrencia, { fundo: string; texto: string }> = {
  Aberta: { fundo: '#FFF4D6', texto: '#7A4F00' },
  EmAtendimento: { fundo: '#E1ECFB', texto: '#174EA6' },
  Resolvida: { fundo: '#E3F2EA', texto: '#0B5D3B' },
  Cancelada: { fundo: '#ECEDEC', texto: '#4F5853' },
};

export function StatusBadge({ status }: { status: StatusOcorrencia }) {
  const cor = CORES[status];
  return (
    <View style={[estilos.badge, { backgroundColor: cor.fundo }]}>
      <Text style={[estilos.texto, { color: cor.texto }]}>{rotuloStatus(status)}</Text>
    </View>
  );
}

const estilos = StyleSheet.create({
  badge: { paddingHorizontal: 10, paddingVertical: 4, borderRadius: 999, alignSelf: 'flex-start' },
  texto: { fontSize: fontes.pequena, fontWeight: '700' },
});
