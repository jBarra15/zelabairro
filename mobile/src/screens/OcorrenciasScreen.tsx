import { useFocusEffect } from '@react-navigation/native';
import { useCallback, useLayoutEffect, useState } from 'react';
import { Alert, FlatList, Pressable, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { Botao } from '../components/Botao';
import { Chip } from '../components/Chip';
import { Carregando, ErroTela, Vazio } from '../components/EstadoTela';
import { OcorrenciaCard } from '../components/OcorrenciaCard';
import { useMorador } from '../context/MoradorContext';
import type { ScreenProps } from '../navigation/types';
import { mensagemDeErro } from '../services/api';
import { listarOcorrencias } from '../services/ocorrenciaService';
import { cores, espacos, fontes } from '../theme';
import type { EstadoCarga, Ocorrencia } from '../types/models';

type Filtro = 'bairro' | 'minhas';

export function OcorrenciasScreen({ navigation }: ScreenProps<'Ocorrencias'>) {
  const { morador, sair } = useMorador();
  const [filtro, setFiltro] = useState<Filtro>('bairro');
  const [ocorrencias, setOcorrencias] = useState<Ocorrencia[]>([]);
  const [estado, setEstado] = useState<EstadoCarga>('carregando');
  const [mensagemErro, setMensagemErro] = useState('');
  const [atualizando, setAtualizando] = useState(false);

  const carregar = useCallback(
    async (modo: 'inicial' | 'atualizar') => {
      if (!morador) return;
      if (modo === 'inicial') setEstado('carregando');
      else setAtualizando(true);

      try {
        const lista = await listarOcorrencias(
          filtro === 'bairro' ? { bairroId: morador.bairroId } : { moradorId: morador.id },
        );
        setOcorrencias(lista);
        setEstado('pronto');
      } catch (erro) {
        setMensagemErro(mensagemDeErro(erro));
        setEstado('erro');
      } finally {
        setAtualizando(false);
      }
    },
    [morador, filtro],
  );

  // Recarrega sempre que a tela volta ao foco (ex.: depois de registrar uma ocorrência).
  useFocusEffect(
    useCallback(() => {
      void carregar('inicial');
    }, [carregar]),
  );

  const confirmarSaida = useCallback(() => {
    Alert.alert('Sair', 'Deseja trocar de morador neste celular?', [
      { text: 'Cancelar', style: 'cancel' },
      { text: 'Sair', style: 'destructive', onPress: () => void sair() },
    ]);
  }, [sair]);

  useLayoutEffect(() => {
    navigation.setOptions({
      headerRight: () => (
        <Pressable accessibilityRole="button" accessibilityLabel="Sair" onPress={confirmarSaida} hitSlop={12}>
          <Text style={estilos.sair}>Sair</Text>
        </Pressable>
      ),
    });
  }, [navigation, confirmarSaida]);

  const primeiroNome = morador?.nome.split(' ')[0] ?? '';

  function renderConteudo() {
    if (estado === 'carregando') return <Carregando mensagem="Buscando ocorrências..." />;
    if (estado === 'erro') return <ErroTela mensagem={mensagemErro} onTentarNovamente={() => void carregar('inicial')} />;

    return (
      <FlatList
        data={ocorrencias}
        keyExtractor={(item) => String(item.id)}
        renderItem={({ item }) => (
          <OcorrenciaCard ocorrencia={item} onPress={() => navigation.navigate('Detalhe', { id: item.id })} />
        )}
        contentContainerStyle={ocorrencias.length === 0 ? estilos.listaVazia : estilos.lista}
        ListEmptyComponent={
          <Vazio
            titulo={filtro === 'bairro' ? 'Nenhum problema registrado' : 'Você ainda não registrou nada'}
            mensagem="Viu lixo acumulado, poste apagado ou buraco? Toque em “Registrar problema”."
          />
        }
        refreshControl={
          <RefreshControl refreshing={atualizando} onRefresh={() => void carregar('atualizar')} colors={[cores.primaria]} />
        }
      />
    );
  }

  return (
    <View style={estilos.container}>
      <View style={estilos.cabecalho}>
        <Text style={estilos.saudacao}>Olá, {primeiroNome}!</Text>
        <View style={estilos.filtros}>
          <Chip rotulo="Do meu bairro" selecionado={filtro === 'bairro'} onPress={() => setFiltro('bairro')} />
          <Chip rotulo="Minhas" selecionado={filtro === 'minhas'} onPress={() => setFiltro('minhas')} />
        </View>
      </View>

      <View style={estilos.flex}>{renderConteudo()}</View>

      <View style={estilos.rodape}>
        <Botao titulo="+ Registrar problema" onPress={() => navigation.navigate('NovaOcorrencia')} />
      </View>
    </View>
  );
}

const estilos = StyleSheet.create({
  container: { flex: 1, backgroundColor: cores.fundo },
  flex: { flex: 1 },
  cabecalho: { paddingHorizontal: espacos.medio, paddingTop: espacos.medio },
  saudacao: { fontSize: fontes.grande, fontWeight: '700', color: cores.texto, marginBottom: espacos.pequeno },
  filtros: { flexDirection: 'row', flexWrap: 'wrap' },
  lista: { padding: espacos.medio },
  listaVazia: { flexGrow: 1 },
  rodape: { padding: espacos.medio, borderTopWidth: 1, borderTopColor: cores.borda, backgroundColor: cores.superficie },
  sair: { color: cores.textoSobrePrimaria, fontSize: fontes.normal, fontWeight: '700' },
});
