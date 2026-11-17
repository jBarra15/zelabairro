import { Pressable, StyleSheet, Text, View } from 'react-native';
import type { Ocorrencia } from '../types/models';
import { cores, espacos, fontes } from '../theme';
import { formatarData, rotuloStatus } from '../utils/formatacao';
import { StatusBadge } from './StatusBadge';

interface OcorrenciaCardProps {
  ocorrencia: Ocorrencia;
  onPress: () => void;
}

export function OcorrenciaCard({ ocorrencia, onPress }: OcorrenciaCardProps) {
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={`${ocorrencia.titulo}. ${ocorrencia.categoria}. ${rotuloStatus(ocorrencia.status)}.`}
      onPress={onPress}
      style={({ pressed }) => [estilos.card, pressed && estilos.pressionado]}
    >
      <View style={estilos.topo}>
        <Text style={estilos.titulo} numberOfLines={2}>
          {ocorrencia.titulo}
        </Text>
        <StatusBadge status={ocorrencia.status} />
      </View>
      <Text style={estilos.categoria}>{ocorrencia.categoria}</Text>
      <View style={estilos.rodape}>
        <Text style={estilos.meta}>{formatarData(ocorrencia.criadaEm)}</Text>
        <Text style={estilos.meta}>👍 {ocorrencia.confirmacoes}</Text>
        {ocorrencia.origem === 'Sensor' ? <Text style={estilos.meta}>📡 Sensor</Text> : null}
      </View>
      {ocorrencia.atrasada ? <Text style={estilos.atrasada}>⚠ Fora do prazo de atendimento</Text> : null}
    </Pressable>
  );
}

const estilos = StyleSheet.create({
  card: {
    backgroundColor: cores.superficie,
    borderRadius: 14,
    padding: espacos.medio,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: cores.borda,
  },
  pressionado: { opacity: 0.85 },
  topo: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', gap: 8 },
  titulo: { flex: 1, fontSize: fontes.normal + 1, fontWeight: '700', color: cores.texto },
  categoria: { fontSize: fontes.pequena, color: cores.textoSuave, marginTop: 4 },
  rodape: { flexDirection: 'row', flexWrap: 'wrap', gap: 12, marginTop: 10 },
  meta: { fontSize: fontes.pequena, color: cores.textoSuave },
  atrasada: { marginTop: 8, fontSize: fontes.pequena, fontWeight: '700', color: cores.alerta },
});
