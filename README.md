# Flappy Canarinho

Jogo 2D no estilo *Flappy Bird*, feito em Unity. Um canarinho precisa passar pelas aberturas entre torres que surgem sem parar, e cada abertura vencida vale 1 ponto. O mapa é infinito: o cenário rola continuamente e os obstáculos são gerados enquanto você sobreviver.

Trabalho individual da disciplina **Desenvolvimento de Jogos Digitais**, do curso de **Ciência da Computação** da **Universidade do Sagrado Coração (Unisagrado)**, ministrada pelo professor **Luis Filipe Grael Tinós**.

- **Autor:** João Vitor Camargo
- **Entrega:** 18/11/2026
- **Engine:** Unity `6000.0.84f1` (Unity 6), com URP 2D

---

Download da Build executavel em: https://github.com/Jvcamargo1/OUTROS-J.D-USC-2026/releases/tag/Build

---

## Como jogar

| Ação | Controle |
|---|---|
| Voar (dar um impulso para cima) | **Barra de espaço** |
| Botões do menu e da tela de game over | **Mouse** (clique) |

1. No menu, clique em **Iniciar** (ou em **Créditos** para ver os créditos).
2. O canarinho fica pairando até o seu primeiro toque. Aperte **espaço** para começar.
3. Aperte **espaço** repetidamente para manter o pássaro no ar e passar pelas aberturas entre as torres.
4. Cada par de torres ultrapassado soma **1 ponto**.
5. Bater em uma torre ou no chão encerra a partida (**Game Over**). Na tela final você vê seus pontos e o seu melhor resultado, e escolhe entre **Reiniciar** ou voltar ao **Menu**.

O melhor resultado fica salvo no computador (`PlayerPrefs`) e permanece entre as partidas.

---

## Como abrir e executar o projeto

1. Instale o **Unity Hub** e a versão **Unity 6000.0.84f1** (versões próximas da série 6000.0 costumam abrir, mas esta é a usada no desenvolvimento).
2. No Unity Hub, clique em **Add** > **Add project from disk** e selecione a pasta deste projeto.
3. Abra a cena `Assets/FlappyCanarinho/Scenes/Menu.unity`.
4. Aperte **Play**.

As cenas **Menu** (índice 0) e **Game** (índice 1) já estão no *Build Settings*, então também é possível gerar um executável em `File > Build Profiles`.

> A pasta `Library/` não faz parte da entrega. O Unity a recria automaticamente na primeira abertura, o que pode levar alguns minutos.

---

## O que foi implementado

O enunciado pede a aplicação de conceitos fundamentais de desenvolvimento de jogos. Estes são os usados aqui:

| Conceito | Onde está no projeto |
|---|---|
| Controle de entrada do jogador | Barra de espaço lida em `BirdController` |
| Movimentação por física | O pássaro usa `Rigidbody2D` (gravidade e impulso de voo) |
| Movimentação por Transform | Canos, chão e camadas do fundo se movem por `Transform` |
| Colisões e interações | Torres (trigger) e chão (colisão sólida) encerram a partida |
| Pontuação e condição de derrota | Ponto por par ultrapassado, Game Over e melhor pontuação salva |
| Controle de estados | `GameState`: `Ready` (aguardando), `Playing` (jogando) e `Dead` (game over) |
| Interface e feedback | Menu, placar na tela, aviso inicial, game over e créditos |
| Geração procedural | Pares de torres com altura aleatória, sem fim |
| Parallax | Fundo em várias camadas, cada uma com sua velocidade |

### Detalhes técnicos

- **Mapa infinito:** o fundo é feito de camadas (nuvens, montanhas, arbustos, chão) com 3 *tiles* cada. Quando um tile sai pela esquerda, ele volta pela direita (`ScrollingBackground`). As camadas mais distantes andam mais devagar, o que dá o efeito de profundidade.
- **Torres sempre fora da tela:** o ponto de nascimento é calculado a partir da borda direita da câmera, então as torres entram pela lateral em qualquer proporção de tela. Elas são destruídas só depois de saírem pela borda esquerda.
- **Altura das aberturas:** a altura de cada abertura é sorteada, mas a variação entre dois pares seguidos é limitada, para o jogo continuar sempre possível.
- **Antes do primeiro toque:** o pássaro paira com um leve movimento, para o jogador não morrer assim que a cena carrega.
- **Hitbox mais justa:** o colisor do pássaro é menor que o sprite.

### Parâmetros fáceis de ajustar

Direto no Inspector, sem mexer em código:

| Componente | Campo | O que controla |
|---|---|---|
| `GameManager` | `pipeSpeed` | Velocidade das torres e do chão |
| `PipeSpawner` | `spawnInterval` | Tempo entre dois pares de torres |
| `PipeSpawner` | `gapSize` | Tamanho da abertura |
| `PipeSpawner` | `minGapCenter` / `maxGapCenter` | Faixa de altura das aberturas |
| `PipeSpawner` | `maxStep` | Variação máxima de altura entre pares seguidos |
| `BirdController` | `flapVelocity` | Força do impulso de voo |
| `BirdController` | `gravityScale` | Intensidade da gravidade |

---

## Estrutura do projeto

```
Assets/
├── FlappyCanarinho/
│   ├── Scenes/            Menu.unity e Game.unity
│   ├── Scripts/
│   │   ├── GameManager.cs        Estado da partida, pontuação e UI de game over
│   │   ├── BirdController.cs     Entrada, voo, inclinação e morte do pássaro
│   │   ├── Pipe.cs               Par de torres: movimento, pontuação e destruição
│   │   ├── PipeSpawner.cs        Geração infinita de torres
│   │   ├── ScrollingBackground.cs  Camadas de fundo com rolagem infinita
│   │   └── MenuController.cs     Botões Iniciar, Créditos e Voltar
│   ├── Editor/
│   │   └── SceneBuilder.cs       Ferramenta de editor usada para montar as cenas iniciais
│   ├── Prefabs/           PipePair.prefab (par de torres)
│   ├── Materials/         Material de texto com contorno
│   └── img/               CRÉDITOS.png (imagem da tela de créditos)
└── Jovial Games/
    └── Basic 2D Side Scroller Art Kit/   Arte usada no jogo
```

> `SceneBuilder.cs` foi usado para montar a primeira versão das cenas. Depois disso, as cenas foram ajustadas manualmente (fundo, torres, créditos). **Não é necessário executá-lo**, e rodá-lo de novo recria as cenas do zero, desfazendo esses ajustes.

---

## Créditos

- **Desenvolvimento:** João Vitor Camargo
- **Arte:** *Basic 2D Side Scroller Art Kit*, de **Jovial Games** (Unity Asset Store). A arte pertence aos seus autores.
- **Fonte dos textos:** TextMeshPro (Liberation Sans SDF)
- **Engine:** Unity

---

## Agradecimento especial

Obrigado, professor Luis Filipe Grael Tinós, pelas aulas de Desenvolvimento de Jogos Digitais. A física, as colisões e o controle de estados que aparecem neste jogo vieram do que vi em sala. Agradeço também por deixar a escolha do tema e do estilo com a gente: pude fazer um Flappy Bird com um canarinho.
