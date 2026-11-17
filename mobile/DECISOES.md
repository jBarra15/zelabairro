# Decisões técnicas — App ZelaBairro

## 1. Expo + React Navigation (native-stack)
O app roda no **Expo Go**, sem build nativo, o que permite testar no celular real em minutos. Escolhi o *native-stack* do React Navigation porque usa a navegação nativa de cada plataforma (gestos e animações do Android/iOS) e tem tipagem forte das rotas (`RootStackParamList`): navegar para `Detalhe` sem `id` é erro de compilação.

## 2. Rotas condicionais para a identificação
Em vez de redirecionar manualmente, o navegador decide quais telas existem: **sem morador salvo, só existe a tela de Identificação**; com morador, existe o app inteiro. Isso impede voltar para telas internas depois de sair e elimina estados inconsistentes. A identificação fica em um **Context** (`MoradorContext`) e é persistida com **AsyncStorage**, então o app lembra do morador ao reabrir. Ao ler o armazenamento, um *type guard* (`ehMorador`) valida o dado, para que um JSON corrompido não quebre o app.

**Por que só nome, e-mail e bairro, sem senha?** O briefing pede uma solução simples para o cidadão comum. A API identifica pelo e-mail (cadastra ou atualiza). Em produção, o próximo passo seria um código de verificação por e-mail; registrei isso como limitação conhecida, tratada no relatório de Cybersecurity.

## 3. Camada `services/`
Nenhuma tela chama `fetch` diretamente. `services/api.ts` centraliza: URL base, **timeout de 10 s** (com `AbortController`), JSON, e a conversão dos dois formatos de erro da API (`{ erro }` das regras de negócio e `{ errors: {...} }` da validação automática do ASP.NET) em **uma mensagem legível** (`ApiError`). Assim as telas só lidam com sucesso ou com uma frase pronta para mostrar.

## 4. Validação pura
`validation/` contém funções que recebem o formulário e devolvem `{ campo: mensagem }`, sem React, rede ou estado. Isso as torna testáveis isoladamente e reaproveitáveis. As regras **espelham as do backend** (título 3–100, descrição até 500, e-mail válido), então o usuário recebe o erro antes de gastar dados móveis; o backend continua validando por segurança.

## 5. Estados de lista explícitos
Cada tela que carrega dados usa um tipo `EstadoCarga = 'carregando' | 'erro' | 'pronto'`, e a lista trata o vazio com `ListEmptyComponent`. O erro sempre oferece **"Tentar novamente"**, e a lista tem *puxar para atualizar*. A lista recarrega ao voltar ao foco (`useFocusEffect`), então uma ocorrência recém-criada aparece sem ação do usuário.

## 6. Recursos nativos: câmera e localização
- **Câmera** (`expo-image-picker`): a foto é a evidência do problema. Qualidade reduzida (0.6) para economizar dados e caber no limite de 5 MB da API.
- **Localização** (`expo-location`): marca onde está o problema, com tentativa de endereço aproximado.

**Permissão negada não bloqueia o fluxo** (requisito "relevante fora do cenário ideal"): sem localização, o morador digita o endereço; sem câmera, envia sem foto. Se a permissão foi negada de forma permanente, o app explica e oferece **abrir as Configurações**. Quando não há coordenadas, enviamos `0,0` e o endereço digitado; a tela de detalhe oculta coordenadas `0,0`.

A foto é enviada em **uma segunda requisição** (multipart) depois de criar a ocorrência. Se o upload falhar (rede fraca), a ocorrência não se perde: o morador é avisado e o registro continua válido.

## 7. TypeScript estrito
`strict: true`, **nenhum `any`**: dados externos (resposta da API, JSON do armazenamento) entram como `unknown` e são convertidos com tipagem explícita ou *type guards*. `npm run typecheck` (`tsc --noEmit`) passa sem erros.

## 8. Acessibilidade e o cidadão real
Fontes de no mínimo 14 (texto principal 16), cores com contraste AA, alvos de toque de 44–52 px, `accessibilityLabel`/`accessibilityRole` nos elementos interativos e linguagem simples ("Registrar problema", "Eu também vi este problema"). A tela de foto lembra de não fotografar rostos ou placas, alinhado à regra de dados pessoais.

## 9. Datas
A API devolve datas UTC às vezes sem o sufixo `Z` e com 7 casas de fração de segundo, o que faz o JavaScript errar o fuso em 3 horas. `utils/formatacao.ts` normaliza antes de exibir.

## Limitações conhecidas
- Identificação por e-mail sem verificação (ver Cybersecurity).
- A URL da API é fixa em `config.ts`; em produção viria de variável de ambiente com HTTPS.
