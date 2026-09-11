# Validação 1.0.4 — Lykos

Entrega atualizada em 9 de setembro de 2026, sem ZIP: JOGO REDUNGEON/PC e Android.

- Contratos: 25/25 aprovados. Incluem preço/níveis, relógio de transformação/recarga, reforço consumido uma vez e geometria de gelo nas duas orientações.
- QA real no executável MonoGame: 15 cenários aprovados, com saves isolados. Loja bloqueada e desbloqueada; compra deduzindo exatamente 100 e persistindo em disco; partida com WolfChar; habilidade acionada pelo input; nível 2; cinco esqueletos/reforço; expiração com restauração do tempo; mortes por impacto, queda, choque, fogo, esmagamento e flecha; retorno ao menu; ressurreição paga; gelo em retrato.
- Capturas e relatórios: artifacts/lykos no código. Retrato novo e esqueleto elétrico foram inspecionados depois da última importação. A morte por impacto foi repetida após corrigir o espelhamento.
- Build Debug PC: 0 erros, 21 avisos. Publish Release PC win-x64 autocontido: sucesso. Build Release Android ARM64: 0 erros, 23 avisos. Avisos existentes incluem APIs obsoletas e campos sem uso.
- Atlas da entrega PC tem SHA-256 idêntico ao fonte. APK assinado inclui lykos.png e lykos.json, versão Android 1.0.4 / código 5.

Limites: não havia Android conectado. Toque, rotação física, desempenho e sensação de vibração precisam ser confirmados no aparelho. A imagem de gelo foi validada em execução PC com viewport 540×960; isso não equivale a teste físico Android. O balanceamento foi validado mecanicamente, ainda pode ser refinado após partidas humanas.

A limpeza das builds antigas focus-build, pc-controls-win-x64, qa-ui e ui-tests (aproximadamente 398 MB) foi bloqueada pela revisão automática, sem motivo detalhado. Elas permanecem em artifacts. A área temporária C:/Dev/RedungeonBuild também foi preservada. A única entrega atual é JOGO REDUNGEON.
