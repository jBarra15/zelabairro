import * as ImagePicker from 'expo-image-picker';
import * as Location from 'expo-location';
import { useCallback, useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Image,
  KeyboardAvoidingView,
  Linking,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { Botao } from '../components/Botao';
import { Campo } from '../components/Campo';
import { Chip } from '../components/Chip';
import { Carregando, ErroTela } from '../components/EstadoTela';
import { useMorador } from '../context/MoradorContext';
import type { ScreenProps } from '../navigation/types';
import { mensagemDeErro } from '../services/api';
import { criarOcorrencia, enviarFoto } from '../services/ocorrenciaService';
import { listarCategorias } from '../services/referenciaService';
import { cores, espacos, fontes } from '../theme';
import type { Categoria, EstadoCarga } from '../types/models';
import { formularioValido } from '../validation/comum';
import { validarOcorrencia, type ErrosOcorrencia, type OcorrenciaForm } from '../validation/ocorrencia';

const FORM_INICIAL: OcorrenciaForm = {
  titulo: '',
  descricao: '',
  categoriaId: null,
  latitude: null,
  longitude: null,
  endereco: '',
};

function avisarPermissaoNegada(recurso: string, podePerguntarDeNovo: boolean) {
  if (podePerguntarDeNovo) {
    Alert.alert(`${recurso} não liberada`, 'Tudo bem! Você pode continuar o registro sem esse recurso.');
    return;
  }
  Alert.alert(
    `${recurso} bloqueada`,
    `Para usar, libere a permissão de ${recurso.toLowerCase()} nas configurações do celular.`,
    [
      { text: 'Agora não', style: 'cancel' },
      { text: 'Abrir configurações', onPress: () => void Linking.openSettings() },
    ],
  );
}

export function NovaOcorrenciaScreen({ navigation }: ScreenProps<'NovaOcorrencia'>) {
  const { morador } = useMorador();

  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [estado, setEstado] = useState<EstadoCarga>('carregando');
  const [erroCarga, setErroCarga] = useState('');

  const [form, setForm] = useState<OcorrenciaForm>(FORM_INICIAL);
  const [erros, setErros] = useState<ErrosOcorrencia>({});
  const [fotoUri, setFotoUri] = useState<string | null>(null);
  const [buscandoLocal, setBuscandoLocal] = useState(false);
  const [avisoLocal, setAvisoLocal] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);
  const [erroEnvio, setErroEnvio] = useState<string | null>(null);

  const carregarCategorias = useCallback(async () => {
    setEstado('carregando');
    try {
      setCategorias(await listarCategorias());
      setEstado('pronto');
    } catch (erro) {
      setErroCarga(mensagemDeErro(erro));
      setEstado('erro');
    }
  }, []);

  useEffect(() => {
    void carregarCategorias();
  }, [carregarCategorias]);

  // ---------- Recurso nativo 1: câmera ----------
  async function tirarFoto() {
    const permissao = await ImagePicker.requestCameraPermissionsAsync();
    if (!permissao.granted) {
      avisarPermissaoNegada('Câmera', permissao.canAskAgain);
      return;
    }
    const resultado = await ImagePicker.launchCameraAsync({ mediaTypes: ['images'], quality: 0.6 });
    if (!resultado.canceled && resultado.assets.length > 0) setFotoUri(resultado.assets[0].uri);
  }

  // ---------- Recurso nativo 2: localização ----------
  async function usarLocalizacao() {
    setBuscandoLocal(true);
    setAvisoLocal(null);
    try {
      const permissao = await Location.requestForegroundPermissionsAsync();
      if (!permissao.granted) {
        setAvisoLocal('Sem permissão de localização. Digite o endereço abaixo.');
        avisarPermissaoNegada('Localização', permissao.canAskAgain);
        return;
      }

      const posicao = await Location.getCurrentPositionAsync({ accuracy: Location.Accuracy.Balanced });
      const { latitude, longitude } = posicao.coords;
      setForm((f) => ({ ...f, latitude, longitude }));

      // Endereço aproximado é um bônus: se falhar, as coordenadas já bastam.
      try {
        const [endereco] = await Location.reverseGeocodeAsync({ latitude, longitude });
        if (endereco) {
          const texto = [endereco.street, endereco.streetNumber, endereco.district].filter(Boolean).join(', ');
          if (texto) setForm((f) => ({ ...f, endereco: f.endereco || texto }));
        }
      } catch {
        // sem endereço aproximado
      }
    } catch {
      setAvisoLocal('Não foi possível obter sua localização. Digite o endereço abaixo.');
    } finally {
      setBuscandoLocal(false);
    }
  }

  async function enviar() {
    if (!morador) return;
    const encontrados = validarOcorrencia(form);
    setErros(encontrados);
    if (!formularioValido(encontrados) || form.categoriaId === null) return;

    setEnviando(true);
    setErroEnvio(null);
    try {
      const criada = await criarOcorrencia({
        titulo: form.titulo.trim(),
        descricao: form.descricao.trim(),
        categoriaId: form.categoriaId,
        bairroId: morador.bairroId,
        moradorId: morador.id,
        latitude: form.latitude ?? 0,
        longitude: form.longitude ?? 0,
        endereco: form.endereco.trim() || null,
        origem: 'App',
      });

      if (fotoUri) {
        try {
          await enviarFoto(criada.id, fotoUri);
        } catch (erro) {
          Alert.alert('Ocorrência registrada', `Mas a foto não foi enviada: ${mensagemDeErro(erro)}`);
        }
      }

      navigation.replace('Detalhe', { id: criada.id });
    } catch (erro) {
      setErroEnvio(mensagemDeErro(erro));
      setEnviando(false);
    }
  }

  if (estado === 'carregando') return <Carregando mensagem="Preparando formulário..." />;
  if (estado === 'erro') return <ErroTela mensagem={erroCarga} onTentarNovamente={() => void carregarCategorias()} />;

  const temCoordenadas = form.latitude !== null && form.longitude !== null;

  return (
    <KeyboardAvoidingView style={estilos.flex} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView contentContainerStyle={estilos.conteudo} keyboardShouldPersistTaps="handled">
        <Text style={estilos.secao}>1. Qual é o problema?</Text>
        <View style={estilos.chips}>
          {categorias.map((categoria) => (
            <Chip
              key={categoria.id}
              rotulo={categoria.nome}
              selecionado={form.categoriaId === categoria.id}
              onPress={() => setForm((f) => ({ ...f, categoriaId: categoria.id }))}
            />
          ))}
        </View>
        {erros.categoriaId ? <Text style={estilos.erro}>{erros.categoriaId}</Text> : null}

        <View style={estilos.espaco} />
        <Campo
          rotulo="Título"
          placeholder="Ex.: Poste apagado na esquina"
          value={form.titulo}
          onChangeText={(titulo) => setForm((f) => ({ ...f, titulo }))}
          erro={erros.titulo}
          maxLength={100}
        />
        <Campo
          rotulo="Detalhes (opcional)"
          placeholder="Há quanto tempo? Atrapalha quem?"
          value={form.descricao}
          onChangeText={(descricao) => setForm((f) => ({ ...f, descricao }))}
          erro={erros.descricao}
          multiline
          numberOfLines={3}
          maxLength={500}
          style={estilos.multilinha}
        />

        <Text style={estilos.secao}>2. Onde?</Text>
        <Botao
          titulo={temCoordenadas ? '📍 Localização capturada ✓' : '📍 Usar minha localização'}
          variante="secundario"
          onPress={() => void usarLocalizacao()}
          carregando={buscandoLocal}
        />
        {buscandoLocal ? <ActivityIndicator style={estilos.indicador} color={cores.primaria} /> : null}
        {avisoLocal ? <Text style={estilos.aviso}>{avisoLocal}</Text> : null}
        <View style={estilos.espaco} />
        <Campo
          rotulo="Endereço ou ponto de referência"
          placeholder="Ex.: Rua X, em frente à padaria"
          value={form.endereco}
          onChangeText={(endereco) => setForm((f) => ({ ...f, endereco }))}
          erro={erros.local}
          maxLength={200}
        />

        <Text style={estilos.secao}>3. Foto (opcional)</Text>
        {fotoUri ? <Image source={{ uri: fotoUri }} style={estilos.foto} accessibilityLabel="Prévia da foto" /> : null}
        <Botao
          titulo={fotoUri ? '📷 Tirar outra foto' : '📷 Fotografar o problema'}
          variante="secundario"
          onPress={() => void tirarFoto()}
        />
        <Text style={estilos.dica}>Evite fotografar rostos ou placas de carro.</Text>

        {erroEnvio ? <Text style={[estilos.erro, estilos.erroEnvio]}>{erroEnvio}</Text> : null}

        <View style={estilos.acao}>
          <Botao titulo="Enviar ocorrência" onPress={() => void enviar()} carregando={enviando} />
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const estilos = StyleSheet.create({
  flex: { flex: 1 },
  conteudo: { padding: espacos.medio, paddingBottom: 48 },
  secao: { fontSize: fontes.grande, fontWeight: '700', color: cores.texto, marginTop: espacos.medio, marginBottom: espacos.pequeno },
  chips: { flexDirection: 'row', flexWrap: 'wrap' },
  espaco: { height: espacos.medio },
  multilinha: { minHeight: 90, textAlignVertical: 'top' },
  erro: { color: cores.erro, fontSize: fontes.pequena, marginTop: 4 },
  erroEnvio: { marginTop: espacos.medio, fontSize: fontes.normal },
  aviso: { color: cores.alerta, fontSize: fontes.pequena, marginTop: espacos.pequeno },
  indicador: { marginTop: espacos.pequeno },
  foto: { width: '100%', height: 200, borderRadius: 12, marginBottom: espacos.pequeno },
  dica: { fontSize: fontes.pequena, color: cores.textoSuave, marginTop: 6 },
  acao: { marginTop: espacos.grande },
});
