# Validação PC 1.0.6 — Lykos e progressão

Esta rodada atualiza somente `JOGO REDUNGEON/PC`. Android não foi compilado.

- Build Debug e Release: 0 erros; 21 avisos preexistentes do projeto.
- Contratos automatizados: 25/25 aprovados.
- Gelo e mímico: o deslizamento executa a interação, destrói o mímico,
  contabiliza um osso, para o movimento e impede sobreposição de pisos.
- Buraco: Lykos transformado entra no estado de queda e morre normalmente.
- Teia: Lykos fica preso nas duas formas e não ativa Lua Cheia enquanto preso.
- Equilíbrio: buracos, teias, inimigos, feitiços e escuridão continuam perigosos.
- Progressão: todos os personagens são carregados desbloqueados no maior nível
  definido por cada personagem, sem alterar moedas, recordes ou estatísticas.
- Regressão: transformação, lua, queda humana e consumo de esqueletos continuam
  aprovados nos cenários do executável.

Capturas e relatórios ficam em `artifacts/lykos`; não são versões do jogo.
