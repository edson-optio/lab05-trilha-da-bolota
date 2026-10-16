# Lab 05 — Plataforma: A Trilha da Bolota

Jogo de plataforma 2D em Unity feito a partir do enunciado `LAB5_Plataforma_Slides.pdf` e dos
tilesets `tilesetOpenGame.png` e `tilesetOpenGameBackground.png`.

> **História:** o inverno está chegando e o vento espalhou as bolotas do esquilo **Tito** pela
> floresta. Ajude Tito a juntar todas antes que a neve caia, sem se deixar pegar pelas lesmas e abelhas.

## Jogar

- **Mac:** abra `Entrega/A Trilha da Bolota.app`. Não precisa ter o Unity instalado.
- **Unity:** clique duas vezes em `Abrir projeto Unity.command` ou, no Unity Hub, use
  **Add project from disk** e escolha a pasta `TrilhaDaBolota`. Use o **Unity 6.3 LTS (6000.3.24f1)**.
  O projeto abre na cena `Assets/Scenes/Menu.unity`; aperte **Play**.

| Ação | Teclas |
|---|---|
| Andar | Setas esquerda/direita ou A / D |
| Pular (segure para pular mais alto) | Espaço, W, seta para cima ou Z |
| Pausar / continuar | P ou Esc |
| Começar / jogar de novo | Enter ou o botão na tela |

## Requisitos do enunciado

| Enunciado | Como foi feito |
|---|---|
| Jogo do tipo plataforma | Esquilo com corrida, pulo variável, *coyote time* e *jump buffer* (`PlayerController.cs`) |
| Duas fases | Cenas `Fase1` (Bosque do Girassol, 8 bolotas) e `Fase2` (Colina das Nuvens, 10 bolotas, buracos e plataformas flutuantes) |
| Colisão com as plataformas | Tilemap `Chao` com **Tilemap Collider 2D + Composite Collider 2D + Rigidbody 2D estático**, como nos slides. O esquilo usa Rigidbody 2D dinâmico e Box Collider 2D |
| Parallax scrolling | Três camadas (nuvens 0,92, colinas 0,75, bosque 0,5) que acompanham uma fração do movimento da câmera e se repetem sem fim (`Parallax.cs`) |
| Sprites animados | Esquilo (parado, correndo, pulando, caindo), lesma, abelha, bolota girando, morango, coração e brilho de coleta (`SpriteAnimator.cs`) |
| Itens colecionáveis | Bolota (+10, obrigatória), morango (+50), coração (+1 vida) |
| Pontuação | HUD com pontos, vidas em corações, bolotas `x / total` e nome da fase. Pisar num inimigo vale +100 e concluir uma fase dá +200 |
| Cena final com botão de reiniciar | Cena `Fim` com **VITÓRIA!** ou **FIM DE JOGO**, pontos, bolotas e os botões *Jogar novamente* e *Menu* |
| Derrota | Inimigos (ou quedas em buracos) tiram vidas. Com 0 vidas, a cena final mostra a derrota |
| Vitória | Coletar as **18 bolotas** (todas das duas fases) |
| Tela inicial com instruções | Cena `Menu` com história, controles, legenda de itens e inimigos e botão **JOGAR** |
| Sprites e história próprios | O personagem, os inimigos e os itens foram desenhados para este projeto (ver *Arte*) |

## Organização

```
lab05/
├── Entrega/                     app para Mac e ZIP do projeto
├── TrilhaDaBolota/              projeto Unity
│   └── Assets/
│       ├── Art/                 tilesets do laboratório + Gerados/ (pixel art própria)
│       ├── Fases/               fase1.txt e fase2.txt: o desenho de cada fase em texto
│       ├── Tiles/               Tiles recortados (16x16) e a Tile Palette "Floresta"
│       ├── Prefabs/             Bolota, Morango, Coracao, Lesma, Abelha
│       ├── Scenes/              Menu, Fase1, Fase2, Fim
│       ├── Scripts/             código do jogo (um arquivo por responsabilidade)
│       └── Editor/              monta o projeto e gera o app
├── ferramentas/                 scripts Python que geram a arte e os mapas
└── Verificacao/                 relatório do teste automático e capturas de tela
```

### Scripts principais

| Arquivo | Papel |
|---|---|
| `PlayerController.cs` | Movimento, pulo, detecção de chão (contato com a camada `Chao` com a normal para cima), dano e invulnerabilidade |
| `Enemy.cs`, `Snail.cs`, `Bee.cs` | Contato de inimigo: pisão derrota o inimigo e o contato lateral causa dano. A lesma vira na parede ou na beirada; a abelha patrulha voando |
| `Collectible.cs` | Itens com gatilho (trigger); as bolotas são obrigatórias |
| `LevelManager.cs` | Regras da fase: pontos, vidas, quedas, ponto de retorno (placas), pausa e fim da fase |
| `GameSession.cs` | Pontos, vidas e bolotas que passam de uma cena para outra; troca de cenas |
| `Parallax.cs` | Fundo em camadas com repetição infinita |
| `CameraFollow.cs` | Câmera suave limitada às bordas do mapa |
| `GameHUD.cs`, `MenuScreen.cs`, `EndScreen.cs`, `UiKit.cs` | Interface montada por código |

### Como as fases são feitas

Cada fase é um arquivo de texto (`Assets/Fases/faseN.txt`) em que cada caractere é um bloco de
16 px. O `LevelBuilder` (Editor) lê o arquivo e:
1. pinta o chão (`#`) no Tilemap `Chao`, escolhendo o bloco certo (topo com grama, borda,
   terra ou plataforma fina) conforme os vizinhos;
2. pinta árvores, cercas, girassóis, pedras, postes e placas no Tilemap `Decoracao`, sem colisão;
3. coloca o esquilo, os itens e os inimigos a partir dos prefabs.

Para mudar uma fase, edite o `.txt` e use **Trilha da Bolota → 1. Montar fases e assets**. Dá
também para pintar à mão com a Tile Palette *Floresta* (Window → 2D → Tile Palette).

### Fluxo do tileset (igual aos slides)

`ArtImporter.cs` faz por código o passo a passo dos slides: Sprite Mode **Multiple**,
**16 Pixels Per Unit**, Mesh Type **Full Rect**, **Sprite Editor → Slice → Grid By Cell Size 16×16**,
criação dos Tiles e da Tile Palette.

## Arte

- `tilesetOpenGame.png` e `tilesetOpenGameBackground.png`: fornecidos no laboratório
  (OpenGameArt, arte de Paulina Riva / *paur*). O chão (`Gerados/chao.png`) foi recortado do morro do
  tileset, com bordas acrescentadas, e o bosque do fundo usa a árvore do tileset em silhueta.
- Esquilo, lesma, abelha, bolota, morango, coração, brilho e as colinas do fundo: pixel art desenhada
  para este projeto por `ferramentas/gerar_arte.py`. **Para trocar um sprite**, substitua o PNG em
  `TrilhaDaBolota/Assets/Art/Gerados/` mantendo o nome e use **Trilha da Bolota → 1. Montar fases e assets**.
- Sons: sintetizados no código (`GameAudio.cs`). Fonte: LegacyRuntime, embutida no Unity.

## Verificação

O app aceita `-selfTest -testOutput /caminho/da/pasta`. Nesse modo ele joga sozinho as cenas reais e
salva `resultado-testes.json` e as capturas de tela. O teste passa por:
- tela inicial com instruções e o botão JOGAR;
- queda e pouso no chão do Tilemap, corrida, pulo, parede do degrau e subida no degrau;
- câmera seguindo o esquilo, as três camadas de parallax com frações diferentes e o fundo sem buracos;
- animação do esquilo, da lesma e da bolota;
- bolota, morango e coração (pontos, contador e vida), dano da lesma e da abelha, pisão e queda em
  buraco com volta à placa;
- pausa, conclusão da fase 1 e ida para a fase 2 mantendo os pontos;
- derrota com 0 vidas, botão de reiniciar, vitória com as 18 bolotas e volta ao menu;
- proporção 16:9 mantida numa janela 4:3.

**Resultado em 16/10/2026: 43 verificações aprovadas e nenhuma falha** no app para macOS compilado com
Unity 6000.3.24f1 (Apple Silicon). Veja `Verificacao/`.

Para refazer tudo pelo terminal:

```sh
UNITY=/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity
$UNITY -batchmode -quit -projectPath "$PWD/TrilhaDaBolota" \
  -executeMethod TrilhaDaBolota.Editor.ProjectBuilder.BuildAll -logFile Verificacao/unity-build.log
"Entrega/A Trilha da Bolota.app/Contents/MacOS/A Trilha da Bolota" -selfTest \
  -testOutput "$PWD/Verificacao" -screen-width 1280 -screen-height 720 -screen-fullscreen 0
```

## Entrega

`Entrega/Lab05-TrilhaDaBolota-Projeto.zip` traz o projeto editável (`Assets`, `Packages`,
`ProjectSettings`), as ferramentas, este guia e as evidências dos testes. O app para Mac fica separado em
`Entrega/A Trilha da Bolota.app`.
