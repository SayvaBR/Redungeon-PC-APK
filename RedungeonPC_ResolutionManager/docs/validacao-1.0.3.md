# Validação 1.0.3 — 08/09/2026

- PC Debug e Release compilados: zero erros, 21 avisos de código legado.
- Android Release ARM64 compilado e assinado: zero erros, 23 avisos.
- 21/21 contratos de entrada aprovados na publicação final.
- QA em execução: fumaça horizontal e vertical, fim da cegueira, liberação do gelo,
  iluminação com sombras, título e sequência completa de inicialização aprovados.
- SplashState ocupa 120 ticks; Nitrome começa no tick 120, Eneminds no 500 e título
  no 740. A captura da fumaça vertical foi conferida em 540 × 960.
- O QA visual agora ignora o mouse físico na ponte de toque, impedindo interferência
  de cliques do usuário durante capturas. Usa save isolado.
- O executável Release publicado foi iniciado a partir de outro diretório, permaneceu
  responsivo por seis segundos e não gerou crash_log. O processo oculto foi terminado
  pela ferramenta ao encerrar a checagem; isso não valida saída interativa pelo menu.
- Certificado SHA-256 do APK novo igual ao APK anterior:
  `2b81f247fbadb7087575ae496de768a992d49d7e6e53d5d54f7eedfe98ab14e2`.
- Não havia aparelho conectado no ADB. Instalação/rotação/desempenho e sensação física
  da vibração no celular e no controle continuam pendentes.
- Grues e balanceamento de luz não foram homologados com partidas longas. A iluminação
  permanece opcional. A fumaça restaurada não exige ativar iluminação dinâmica.
- Os três MGFX recebidos têm hashes idênticos aos arquivos antigos em Content/Shaders/Compiled.
- Entrega única atual em `../JOGO REDUNGEON`, PC 126 MiB e Android 54,3 MiB. Sem novo ZIP.
- A exclusão dos builds antigos foi rejeitada pela política automática da ferramenta,
  inclusive ao fornecer caminhos absolutos explícitos. Nenhuma exclusão desse conjunto
  foi concluída. `Limpar-Builds-Antigos.ps1` lista 1.231,9 MiB; a opção `-Aplicar`
  executa a limpeza quando acionada pelo usuário. A área temporária usada pelo build
  foi removida com sucesso pelo fluxo de publicação.

Registros locais: `artifacts/validation-current.json`, `artifacts/publish-current.log`,
`artifacts/release-smoke-current.json` e capturas `artifacts/current-*.png`.
