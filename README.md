# Eternal Bastion - Tower Defense

## 📖 Sobre o Jogo
**Eternal Bastion** é um jogo de estratégia do tipo *Tower Defense* desenvolvido em Unity. O objetivo principal é defender a base Eternal Bastion contra ondas de inimigos, utilizando uma variedade de torres defensivas, gestão de recursos e planeamento estratégico.

---

## 🎮 Funcionalidades Principais

* **Múltiplos Modos de Jogo:**
    * **Singleplayer:** Campanha clássica com várias dificuldades (Fácil, Médio e Difícil).
    * **Multiplayer:** Modo competitivo em rede (via `NetworkConnectUI` e `Netcode`) em que 2 jogadores enfrentam-se defendendo a sua base e atacando a base inimiga para descobrir quem é o melhor jogador.
    * **PvE (Player vs IA):** Modo semelhante ao multiplayer porém contra uma Inteligência Artificial que usa Árvore de Comportamento para atacar e defender.
* **Sistema de Torres:**
    * Três tipos de torres: Archer Tower (Dano base), Fire Tower (Dano em área/Explosão) e Piercing Tower (Tiros perfurantes).
    * Sistema de *Upgrade* visual e estatístico.
* **Sistema de Minas de Ouro:**
    * Minas de Ouro que vão gerando ouro periodicamente para o jogador.
    * Sistema de *Upgrade* visual e estatístico.
* **Inimigos e Bosses:**
    * Variedade de inimigos: Normal, Tanque, Cavalos.
    * **Boss:** Cavalo de Troia (`TrojanHorseBoss`), com mecânicas de *spawn* de tropas ao morrer.
* **Consumíveis:**
    * Bomba que pode ser colocada no caminho e explode ao entrar em contacto com um inimigo.

---

## 🧠 Inteligência Artificial e Navegação

O projeto implementa três sistemas distintos de IA e Arquitetura para a gestão de agentes: Tomada de Decisão (Behavior Trees), Navegação Estratégica (Pathfinding) e Configuração Dinâmica (Decorator Pattern).

### 1. Behavior Trees (PvE)
Para o controlo do inimigo no modo PvE, implementamos uma Árvore de Comportamento, composta por nós de:
* **Sequência e Seleção (Composites):** Para lógica complexa.
* **Ações e Verificações (Leaves):** Para executar ações.

### 2. Estratégias de Pathfinding (NavMesh & Area Costs)
O jogo utiliza o **Unity NavMesh** de forma dinâmica. As torres não agem apenas como obstáculos físicos, mas alteram o "custo" do terreno ao seu redor através do script **`TowerDangerZone.cs`**. Isso cria zonas invisíveis de "perigo" (`NavMeshModifierVolume`) que influenciam a tomada de decisão dos inimigos em relação ao caminho que estes vão escolher.

Existem dois comportamentos de navegação distintos implementados:

#### A. Navegação Tática (Inimigo Padrão - `Enemy.cs`)
Os inimigos normais são programados para **procurar o caminho mais seguro**.
* **Lógica:** O script atribui custos elevados às áreas cobertas por torres (`DangerLevel1` = 5, `DangerLevel2` = 15, etc.).
* **Resultado:** O algoritmo A* do NavMesh calcula um caminho que evita as torres, mesmo que esse caminho seja fisicamente mais longo. O inimigo tenta evitar as defesas para encontrar um caminho seguro.

#### B. Navegação "Tanque" (Boss - `TrojanHorseBoss.cs`)
O Cavalo de Troia graças à sua grande quantidade de vida não tem de se preocupar com o perigo.
* **Lógica:** No método `IgnorarPerigo()`, o script define o custo de todas as áreas de perigo para **1** (igual ao custo de terreno normal).
* **Resultado:** O Boss ignora completamente a quantidade de torres no caminho e escolhe sempre a rota mais curta em direção à base, forçando o jogador a ter poder de fogo suficiente para o parar.

### 3. Padrão Decorator (Gestão Dinâmica de Agentes)
Para otimizar a memória e flexibilizar a criação de inimigos, implementámos o **Decorator Pattern** no sistema de *Spawning*. Em vez de depender de *prefabs* diferentes para o inimigo normal e para o tanque, o jogo utiliza um único modelo base que é alterado em tempo de execução.

* **Interface `IEnemyDecorator`:** Define o contrato para modificação de agentes.
* **Injeção de Comportamento:** O `EnemySpawner` decide dinamicamente qual decorador aplicar:
    * **NormalDecorator:** Configura os atributos do inimigo normal.
    * **TankDecorator:** Transforma o inimigo base num Tanque, alterando a escala (2x), modificando a textura e aumentando drasticamente a vida e a recompensa quando morto, sem necessidade de um novo *prefab*.
* **Sistema Híbrido:** A arquitetura permite misturar geração via código (Normal/Tanque) com prefabs dedicados (Cavalo) quando existem diferenças complexas de modelo e esqueleto (Rigging), garantindo o melhor equilíbrio entre performance e qualidade visual.

---

## 🛠️ Detalhes Técnicos de Implementação

### Scripts da Árvore de Comportamento (PvE)
* **`AIController.cs`:**
    * Atua como o "cérebro" da IA, inicializando a árvore (`SetupBehaviorTree`) e executando a avaliação lógica periodicamente.
    * Contém os métodos concretos de ação (ex: `Action_BuildBestTower`) e verificação (ex: `Check_HasMoney`).
* **`BTNode.cs`:**
    * Classe base abstrata para todos os nós. Define o método `Evaluate()` e os estados de retorno possíveis (`SUCCESS`, `FAILURE`, `RUNNING`).
* **`BTSelector.cs` (Lógica "OU"):**
    * Percorre os nós filhos e retorna sucesso assim que encontrar um que funcione.
    * Essencial para definir as prioridades da IA (tenta Defender; se não der, tenta Atacar).
* **`BTSequence.cs` (Lógica "E"):**
    * Executa os nós filhos em ordem, parando e falhando a sequência inteira se um único filho falhar.
    * Garante que ações só ocorrem se as condições anteriores forem atendidas (ex: Ter Dinheiro -> Comprar Tropa).
* **`BTAction.cs` & `BTCheck.cs`:**
    * Nós folha da árvore. O `BTCheck` executa validações booleanas e o `BTAction` invoca os métodos que alteram efetivamente o estado do jogo.

### Scripts do PathFinding
* **`TowerDangerZone.cs`:**
    * Deteta o nível da torre e cria um volume `NavMeshModifierVolume`.
    * A cada upgrade, redimensiona a zona e atualiza a `NavMeshSurface` em tempo real (`RebakeNavMesh`), alterando dinamicamente o mapa de navegação durante o jogo.
* **`Enemy.cs`:**
    * Usa `agent.SetAreaCost` para aumentar a aversão a áreas de perigo.
    * `stoppingDistance = 0` garante que o inimigo entra fisicamente na base.
* **`TrojanHorseBoss.cs`:**
    * Sobrescreve os custos de área padrão para ignorar a lógica de segurança.
    * Implementa uma corrotina `SpawnTroopsRoutine` para libertar inimigos após a sua destruição.
 
### Scripts do Decorator
* **`EnemyDecorators.cs`:**
    * Contém a lógica para manipular os `GameObjects` instanciados, aplicando alterações de stats e visuais sem dependência direta de MonoBehaviour.
* **`EnemySpawner.cs`:**
    * Responsável por gerar as ondas de inimigos e é o que decide se a tropa a colocar é Inimigo Normal, Tanque, Cavalo ou Cavalo de Troia, chamando o `EnemyDecorator.cs` para definir os status e materiais dos inimigos normais e tanques.

---

## 🎮 Controlos de Jogo

* **WASD para mover a câmara**
* **Scroll do mouse para aumentar/diminuir zoom da câmara**
* **Botão Esquerdo do Mouse para selecionar botões**
* **Tecla Esc para pausar o jogo e aceder ao painel de resume**
  

## 👥 Créditos

**Desenvolvido por:** Emanuel Barbosa Nº29847 , Gabriel Savignon Nº27924 , José Abreu Nº27918.

**Projeto:** Inteligência Artificial Aplicada a Jogos
