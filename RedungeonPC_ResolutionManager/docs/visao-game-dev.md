# Visão de evolução do Redungeon

O ponto forte é a combinação de silhuetas reconhecíveis, decisões rápidas e animação
expressiva em poucos pixels. Eu elevaria a resposta dos controles, a leitura dos
perigos e a atmosfera mantendo essa identidade. O maior risco visual é iluminar
tudo por igual ou exagerar no blur e perder a clareza dos tiles.

## Primeiro: fechar o clássico

- Testar a vibração física no controle do usuário, por USB e Bluetooth, e no celular.
  Confirmar 0%, intensidade intermediária, 100%, impacto, prévia e suspensão do app.
- Validar Android instalado, retomada, rotação durante jogo/pausa e sessões longas.
  Compilar o APK não mede estabilidade térmica, consumo ou tempo de frame no aparelho.
- Comparar visualmente distância de morte e borda da sombra dos grues. O perigo
  precisa ser percebido antes de matar; brilho decorativo não deve sugerir segurança.
- Medir antes de ativar iluminação por padrão: 16,67 ms por frame a 60 Hz, incluindo
  picos e percentis, sem usar somente FPS médio. Perfis separados para PC e celular.
- Revisar progressão do combo e alcance da espada com partidas completas. O código
  já inclui essas mudanças anteriores; elas precisam de balanceamento, não de mais
  efeitos para esconder problemas de ritmo.

## Salto gráfico com melhor retorno

1. **Materiais iluminados em 2D.** Normal maps desenhados para paredes, metal e chão,
   além de máscara emissiva para fogo, olhos, wisps e cristais. Começar por uma sala:
   uma tocha deve revelar o relevo próximo sem alterar a silhueta pixelada.
2. **Bloom seletivo real.** Hoje `LightBloom` adiciona halos nas fontes de luz.
   Um bloom extraído de emissivos, com limiar e blur em resolução reduzida, permite
   brilho consistente sem borrar texto, moedas e a arte inteira. Compor antes do HUD.
3. **Sombras e oclusão.** O código atual usa raios e três amostras por luz na CPU.
   Otimizar com cache de paredes e invalidar quando luz/obstáculo se move; reduzir
   frequência de atualização de luzes distantes. Aumentar qualidade só depois de medir.
4. **Profundidade atmosférica.** Poeira localizada nos feixes, névoa em camadas lentas,
   pequenas variações de cor por ambiente e bordas de abismo mais legíveis. Manter
   as áreas de caminhada e os sinais de ataque livres de ruído visual.
5. **Resposta a ações.** Impactos com pausa de poucos frames, som/material específico,
   partículas direcionais e padrões táteis curtos e distintos para gelo, escudo e dano.
   Intensidades reguláveis, evitando tremer a câmera em cada moeda.

## Extrair mais do MonoGame

MonoGame permite implementar esses passes; a qualidade virá da arte, dos shaders e
do orçamento de cada cena. Não existe um botão que transforme o port em um renderizador
moderno. Eu manteria o mundo em baixa resolução com PointClamp, o mapa de luz separado
e filtrado, e a interface desenhada depois dos efeitos. Reutilizaria render targets,
buffers e partículas, mediria alocações por frame e organizaria as fontes em lotes.

O renderer já limita a 24 luzes visíveis e usa máscaras reutilizadas. O passo seguinte
é conhecer custo real de sombras por quantidade de obstáculos e evitar cálculos
repetidos quando nada mudou. No celular, reduzir a resolução do mapa de luz e a
quantidade de luzes com sombra oferece mais controle que degradar toda a imagem.

Não priorizaria ray tracing, iluminação global pesada ou texturas suavizadas. Minha
primeira entrega futura seria uma única sala de referência incorporada ao jogo atual:
tocha, parede, metal, wisp, gelo e sombra dos grues. Comparar ligada/desligada e medir
no PC e no telefone; aprovada a combinação, expandi-la ao restante do clássico.

## Disciplina de entrega

Uma base de código e uma pasta `JOGO REDUNGEON`, com um executável PC e um APK atual.
QA é uma forma de executar a mesma base, com save isolado e capturas pequenas; não
precisa produzir distribuições paralelas. `Atualizar-Jogo.ps1` atualiza essa pasta
e remove a área temporária de compilação quando conclui com sucesso.
