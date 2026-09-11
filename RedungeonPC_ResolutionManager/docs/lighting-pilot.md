# Iluminação 2D: primeiro teste

## Plano e implementação

1. Substituir o fade recortado da sombra perseguidora por máscara preta
   procedural, com alpha suave e repetição por toda a largura visível.
   Preservar os sprites dos grues e as regras de morte.
2. Reutilizar as fontes do LightManager e o render target de luz já existente.
   Compor luz colorida por pixel, com queda radial, antes dos pós-efeitos e HUD.
   O preto continua preto; ambiente a aproximadamente 85% dá contraste às
   fontes, com alcance ampliado em 60% e intensidade local mais perceptível.
3. Oferecer Vídeo > Iluminação dinâmica, inicialmente desligada, persistida
   junto das opções. Restaurar padrões desliga o efeito.
4. Compilar e executar contratos; validar visualmente no jogo antes de expandir.

As duas máscaras de 128x128 são criadas uma vez e descartadas no Unload.
Não há alocação de render targets por frame. Fontes fora da tela são ignoradas.
O efeito não implementa ray tracing, reflexão, normal maps ou oclusão por paredes.

## Aprovação visual pendente

- Comparar uma tocha com a opção ligada/desligada: piso e personagem próximos
  recebem luz; a interface mantém suas cores.
- Aproximar a sombra dos grues: fade contínuo sem quadrados ou emendas,
  inclusive em 4:3 e ultrawide. Conferir a distância visual de perigo.
- Redimensionar, pausar, morrer e reiniciar com luz ligada.
- Conferir a composição com veneno, gelo e cegueira.
- Medir tempo de frame com várias fontes antes de ativar por padrão.

Build e contratos não substituem estas verificações visuais.
