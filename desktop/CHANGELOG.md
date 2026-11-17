# Changelog — ZelaBairro Desktop
Formato: [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) · Versionamento: [SemVer](https://semver.org/lang/pt-BR/)

## [1.1.0] — 2026-11-16
### Adicionado
- Previsão de prioridade pelo modelo de Machine Learning (`ModeloPrioridade`, lê `modelo_prioridade.json`), com confiança e fatores.
- Botão "Usar previsão do modelo"; o histórico registra quando a previsão do ML foi aplicada.
### Alterado
- A regra de pontos passa a ser exibida como referência, abaixo da previsão do modelo.
### Corrigido
- Descrição vazia no cadastro de balcão (a API passou a aceitar descrição nula no Oracle).

## [1.0.0] — 2026-11-16
### Adicionado
- Fila de triagem com filtros por bairro e status, ordenada por atraso e prioridade.
- Indicadores: total, abertas, em atendimento, fora do prazo e % resolvidas no prazo.
- Sugestão de prioridade com explicação dos pontos (`Priorizador`).
- Ações de atendimento: iniciar, resolver, cancelar e definir prioridade.
- Cadastro de ocorrência no balcão/telefone com validação.
- Histórico local de atendimento em JSON e modo offline com cache.
- Exibição da foto enviada pelo morador.
