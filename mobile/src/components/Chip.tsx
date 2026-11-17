import { Pressable, StyleSheet, Text } from 'react-native';
import { cores, fontes } from '../theme';

interface ChipProps {
  rotulo: string;
  selecionado: boolean;
  onPress: () => void;
}

export function Chip({ rotulo, selecionado, onPress }: ChipProps) {
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={rotulo}
      accessibilityState={{ selected: selecionado }}
      onPress={onPress}
      style={[estilos.chip, selecionado && estilos.selecionado]}
    >
      <Text style={[estilos.texto, selecionado && estilos.textoSelecionado]}>{rotulo}</Text>
    </Pressable>
  );
}

const estilos = StyleSheet.create({
  chip: {
    minHeight: 44,
    paddingHorizontal: 14,
    justifyContent: 'center',
    borderRadius: 22,
    borderWidth: 1.5,
    borderColor: cores.borda,
    backgroundColor: cores.superficie,
    marginRight: 8,
    marginBottom: 8,
  },
  selecionado: { backgroundColor: cores.primaria, borderColor: cores.primaria },
  texto: { fontSize: fontes.normal, color: cores.texto },
  textoSelecionado: { color: cores.textoSobrePrimaria, fontWeight: '700' },
});
