import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { StatusBar } from 'expo-status-bar';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { Carregando } from './src/components/EstadoTela';
import { MoradorProvider, useMorador } from './src/context/MoradorContext';
import type { RootStackParamList } from './src/navigation/types';
import { DetalheScreen } from './src/screens/DetalheScreen';
import { IdentificacaoScreen } from './src/screens/IdentificacaoScreen';
import { NovaOcorrenciaScreen } from './src/screens/NovaOcorrenciaScreen';
import { OcorrenciasScreen } from './src/screens/OcorrenciasScreen';
import { cores } from './src/theme';

const Stack = createNativeStackNavigator<RootStackParamList>();

/** Sem morador salvo: só a tela de identificação existe. Com morador: o app inteiro. */
function Rotas() {
  const { morador, carregando } = useMorador();

  if (carregando) return <Carregando mensagem="Abrindo o ZelaBairro..." />;

  return (
    <Stack.Navigator
      screenOptions={{
        headerStyle: { backgroundColor: cores.primaria },
        headerTintColor: cores.textoSobrePrimaria,
        headerTitleStyle: { fontWeight: '700' },
        contentStyle: { backgroundColor: cores.fundo },
      }}
    >
      {morador === null ? (
        <Stack.Screen name="Identificacao" component={IdentificacaoScreen} options={{ title: 'ZelaBairro' }} />
      ) : (
        <>
          <Stack.Screen name="Ocorrencias" component={OcorrenciasScreen} options={{ title: 'ZelaBairro' }} />
          <Stack.Screen name="Detalhe" component={DetalheScreen} options={{ title: 'Ocorrência' }} />
          <Stack.Screen name="NovaOcorrencia" component={NovaOcorrenciaScreen} options={{ title: 'Registrar problema' }} />
        </>
      )}
    </Stack.Navigator>
  );
}

export default function App() {
  return (
    <SafeAreaProvider>
      <MoradorProvider>
        <NavigationContainer>
          <StatusBar style="light" />
          <Rotas />
        </NavigationContainer>
      </MoradorProvider>
    </SafeAreaProvider>
  );
}
