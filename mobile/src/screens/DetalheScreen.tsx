import { useCallback, useEffect, useState } from 'react';
import { Alert, Image, ScrollView, StyleSheet, Text, View } from 'react-native';
import { Botao } from '../components/Botao';
import { Carregando, ErroTela } from '../components/EstadoTela';
import { StatusBadge } from '../components/StatusBadge';
import type { ScreenProps } from '../navigation/types';
import { mensagemDeErro, urlFotoOcorrencia } from '../services/api';
import { confirmarOcorrencia, obterOcorrencia } from '../services/ocorrenciaService';
import { cores, espacos, fontes } from '../theme';
import type { EstadoCarga, Ocorrencia } from '../types/models';
import { formatarData, rotuloPrioridade } from '../utils/formatacao';

function Linha({ rotulo, valor }: { rotulo: string; valor: string }) {
  return (
    <View style={estilos.linha}>
      <Text style={estilos.rotulo}>{rotulo}</Text>
      <Text style={estilos.valor}>{valor}</Text>
    </View>
  );
}

export function DetalheScreen({ route }: ScreenProps<'Detalhe'>) {
  const { id } = route.params;
  const [ocorrencia, setOcorrencia] = useState<Ocorrencia | null>(null);
  const [estado, setEstado] = useState<EstadoCarga>('carregando');
  const [mensagemErro, setMensagemErro] = useState('');
  const [confirmando, setConfirmando] = useState(false);
  const [jaConfirmou, setJaConfirmou] = useState(false);
  const [fotoFalhou, setFotoFalhou] = useState(false);

  const carregar = useCallback(async () => {
    setEstado('carregando');
    try {
      setOcorrencia(await obterOcorrencia(id));
      setEstado('pronto');
    } catch (erro) {
      setMensagemErro(mensagemDeErro(erro));
      setEstado('erro');
    }
  }, [id]);

  useEffect(() => {
    void carregar();
  }, [carregar]);

  async function confirmar() {
    setConfirmando(true);
    try {
      setOcorrencia(await confirmarOcorrencia(id));
      setJaConfirmou(true);
      Alert.alert('Obrigado!', 'Sua confirmação ajuda a priorizar este problema.');
    } catch (erro) {
      Alert.alert('Não foi possível confirmar', mensagemDeErro(erro));
    } finally {
      setConfirmando(false);
    }
  }

  if (estado === 'carregando') return <Carregando mensagem="Abrindo ocorrência..." />;
  if (estado === 'erro' || ocorrencia === null) {
    return <ErroTela mensagem={mensagemErro || 'Ocorrência não encontrada.'} onTentarNovamente={() => void carregar()} />;
  }

  const encerrada = ocorrencia.status === 'Resolvida' || ocorrencia.status === 'Cancelada';
  const temCoordenadas = ocorrencia.latitude !== 0 || ocorrencia.longitude !== 0;

  return (
    <ScrollView contentContainerStyle={estilos.conteudo}>
      {ocorrencia.temFoto && !fotoFalhou ? (
        <Image
          source={{ uri: urlFotoOcorrencia(ocorrencia.id) }}
          style={estilos.foto}
          resizeMode="cover"
          accessibilityLabel={`Foto do problema: ${ocorrencia.titulo}`}
          onError={() => setFotoFalhou(true)}
        />
      ) : null}

      <StatusBadge status={ocorrencia.status} />
      <Text style={estilos.titulo}>{ocorrencia.titulo}</Text>
      {ocorrencia.descricao ? <Text style={estilos.descricao}>{ocorrencia.descricao}</Text> : null}

      {ocorrencia.atrasada ? (
        <View style={estilos.alerta}>
          <Text style={estilos.alertaTexto}>⚠ Este problema já passou do prazo de atendimento da categoria.</Text>
        </View>
      ) : null}

      <View style={estilos.cartao}>
        <Linha rotulo="Tipo" valor={ocorrencia.categoria} />
        <Linha rotulo="Bairro" valor={ocorrencia.bairro} />
        {ocorrencia.endereco ? <Linha rotulo="Endereço" valor={ocorrencia.endereco} /> : null}
        {temCoordenadas ? (
          <Linha rotulo="Localização" valor={`${ocorrencia.latitude.toFixed(5)}, ${ocorrencia.longitude.toFixed(5)}`} />
        ) : null}
        <Linha rotulo="Registrado em" valor={formatarData(ocorrencia.criadaEm)} />
        {ocorrencia.resolvidaEm ? <Linha rotulo="Resolvido em" valor={formatarData(ocorrencia.resolvidaEm)} /> : null}
        <Linha rotulo="Prioridade" valor={rotuloPrioridade(ocorrencia.prioridade)} />
        <Linha rotulo="Vizinhos que confirmaram" valor={String(ocorrencia.confirmacoes)} />
        <Linha rotulo="Origem" valor={ocorrencia.origem === 'Sensor' ? 'Sensor automático' : ocorrencia.origem === 'App' ? 'Aplicativo' : 'Subprefeitura'} />
      </View>

      {!encerrada ? (
        <Botao
          titulo={jaConfirmou ? 'Confirmação registrada ✓' : '👍 Eu também vi este problema'}
          variante="secundario"
          onPress={() => void confirmar()}
          carregando={confirmando}
          desabilitado={jaConfirmou}
        />
      ) : null}
    </ScrollView>
  );
}

const estilos = StyleSheet.create({
  conteudo: { padding: espacos.medio, paddingBottom: 48 },
  foto: { width: '100%', height: 220, borderRadius: 14, marginBottom: espacos.medio, backgroundColor: cores.borda },
  titulo: { fontSize: fontes.titulo, fontWeight: '800', color: cores.texto, marginTop: espacos.pequeno },
  descricao: { fontSize: fontes.normal, color: cores.texto, marginTop: espacos.pequeno, lineHeight: 22 },
  alerta: { backgroundColor: cores.alertaFundo, padding: 12, borderRadius: 10, marginTop: espacos.medio },
  alertaTexto: { color: cores.alerta, fontSize: fontes.normal, fontWeight: '600' },
  cartao: {
    backgroundColor: cores.superficie,
    borderRadius: 14,
    borderWidth: 1,
    borderColor: cores.borda,
    padding: espacos.medio,
    marginVertical: espacos.medio,
  },
  linha: { paddingVertical: 8, borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: cores.borda },
  rotulo: { fontSize: fontes.pequena, color: cores.textoSuave },
  valor: { fontSize: fontes.normal, color: cores.texto, fontWeight: '600', marginTop: 2 },
});
