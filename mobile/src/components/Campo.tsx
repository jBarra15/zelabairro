import { StyleSheet, Text, TextInput, View, type TextInputProps } from 'react-native';
import { cores, espacos, fontes } from '../theme';

interface CampoProps extends TextInputProps {
  rotulo: string;
  erro?: string;
}

export function Campo({ rotulo, erro, style, ...props }: CampoProps) {
  return (
    <View style={estilos.container}>
      <Text style={estilos.rotulo}>{rotulo}</Text>
      <TextInput
        accessibilityLabel={rotulo}
        placeholderTextColor={cores.textoSuave}
        style={[estilos.input, erro ? estilos.inputComErro : null, style]}
        {...props}
      />
      {erro ? (
        <Text style={estilos.erro} accessibilityLiveRegion="polite">
          {erro}
        </Text>
      ) : null}
    </View>
  );
}

const estilos = StyleSheet.create({
  container: { marginBottom: espacos.medio },
  rotulo: { fontSize: fontes.normal, fontWeight: '600', color: cores.texto, marginBottom: 6 },
  input: {
    minHeight: 50,
    borderWidth: 1.5,
    borderColor: cores.borda,
    borderRadius: 10,
    paddingHorizontal: 12,
    paddingVertical: 10,
    fontSize: fontes.normal,
    color: cores.texto,
    backgroundColor: cores.superficie,
  },
  inputComErro: { borderColor: cores.erro },
  erro: { color: cores.erro, fontSize: fontes.pequena, marginTop: 4 },
});
