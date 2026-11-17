# ZelaBairro — Modelo de prioridade (Inteligência Artificial & Machine Learning)

**Integrante:** Joel — RM 560796 — 3ESPS

## O problema
A subprefeitura recebe ocorrências do app, de sensores e do balcão, e o operador precisa decidir **o que atender primeiro**. O modelo prevê a **prioridade (Baixa, Média ou Alta)** de uma ocorrência nova, aprendendo com triagens anteriores. A previsão aparece como **sugestão** no terminal da subprefeitura; quem decide é o operador.

## Dados e origem
- **Simulados** (o regulamento proíbe dados pessoais reais, e o projeto ainda não tem histórico): 3.000 ocorrências triadas, geradas no próprio notebook com **semente fixa (42)**, e salvas em `data/ocorrencias_historico.csv`.
- Premissas documentadas no notebook (seção 2): volume por categoria, prazos iguais aos da API, sensores só para lixo e iluminação, e a decisão do operador com **interações e ruído**.
- O **bairro não é usado** como variável, para o modelo não aprender a priorizar um bairro em vez de outro (ODS 10).

## Decisões
| Decisão | Por quê |
|---|---|
| Classificação multiclasse | Saída é uma das 3 prioridades usadas pela API |
| **F1-macro** como métrica principal (+ recall da classe Alta) | Classes desbalanceadas; errar um caso Alta é o pior erro, e a acurácia esconderia isso |
| Variável criada `uso_prazo` = dias em aberto ÷ prazo | Torna o atraso comparável entre categorias com prazos diferentes |
| Só variáveis que a API já fornece | O terminal C# precisa calcular as mesmas entradas |
| Split estratificado 80/20 + validação cruzada em 5 dobras | Proporção de classes preservada; teste usado só no final |
| Comparação com **a regra fixa que o terminal usava** | O modelo só vale a pena se superar o que já existe |
| **Regressão logística** (escolhida) | Melhor F1 na validação cruzada, probabilidades graduais, pesos explicáveis e exportável para C# em um JSON pequeno |

## Resultados (conjunto de teste, 600 ocorrências)
| | Regra atual | Árvore de decisão | **Regressão logística** |
|---|---|---|---|
| F1-macro | 0,608 | 0,718 | **0,761** |
| Acurácia | 0,597 | 0,710 | **0,757** |
| Recall da classe Alta | 0,795 | 0,803 | **0,886** |

## Integração na solução
O notebook exporta `modelo/modelo_prioridade.json` (médias, desvios, pesos e interceptos). O **terminal WinForms** (`desktop/ZelaBairro.Desktop/Services/ModeloPrioridade.cs`) carrega esse arquivo e mostra, para cada ocorrência, **"Previsão do modelo: Alta — 93% de confiança"** e os fatores que mais pesaram, com um botão para aplicar a previsão. O notebook verifica que a conta feita a partir do JSON dá **100% das mesmas previsões** do scikit-learn.

## Como rodar
```bash
cd ml
pip install -r requirements.txt
jupyter notebook ZelaBairro_Prioridade.ipynb
```
Execute **Kernel → Restart & Run All**. Ou envie o `.ipynb` para o **Google Colab** e rode tudo.

Depois de re-treinar, copie `modelo/modelo_prioridade.json` para `desktop/ZelaBairro.Desktop/`.

## Limites
Dados simulados: os números provam o processo, não o desempenho real. Com o sistema em uso, o mesmo notebook deve ser re-treinado com as triagens reais exportadas da API.
