# Validação PC 1.0.5 — Lykos

Esta rodada atualiza somente `JOGO REDUNGEON/PC`. Android não foi recompilado.

- Compra e seleção: preço de 100 moedas, save persistente, retrato e nome próprios.
- Arte: 32 frames de caminhada alinhados pela linha dos pés; resíduos claros externos removidos no importador; quatro direções nas formas humana e lupina.
- Movimento: cadência baseada em Knight, Bragg e Panic Bot; forma humana 0,095 e forma lupina 0,11.
- Gameplay: ativação pelo comando real, troca visual confirmada para o atlas lupino, duração/recarga, transformação com lua e uivo, sombra visível e contador no canto superior esquerdo.
- Mordida: seguidores esqueleto e serpentes esqueleto são destruídos, contam uma vez e alimentam o reforço. Dragões chineses são excluídos.
- Equilíbrio: Lua Cheia neutraliza espinhos, serras, esmagamento, flechas, machados, fogo e choque. Morcegos, slimes, seguidores, feitiços, escuridão, buracos e teias continuam perigosos.
- Morte/retorno: impacto, queda, choque, fogo, esmagamento, flecha, retorno ao menu e ressurreição já possuem cenários de QA.

O WAV `lykos_howl.wav` é original e reproduzível por `tools/generate_lykos_howl.py`. O pipeline MonoGame o compila como efeito de som para o PC.

Capturas e relatórios ficam em `artifacts/lykos`; não são versões do jogo.
