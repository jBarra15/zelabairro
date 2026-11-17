import { useCallback, useEffect, useState } from 'react';
import { KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, View } from 'react-native';
import { Botao } from '../components/Botao';
import { Campo } from '../components/Campo';
import { Chip } from '../components/Chip';
import { Carregando, ErroTela } from '../components/EstadoTela';
import { useMorador } from '../context/MoradorContext';
import { mensagemDeErro } from '../services/api';
import { identificarMorador } from '../services/moradorService';
import { listarBairros } from '../services/referenciaService';
import { cores, espacos, fontes } from '../theme';
import type { Bairro, EstadoCarga } from '../types/models';
import { formularioValido } from '../validation/comum';
import { validarIdentificacao, type ErrosIdentificacao, type IdentificacaoForm } from '../validation/identificacao';

export function IdentificacaoScreen() {
  const { entrar } = useMorador();

  const [bairros, setBairros] = useState<Bairro[]>([]);
  const [estado, setEstado] = useState<EstadoCarga>('carregando');
  const [erroCarga, setErroCarga] = useState('');

  const [form, setForm] = useState<IdentificacaoForm>({ nome: '', email: '', bairroId: null });
  const [erros, setErros] = useState<ErrosIdentificacao>({});
  const [enviando, setEnviando] = useState(false);
  const [erroEnvio, setErroEnvio] = useState<string | null>(null);

  const carregarBairros = useCallback(async () => {
    setEstado('carregando');
    try {
      setBairros(await listarBairros());
      setEstado('pronto');
    } catch (erro) {
      setErroCarga(mensagemDeErro(erro));
      setEstado('erro');
    }
  }, []);

  useEffect(() => {
    void carregarBairros();
  }, [carregarBairros]);

  async function enviar() {
    const encontrados = validarIdentificacao(form);
    setErros(encontrados);
    if (!formularioValido(encontrados) || form.bairroId === null) return;

    setEnviando(true);
    setErroEnvio(null);
    try {
      const morador = await identificarMorador({
        nome: form.nome.trim(),
        email: form.email.trim(),
        bairroId: form.bairroId,
      });
      await entrar(morador); // a navegação troca de tela sozinha quando o morador existe
    } catch (erro) {
      setErroEnvio(mensagemDeErro(erro));
      setEnviando(false);
    }
  }

  if (estado === 'carregando') return <Carregando mensagem="Buscando bairros..." />;
  if (estado === 'erro') return <ErroTela mensagem={erroCarga} onTentarNovamente={() => void carregarBairros()} />;

  return (
    <KeyboardAvoidingView style={estilos.flex} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView contentContainerStyle={estilos.conteudo} keyboardShouldPersistTaps="handled">
        <Text style={estilos.titulo}>Cuide do seu bairro</Text>
        <Text style={estilos.subtitulo}>
          Registre problemas como lixo, poste apagado ou buraco e acompanhe até serem resolvidos.
        </Text>

        <Campo
          rotulo="Seu nome"
          placeholder="Ex.: Maria"
          value={form.nome}
          onChangeText={(nome) => setForm((f) => ({ ...f, nome }))}
          erro={erros.nome}
          autoComplete="name"
          maxLength={80}
        />
        <Campo
          rotulo="Seu e-mail"
          placeholder="nome@exemplo.com"
          value={form.email}
          onChangeText={(email) => setForm((f) => ({ ...f, email }))}
          erro={erros.email}
          keyboardType="email-address"
          autoCapitalize="none"
          autoComplete="email"
          maxLength={120}
        />

        <Text style={estilos.rotulo}>Seu bairro</Text>
        <View style={estilos.chips}>
          {bairros.map((bairro) => (
            <Chip
              key={bairro.id}
              rotulo={bairro.nome}
              selecionado={form.bairroId === bairro.id}
              onPress={() => setForm((f) => ({ ...f, bairroId: bairro.id }))}
            />
          ))}
        </View>
        {erros.bairroId ? <Text style={estilos.erro}>{erros.bairroId}</Text> : null}

        {erroEnvio ? <Text style={[estilos.erro, estilos.erroEnvio]}>{erroEnvio}</Text> : null}

        <View style={estilos.acao}>
          <Botao titulo="Entrar" onPress={() => void enviar()} carregando={enviando} />
        </View>
        <Text style={estilos.aviso}>Usamos seu e-mail só para identificar suas ocorrências.</Text>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const estilos = StyleSheet.create({
  flex: { flex: 1 },
  conteudo: { padding: espacos.grande, paddingBottom: 48 },
  titulo: { fontSize: fontes.titulo, fontWeight: '800', color: cores.primaria },
  subtitulo: { fontSize: fontes.normal, color: cores.textoSuave, marginTop: espacos.pequeno, marginBottom: espacos.grande },
  rotulo: { fontSize: fontes.normal, fontWeight: '600', color: cores.texto, marginBottom: 8 },
  chips: { flexDirection: 'row', flexWrap: 'wrap' },
  erro: { color: cores.erro, fontSize: fontes.pequena, marginTop: 4 },
  erroEnvio: { marginTop: espacos.medio, fontSize: fontes.normal },
  acao: { marginTop: espacos.grande },
  aviso: { fontSize: fontes.pequena, color: cores.textoSuave, textAlign: 'center', marginTop: espacos.medio },
});
