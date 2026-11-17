import { ActivityIndicator, StyleSheet, Text, View } from 'react-native';
import { cores, espacos, fontes } from '../theme';
import { Botao } from './Botao';

/** Os três estados obrigatórios de uma lista: carregando, erro e vazio. */

export function Carregando({ mensagem = 'Carregando...' }: { mensagem?: string }) {
  return (
    <View style={estilos.centro} accessibilityRole="progressbar" accessibilityLabel={mensagem}>
      <ActivityIndicator size="large" color={cores.primaria} />
      <Text style={estilos.texto}>{mensagem}</Text>
    </View>
  );
}

export function ErroTela({ mensagem, onTentarNovamente }: { mensagem: string; onTentarNovamente: () => void }) {
  return (
    <View style={estilos.centro}>
      <Text style={estilos.icone}>⚠️</Text>
      <Text style={estilos.titulo}>Não deu certo</Text>
      <Text style={estilos.texto}>{mensagem}</Text>
      <View style={estilos.acao}>
        <Botao titulo="Tentar novamente" onPress={onTentarNovamente} />
      </View>
    </View>
  );
}

export function Vazio({ titulo, mensagem }: { titulo: string; mensagem: string }) {
  return (
    <View style={estilos.centro}>
      <Text style={estilos.icone}>🏘️</Text>
      <Text style={estilos.titulo}>{titulo}</Text>
      <Text style={estilos.texto}>{mensagem}</Text>
    </View>
  );
}

const estilos = StyleSheet.create({
  centro: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: espacos.grande },
  icone: { fontSize: 40, marginBottom: espacos.pequeno },
  titulo: { fontSize: fontes.grande, fontWeight: '700', color: cores.texto, marginBottom: espacos.pequeno, textAlign: 'center' },
  texto: { fontSize: fontes.normal, color: cores.textoSuave, textAlign: 'center', marginTop: espacos.pequeno },
  acao: { marginTop: espacos.grande, alignSelf: 'stretch' },
});
