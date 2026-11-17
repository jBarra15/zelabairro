import { ActivityIndicator, Pressable, StyleSheet, Text } from 'react-native';
import { cores, espacos, fontes } from '../theme';

interface BotaoProps {
  titulo: string;
  onPress: () => void;
  variante?: 'primario' | 'secundario';
  desabilitado?: boolean;
  carregando?: boolean;
}

export function Botao({ titulo, onPress, variante = 'primario', desabilitado = false, carregando = false }: BotaoProps) {
  const inativo = desabilitado || carregando;
  const primario = variante === 'primario';

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={titulo}
      accessibilityState={{ disabled: inativo, busy: carregando }}
      disabled={inativo}
      onPress={onPress}
      style={({ pressed }) => [
        estilos.base,
        primario ? estilos.primario : estilos.secundario,
        inativo && estilos.inativo,
        pressed && estilos.pressionado,
      ]}
    >
      {carregando ? (
        <ActivityIndicator color={primario ? cores.textoSobrePrimaria : cores.primaria} />
      ) : (
        <Text style={[estilos.texto, primario ? estilos.textoPrimario : estilos.textoSecundario]}>{titulo}</Text>
      )}
    </Pressable>
  );
}

const estilos = StyleSheet.create({
  base: {
    minHeight: 52,
    borderRadius: 12,
    paddingHorizontal: espacos.medio,
    alignItems: 'center',
    justifyContent: 'center',
  },
  primario: { backgroundColor: cores.primaria },
  secundario: { backgroundColor: cores.superficie, borderWidth: 2, borderColor: cores.primaria },
  inativo: { opacity: 0.5 },
  pressionado: { opacity: 0.8 },
  texto: { fontSize: fontes.normal, fontWeight: '700' },
  textoPrimario: { color: cores.textoSobrePrimaria },
  textoSecundario: { color: cores.primaria },
});
