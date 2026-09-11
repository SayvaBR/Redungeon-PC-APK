# Criar personagens no Redungeon

Procedimento consolidado com Lykos, setembro de 2026. Esta documentação conserva o estudo no projeto para outras sessões e modelos.

## Contrato visual

Compare primeiro os PNGs originais em `Sprites Redungeon/Characters` e a captura `artifacts/lykos/atlas-study.png`. Figuras comuns ocupam aproximadamente 17–21 px de largura e 21–25 px de altura. O tamanho da célula não é o tamanho do corpo: nunca estique a imagem para preencher a célula.

Lykos usa células transparentes de 32×32, pés na linha 29, escala uniforme, contorno escuro e paleta curta. Norte (`n`) mostra costas; sul (`s`) mostra frente; leste (`e`) olha à direita; oeste (`w`) à esquerda. Quatro frames por direção. Mantenha cabeça, pés e volume estáveis durante a passada. Valide no tamanho real, não apenas ampliado.

Obrigatórios para cada forma: caminhada nas quatro direções, giro para queda, reação e corpo caído, choque com esqueleto próprio, pose de flecha e compatibilidade com fogo/esmagamento. O motor anima queda e fogo sobre os sprites do personagem; não é necessário duplicar partículas comuns. Confirme a direção: EffectEntity usa espelhamento por padrão; `mirrored: true` preserva o desenho.

UI: retrato, ícone, nome bitmap, caveira, ícones de habilidades e cinco frames de ressurreição. Nunca reutilize acidentalmente o retrato, cadáver ou prefixo de Bragg. O retrato tem escala diferente do sprite de partida.

## Arte e importação

`tools/Import-LykosArt.ps1 -Source docs/characters/lykos-source.png -Extras docs/characters/lykos-ui-source.png` reconstrói `Content/Images/lykos.png` e `lykos.json`. Execute na raiz do código. As coordenadas de recorte são específicas dessas imagens. Para outro personagem, crie seu próprio mapeamento; não aplique os recortes de Lykos cegamente.

O importador remove somente o fundo neutro conectado, preserva olhos/dentes internos, reduz por vizinho mais próximo, quantiza a paleta, alinha os pés e usa a fonte bitmap original para o nome. Imagens geradas são material de entrada: conferir direções, transparência, tamanhos e cada frame continua obrigatório.

Modelo de prompt para próxima arte: “Personagem [nome], identidade [silhueta, roupa, paleta], seguindo os sprites originais anexados. Pixel art nítida, contorno escuro de um pixel, sem suavização. Corpo aproximadamente 20×25 pixels, mesma escala e pés alinhados. Quatro direções e quatro passos por direção; acrescente reação e corpo caído para cada direção. Separar [formas]. Entregar fundo transparente, margens sem contato entre poses. Não desenhar cenário nem texto.” Peça retratos/ícones/esqueletos separadamente se necessário e inspecione antes de integrar.

## Integração

1. Acrescente IDs de Character, Skill, Stat, Achievement e SId ao final dos enums. Nunca reordene IDs já gravados. Lykos mantém o antigo ID Wolf.
2. Preencha CharDescription: classe, nome, biografia, retrato, ícone, caveira, nome bitmap, animação de apresentação, ressurreição e níveis. Atenção: CharLevel multiplica preço por 25; fator 4 significa 100 moedas.
3. Registre atlas e metadados em SpriteManager e descarte a textura no unload. Valide todos os nomes antes da partida.
4. Modele recarga/duração em uma classe pequena testável como LykosPower. Relógio de habilidades não pode depender do tempo lento do mundo. Documente se a recarga começa ao ativar ou ao terminar.
5. Implemente a classe PlayerEntity derivada: movimento, resistência, ativação, sprites e efeitos. Limpe luz e câmera lenta ao expirar, morrer e descarregar. Preserve o comportamento global dos outros personagens.
6. Acrescente descrições de habilidade, localização, estatística/conquista e migração de saves. A compra deve usar o fluxo real da loja e persistir exatamente o valor deduzido.

## Critério de pronto

Executar os contratos em Tests/RedungeonPC.InputContractTests e os cenários reais em tools/Test-LykosVisual.ps1. Para um personagem novo, adaptar os cenários: compra com saldo exato, save/reabertura, seleção, partida, cada nível, duração/recarga, interações, seis mortes, retorno, ressurreição e visual em retrato. QA usa saves isolados; nunca alterar o progresso do usuário para facilitar testes.

Publicar apenas em `JOGO REDUNGEON/PC` e `JOGO REDUNGEON/Android`. `C:/Dev/RedungeonBuild` é área temporária de compilação por causa dos acentos no caminho do MGCB. Capturas pequenas são evidências, não novas distribuições. Compilar APK não substitui testar toque, rotação, desempenho e vibração no aparelho.

## Som e transformação

Um personagem pode ter efeitos originais em WAV. Acrescente o nome ao fim de SoundName, registre o caminho em `Content/Sounds/sounds.json` e inclua a fonte WAV nos MGCB das plataformas que serão publicadas. Nesta rodada somente o PC recebeu o XNB de Lykos; não compilar Android até haver solicitação explícita.

`tools/generate_lykos_howl.py` produz de forma determinística o uivo original de Lykos: ataque curto, frequência ascendente, sustentação com vibrato e queda. Para outro personagem, crie outra fonte e outro identificador; não apenas renomeie este som.

Transformações precisam mostrar causa, troca e confirmação. Lykos usa 24 quadros: lua cheia ascendente e pausa do personagem; troca do atlas no meio; clarão, tremor e forma nova no final. O estado mecânico começa ao apertar a habilidade, mas o atlas visual muda no ponto central. O teste deve verificar ambos separadamente para evitar uma habilidade ativa com sprite antigo.
