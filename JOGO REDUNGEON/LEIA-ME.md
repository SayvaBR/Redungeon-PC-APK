# Redungeon — entrega PC 1.0.7

PC: abra `PC/RedungeonPC.exe`. O runtime está incluído, sem instalação do .NET.
Esta atualização é somente para PC e substitui os arquivos de `PC`. Não há ZIP.
O Android anterior não foi recompilado nem faz parte desta atualização.

Lykos agora usa arte própria no seletor e na partida, com humano/lobo, quatro
direções animadas, efeitos de morte, queda, choque, fogo e ressurreição.
O desbloqueio custa 100 moedas. Nível 1: mordida permanente contra esqueletos.
Nível 2 (melhoria de 150): Lua Cheia, 3 segundos e recarga de 20 segundos.
Nível 3 (250): 5 segundos, recarga de 15 segundos e mundo lento.
Nível 4 (400): comer cinco esqueletos prepara +2 segundos para a próxima Lua Cheia.
A transformação usa o comando normal de habilidade (Espaço no teclado), sem gastar
moedas. A escuridão perseguidora continua perigosa. A recarga começa ao terminar
a transformação. Há estatística de ossos e conquista ao comer 50 esqueletos.

O gelo em retrato agora preserva a proporção das texturas, concentra a moldura nos
cantos e deixa o centro livre. A intensidade do bloom continua ajustável em Vídeo.
O guia técnico para futuros personagens está em docs/creating-characters.md no código.

Ajustes 1.0.5: remoção dos pixels claros residuais nas bordas do sprite; cadência
de caminhada alinhada a Knight, Bragg e Panic Bot; sombra própria visível; contador
de ossos transferido para o canto superior esquerdo; nome sem corte e textos com
acentos. Serpentes esqueleto agora são consumidas e contam um osso. Lua Cheia ganhou
uma transformação curta com lua ascendente, clarão e uivo original. Para equilíbrio,
ela neutraliza armadilhas físicas, mas inimigos, feitiços e escuridão ainda ferem.

Ajustes 1.0.6: o deslizamento no gelo agora para no baú mímico, aciona a
mordida e impede Lykos de ocupar o mesmo piso. A forma lupina cai em buracos,
fica presa em teias e não pode iniciar a transformação enquanto estiver presa.
Todos os personagens carregam desbloqueados e no nível máximo nesta edição.

Ajustes 1.0.7: os wisps receberam halo pulsante, projeção colorida no piso e
luz dinâmica mais intensa. O reforço pode ser ligado ou desligado separadamente
em `Configurações > Vídeo > Brilho intenso dos wisps` e fica salvo.

O código permanece em `RedungeonPC_ResolutionManager`. Execute `Atualizar-Jogo.ps1`
nessa pasta para atualizar a entrega. A área temporária sem acentos necessária ao
compilador de shaders é removida ao concluir. Logs e capturas não são versões do jogo.

Mudanças: splash do capacete por dois segundos antes da Nitrome; fumaça roxa com
duas camadas animadas, abertura ondulante seguindo o jogador e transição gradual;
vibração nativa no Android; suporte SDL a vibração Bluetooth DualShock/DualSense;
prévia de vibração de 300 ms ao ajustar a intensidade em Configurações > Controles.
Em 0% não vibra. Ao perder foco ou ir para segundo plano os motores são desligados.

Os três MGFX enviados são idênticos aos binários antigos arquivados no projeto.
O efeito de fumaça foi reconstruído a partir do GLSL desses arquivos e compilado
para os backends atuais. Os binários antigos não são carregados diretamente.
A iluminação dinâmica, sombras suaves, bloom e gelo existentes foram preservados.

O jogo utiliza suas fontes bitmap existentes. A página de referência Eneminds Bold
informa download desativado: https://fontstruct.com/fontstructions/show/1423872/eneminds-bold

O save do PC permanece em `%APPDATA%/RedungeonPC/save.dat`; no Android permanece
no armazenamento privado do aplicativo. Instale o APK como atualização para conservar
o progresso. A assinatura usa a mesma chave de desenvolvimento local do port anterior.

Validação física ainda necessária: vibração com seu controle conectado e no celular,
desempenho e rotação no aparelho. Compilação e QA de PC não comprovam sensação tátil
nem o comportamento de todos os drivers. No Bluetooth PS4/PS5, o SDL usa relatórios
estendidos; desligar e religar o controle restaura o modo anterior para apps DirectInput.

Referências técnicas:
- https://docs.monogame.net/api/Microsoft.Xna.Framework.Input.GamePad.html
- https://learn.microsoft.com/en-us/dotnet/api/android.os.vibrator.vibrate
- https://wiki.libsdl.org/SDL2/SDL_HINT_JOYSTICK_HIDAPI_PS4_RUMBLE
