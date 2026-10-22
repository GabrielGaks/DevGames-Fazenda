# **GAME DESIGN DOCUMENT (GDD)** 

**“O menino loiro de cabelo castanho que está em uma aventura para recuperar os dedos da mão direito do pai que perdeu depois de uma noite chuvosa”** 

Disciplina: Desenvolvimento de Games 

Engine: Unity 6 - Universal 3D (URP) 

Página 1 

## **1. Página de Título** 

### **1.1. Nome do jogo** 

O menino loiro de cabelo castanho que está em uma aventura para recuperar os dedos da mão direito do pai que perdeu depois de uma noite chuvosa 

### **1.2. Conceito resumido** 

Jogo de aventura e parkour em 3D no qual o jogador controla um garoto que precisa atravessar cinco grandes percursos progressivamente mais difíceis para recuperar os cinco dedos perdidos da mão direita de seu pai. 

A história possui caráter propositalmente absurdo e humorístico. O principal foco do jogo está na jogabilidade, no desafio de movimentação e na evolução da habilidade do jogador. 

## **2. Visão Geral do Jogo** 

### **2.1. Público-alvo** 

O jogo é destinado a jogadores a partir de aproximadamente 10 anos de idade, sem possuir um público infantil específico. 

A proposta é atingir jogadores que gostem de: 

- Jogos de parkour; 

- Jogos de plataforma em 3D; 

- Desafios baseados em habilidade; 

- Speedruns; 

- Jogos com progressão vertical; 

- Experiências semelhantes a Only Up! e Chained Together; 

- Jogos nos quais cometer um erro pode fazer o jogador perder parte significativa de seu progresso. 

Não há sistema de vida, combate tradicional ou morte do personagem. 

### **2.2. Conceito do jogo** 

O jogo é uma aventura de parkour tridimensional dividida em cinco fases. Cada fase apresenta um ambiente completamente diferente e possui um percurso construído principalmente de maneira vertical. O objetivo do jogador é subir pelo cenário utilizando movimentação, corrida, saltos, controle aéreo e escalada até alcançar o final. 

Quando o jogador erra um salto, ele simplesmente cai. Dependendo do local da queda, poderá cair em uma plataforma pela qual já passou, voltar uma parte considerável da fase, cair próximo do início ou retornar praticamente ao começo de todo o percurso. 

Isso cria a principal forma de punição do jogo: perder progresso por causa de um erro de movimentação. 

Ao chegar ao final de uma fase, o jogador encontra um dos dedos perdidos do pai do protagonista. O dedo funciona como o troféu de conclusão daquela fase. Ao coletá-lo, a próxima fase é liberada. 

### **2.3. Gênero** 

Gênero principal: Aventura / Parkour / Plataforma 3D. 

Página 2 

O jogo também possui elementos de desafio, exploração, plataforma de precisão e speedrun. Não existe combate; o principal conflito ocorre entre o jogador e o próprio cenário. 

### **2.4. Resumo do fluxo do jogo** 

Fluxo principal: 

Menu Principal → Seleção de Fase → Início da Fase → Parkour → Coleta do Dedo → Conclusão da Fase → Registro do Tempo → Liberação da Próxima Fase → Seleção de Fase 

Ao selecionar uma fase, o personagem aparece no ponto inicial. No mesmo instante, um cronômetro começa a contar o tempo daquela tentativa. 

O jogador deverá utilizar movimentação, corrida, visão, saltos e escalada para avançar pelo percurso. Caso erre algum salto, ele cairá naturalmente pelo cenário. Não existe Game Over. 

O jogador pode continuar de onde conseguir cair ou pressionar R para retornar imediatamente ao início da fase. Pressionar R não reinicia o cronômetro. 

O cronômetro termina somente quando o jogador conclui a fase ou sai dela. 

Quando uma fase é concluída, o tempo daquela tentativa é comparado com o melhor tempo já registrado para aquela fase. Caso seja menor, ele passa a ser armazenado como o novo recorde pessoal. 

No final do percurso existe um dedo da mão direita do pai. Ao entrar em contato com o objeto, a fase é considerada concluída. 

### **2.5. Aparência e sensação** 

O jogo será desenvolvido em 3D e terá câmera em terceira pessoa. Cada fase terá identidade visual própria, mas o foco será sempre manter os obstáculos, plataformas, caminhos e distâncias legíveis para o jogador. 

Apesar da história absurda e humorística, o visual do jogo não precisa ser completamente cômico. O humor estará principalmente na premissa, no nome do jogo, nos dedos usados como troféus, em pequenos elementos espalhados pelo cenário e em situações inesperadas. 

## **3. História, Cenário e Personagens** 

### **3.1. História e narrativa** 

O protagonista é um menino loiro de cabelo castanho que vive com seu pai. 

Durante uma noite extremamente chuvosa, ocorre um acontecimento inexplicável que faz com que diferentes partes do corpo do pai sejam perdidas. 

O pai informa ao filho que os cinco dedos de sua mão direita desapareceram e precisam ser recuperados. Por razões igualmente inexplicáveis, cada dedo foi parar em uma região completamente diferente. 

O filho então parte sozinho em uma aventura pelo mundo com a missão de recuperar os cinco dedos. Sua jornada passa por: 

1. Uma fazenda; 

2. Uma grande cidade; 

3. Uma região montanhosa; 

Página 3 

4. O espaço; 

5. Uma dimensão psicodélica e incompreensível. 

A cada fase, o protagonista encontra um dos dedos e se aproxima de completar sua missão. 

Durante quase todo o jogo, o jogador é levado a acreditar que os cinco dedos são as principais partes do corpo perdidas pelo pai. Porém, essa informação é propositalmente incompleta. 

A narrativa não pretende ser o principal elemento do jogo. Ela funciona como uma justificativa propositalmente absurda para os desafios apresentados. O foco principal permanece na jogabilidade. 

#### **Final da história** 

Após recuperar o quinto dedo, ocorre uma cutscene. 

O filho retorna para casa e entrega os cinco dedos da mão direita ao pai. Os dois permanecem juntos por alguns instantes e depois olham pela janela. 

Nesse momento, o pai comenta que agora precisam esperar a irmã do protagonista retornar. 

É então revelado que a filha também está em uma aventura e sua missão é recuperar as pernas e o braço esquerdo do pai. 

O jogador descobre apenas nesse momento que a noite chuvosa provocou consequências muito maiores do que as apresentadas no começo da história. O protagonista recebeu somente a responsabilidade de recuperar os dedos da mão direita. 

Além de funcionar como a piada final da narrativa, esse encerramento deixa aberta a possibilidade de uma continuação protagonizada pela filha. 

### **Lente #1 - Experiência Essencial** 

A experiência essencial é fazer com que o jogador tenha constantemente a sensação de: “Eu consigo fazer isso se tentar mais uma vez.” 

O jogador deverá enfrentar desafios difíceis, mas compreensíveis. O jogo não pretende criar obstáculos impossíveis ou injustos. 

Cada dificuldade deverá poder ser superada através de observação, tentativa, aprendizado, precisão e domínio da movimentação. 

Quanto mais o jogador avança, maior é o risco associado a um erro. A altura do cenário também permite que o jogador visualize parte do caminho que já percorreu, reforçando a sensação de progresso e a tensão de uma possível queda. 

#### **Elementos essenciais para essa experiência** 

- Movimentação responsiva; 

- Plataformas bem definidas; 

- Verticalidade; 

- Quedas como principal punição; 

- Dificuldade progressiva; 

- Ausência de checkpoints tradicionais; 

- Cronômetro; 

- Melhor tempo salvo por fase; 

- Dedo como recompensa final de cada nível; 

- Corrida e impulso; 

- Controle aéreo; 

Página 4 

- Mecânica de agarrar e escalar bordas. 

### **Lente #2 - Surpresa** 

O jogo utilizará elementos inesperados para quebrar a previsibilidade das fases. 

- Plataformas móveis; 

- Objetos que começam a se movimentar quando o jogador se aproxima; 

- Plataformas que giram; 

- Mudanças repentinas no caminho; 

- Objetos aparentemente decorativos que fazem parte do parkour; 

- Elementos lançados ou criados no cenário; 

- Mudanças de iluminação; 

- Alterações de gravidade em fases avançadas; 

- Partículas; 

- Objetos posicionados aleatoriamente dentro de limites definidos; 

- Rotas opcionais mais difíceis; 

- Pequenas referências escondidas. 

A maior surpresa narrativa ocorre no encerramento, quando é revelado que o pai também perdeu as pernas e o braço esquerdo, e que a irmã do protagonista está em outra aventura para recuperá-los. 

### **3.2. Mundo do jogo** 

#### **3.2.1. Visual geral e sensação do mundo** 

O mundo começa relativamente normal e se torna progressivamente mais estranho. 

Sequência temática: Fazenda → Cidade → Montanhas → Espaço → Dimensão Psicodélica. 

Essa progressão acompanha tanto o aumento de dificuldade quanto o aumento do absurdo da aventura. 

#### **3.2.2. Áreas** 

#### **Área 1 - Fazenda** 

Ambiente rural composto por elementos como celeiros, casas, cercas, plantações, fardos de feno, tratores, árvores, moinhos, silos, estruturas de madeira, animais e equipamentos agrícolas. 

O percurso começa próximo ao chão e progressivamente sobe pelas estruturas da fazenda. 

#### **Área 2 - Cidade** 

Área urbana vertical formada por ruas, veículos, caçambas, escadas, andaimes, prédios, telhados, placas, postes, guindastes e estruturas em construção. 

A fase aumenta significativamente o uso de altura e os saltos entre estruturas. 

#### **Área 3 - Montanhas** 

Região natural com grandes diferenças de altitude, composta por rochas, penhascos, árvores, pontes, cavernas, troncos, cachoeiras, plataformas naturais e possíveis áreas de neve. 

O cenário deverá dar ao jogador a sensação de estar escalando uma enorme montanha. 

#### **Área 4 - Espaço** 

Região espacial formada por estruturas artificiais e corpos celestes, como satélites, asteroides, partes de estação espacial, painéis solares, plataformas flutuantes, foguetes e planetas ao fundo. 

Página 5 

A fase poderá introduzir alterações na sensação de gravidade e saltos maiores. 

#### **Área 5 - Dimensão Psicodélica** 

A última área abandona parcialmente a lógica visual encontrada anteriormente. É uma dimensão estranha inspirada em elementos cósmicos, psicodélicos, surrealistas, místicos e lovecraftianos. 

O jogador poderá encontrar olhos gigantes, tentáculos, estruturas flutuantes, geometria impossível, objetos sem origem clara, portais, iluminação incomum, objetos gigantes e plataformas que parecem desafiar a lógica. 

Essa fase representa o ápice tanto da dificuldade quanto da estranheza do jogo. 

### **3.3. Personagens** 

#### **Protagonista** 

Um menino loiro de cabelo castanho encarregado de recuperar os dedos perdidos de seu pai. 

Sua personalidade não será profundamente explorada, pois ele funciona principalmente como avatar do jogador. Sua principal característica narrativa é aceitar uma missão completamente absurda como se fosse relativamente normal. 

Habilidades: caminhar, correr, pular, cair, controlar-se no ar, agarrar bordas, escalar e realizar movimentos de parkour. 

#### **Pai** 

É o personagem responsável por iniciar a aventura. Após uma noite extremamente chuvosa, diversas partes de seu corpo são perdidas. 

O filho fica responsável especificamente pela recuperação dos cinco dedos da mão direita. Sua presença no gameplay é pequena, sendo principalmente importante para estabelecer o contexto da aventura e para a cutscene final. 

#### **Irmã** 

A irmã do protagonista é revelada somente no final. Enquanto o protagonista recuperava os dedos da mão direita, ela estava em outra aventura para recuperar as pernas e o braço esquerdo do pai. 

A personagem serve como elemento da piada final e como possível protagonista de uma continuação. 

## **4. Jogabilidade e Mecânicas** 

### **4.1. Gameplay** 

O gameplay é baseado integralmente em movimentação e domínio do parkour. O jogador deverá analisar o cenário e utilizar diferentes formas de movimentação para avançar. 

As principais habilidades envolvidas serão: 

- Caminhar; 

- Correr; 

- Pular; 

- Controlar o personagem durante o salto; 

- Utilizar impulso; 

Página 6 

- Agarrar e escalar determinadas bordas; 

- Recuperar-se de saltos quase fracassados; 

- Planejar rotas; 

- Controlar a câmera. 

A movimentação deverá oferecer liberdade suficiente para que jogadores mais habilidosos consigam percorrer determinadas partes de maneiras mais rápidas. 

#### **4.1.1. Objetivos** 

Objetivo principal: encontrar os cinco dedos perdidos da mão direita do pai. 

Objetivo de cada fase: chegar até o ponto mais alto ou final do percurso e coletar o dedo presente naquele nível. 

Objetivos secundários: completar fases mais rapidamente, melhorar o próprio tempo, explorar caminhos diferentes e encontrar referências ou elementos escondidos. 

#### **4.1.2. Progressão do jogo** 

Existem cinco fases, liberadas progressivamente: 

1. Fazenda; 

2. Cidade; 

3. Montanhas; 

4. Espaço; 

5. Dimensão Psicodélica. 

A dificuldade aumenta progressivamente. Ao terminar uma fase, o jogador recebe um dedo e libera o próximo nível. 

#### **4.1.3. Estrutura dos desafios** 

Cada fase será construída utilizando pequenos conjuntos de obstáculos. 

Exemplo: Plataforma simples → Salto → Plataforma estreita → Objeto móvel → Salto maior → Área segura → Nova sequência. 

No início do jogo, os obstáculos aparecem isoladamente. Nas fases posteriores, diferentes mecânicas passam a ser combinadas. 

Dessa forma, o jogador primeiro aprende uma mecânica e posteriormente demonstra domínio sobre ela. 

#### **4.1.4. Estrutura de enigmas** 

O jogo não terá puzzles tradicionais como foco. Os pequenos problemas apresentados ao jogador serão principalmente relacionados à navegação, como descobrir onde pular, observar o movimento de uma plataforma, encontrar a melhor rota e calcular o momento correto de iniciar um salto. 

#### **4.1.5. Fluxo do gameplay** 

Entrar na fase → Cronômetro inicia → Observar o ambiente → Realizar parkour → Avançar verticalmente → Cair ou continuar → Chegar ao final → Coletar o dedo → Cronômetro para → Registrar tempo → Liberar próxima fase. 

Página 7 

### **4.2. Mecânicas** 

#### **4.2.1. Física** 

O jogo utiliza física tridimensional. Elementos importantes: gravidade, colisões, velocidade, impulso, queda, rotação, torque e objetos móveis. 

A física deverá ser previsível o suficiente para que o jogador consiga aprender como seus movimentos funcionam. 

#### **4.2.2. Movimento no jogo** 

- W: mover para frente; 

- A: mover para esquerda; 

- S: mover para trás; 

- D: mover para direita; 

- Mouse: controlar a câmera em terceira pessoa; 

- Espaço: pular; 

- Shift: correr; 

- R: retornar imediatamente ao ponto inicial da fase; 

- Esc: pausar. 

O uso da tecla R não reinicia o cronômetro. 

#### **Corrida e impulso** 

O jogador poderá segurar Shift enquanto se movimenta para aumentar sua velocidade. 

A corrida possui duas funções principais: percorrer áreas com maior rapidez e gerar impulso adicional para saltos mais longos. 

Alguns obstáculos poderão ser construídos especificamente para exigir uma pequena corrida antes do salto. 

#### **Controle aéreo** 

Após iniciar um salto, o jogador continuará podendo influenciar parcialmente a direção do personagem. 

Esse controle não deverá permitir mudanças completamente irreais de trajetória, mas deverá ser forte o suficiente para permitir pequenos ajustes, correções e melhores aterrissagens. 

#### **Mecânica de agarrar e escalar** 

Quando o jogador realizar um salto e não conseguir chegar completamente sobre uma plataforma, ainda poderá ter uma segunda oportunidade. 

Caso o personagem encoste em uma borda ou lateral escalável, ele poderá permanecer brevemente em uma condição de agarrar. 

Se o jogador pressionar Espaço nessa situação, o personagem realiza uma ação de escalada e sobe para a plataforma. 

Fluxo: Pular → Não alcançar completamente → Encostar na borda → Pressionar Espaço → Escalar → Continuar. 

Caso o jogador não consiga alcançar uma borda válida ou não realize a ação necessária, continuará caindo. 

Página 8 

#### **4.2.3. Objetos** 

O personagem não precisará carregar objetos como mecânica principal. Os objetos presentes no ambiente poderão ser estáticos, móveis, giratórios, possuir física própria, realizar spawn automático, ter movimento repetitivo ou possuir posicionamento parcialmente aleatório. 

#### **4.2.4. Ações** 

- Movimentar; 

- Correr; 

- Pular; 

- Controlar a câmera; 

- Agarrar e escalar bordas; 

- Reiniciar posição; 

- Coletar o dedo; 

- Utilizar menus. 

#### **4.2.5. Combate** 

Não existe combate. Não existem inimigos capazes de matar o personagem. O principal adversário é o próprio percurso. 

#### **4.2.6. Economia** 

O jogo não possui sistema econômico. Não existem dinheiro, loja, itens compráveis ou recursos consumíveis. 

#### **4.2.7. Fluxo de telas** 

Menu Principal → Jogar / Seleção de Fases / Opções / Sair. 

Seleção de Fases → Fazenda / Cidade / Montanhas / Espaço / Dimensão Psicodélica. As fases ainda não liberadas aparecem bloqueadas. 

Durante a fase → HUD / Menu de pausa / Reiniciar / Voltar ao menu. 

Conclusão → Tempo final / Dedo recuperado / Próxima fase / Jogar novamente. 

### **4.3. Opções do jogo** 

- Volume geral; 

- Volume da música; 

- Volume dos efeitos; 

- Sensibilidade do mouse; 

- Resolução; 

- Tela cheia. 

### **4.4. Replay e salvamento** 

O jogo deverá armazenar as fases liberadas, as fases concluídas e o melhor tempo de cada fase. 

Sempre que uma fase for concluída, o tempo atual será comparado com o melhor resultado anterior. Caso o tempo atual seja menor, ele passa a ser salvo como novo recorde. 

Esse sistema aumenta o potencial de replay e incentiva jogadores a dominarem completamente os percursos. 

Página 9 

### **4.5. Easter Eggs e referências** 

Serão colocados pequenos elementos escondidos nos mapas. Eles não interferem na progressão principal. 

Podem existir referências a outros jogos, à disciplina, ao desenvolvimento do próprio projeto, a piadas internas e à história absurda do jogo. 

## **5. Níveis** 

### **5.1. Nível 1 - Fazenda** 

Tema: ambiente rural. 

Função: introduzir todas as mecânicas fundamentais do jogo. 

Dificuldade: baixa a moderada. 

Duração esperada: aproximadamente 5 minutos em uma primeira execução normal. 

O jogador inicia no chão de uma fazenda. Os primeiros saltos são simples. A progressão utiliza estruturas como fardos de feno, cercas, caixas, tratores, telhados, celeiros, silos e moinhos. 

Conforme sobe, os saltos se tornam mais longos e as plataformas mais estreitas. 

Ambientação física (fora da rota, sem alterar a jogabilidade): ao lado do galpão de máquinas, alguns objetos soltos (caixas, fenos e barris) caem girando no início da fase e formam uma pilha que pode ser empurrada; o trator solta fumaça e o balde do poço sobe e desce. 

Objetivo: encontrar o primeiro dedo. 

### **5.2. Nível 2 - Cidade** 

Tema: ambiente urbano. 

Dificuldade: moderada. 

Novidades: maior verticalidade, plataformas móveis, obstáculos giratórios e saltos entre edifícios. O jogador começa próximo às ruas e termina nas regiões superiores dos edifícios. Objetivo: encontrar o segundo dedo. 

### **5.3. Nível 3 - Montanhas** 

Tema: natureza e escalada. 

Dificuldade: moderada para alta. 

O percurso utiliza penhascos, rochas, troncos, pontes, cavernas, cachoeiras e plataformas naturais. As plataformas artificiais diminuem e o terreno passa a fazer parte diretamente do parkour. 

Objetivo: encontrar o terceiro dedo no topo da montanha. 

### **5.4. Nível 4 - Espaço** 

Tema: espacial / tecnologia. 

Dificuldade: alta. 

A fase utilizará plataformas flutuantes e estruturas espaciais. Poderá possuir diferenças de física em algumas regiões, principalmente relacionadas à gravidade. 

Objetivo: encontrar o quarto dedo. 

Página 10 

### **5.5. Nível 5 - Dimensão Psicodélica** 

Tema: surrealismo / horror cósmico / psicodelia. 

Dificuldade: muito alta, porém justa. 

É o nível final e combina os conhecimentos adquiridos nas fases anteriores. 

- Plataformas móveis; 

- Plataformas giratórias; 

- Alterações de gravidade; 

- Objetos flutuantes; 

- Efeitos visuais; 

- Portais; 

- Ilusões; 

- Estruturas aparentemente impossíveis. 

Objetivo: encontrar o quinto e último dedo. Ao coletá-lo, o objetivo principal do jogo é concluído e a cutscene final é iniciada. 

### **5.6. Progressão de habilidade** 

Fase 1 - Fazenda: o jogador aprende movimentação, câmera, pulo, corrida, impulso, controle aéreo e escalada de bordas. 

Fase 2 - Cidade: o jogador combina essas mecânicas em situações mais verticais. 

Fase 3 - Montanhas: a precisão se torna mais importante e o ambiente possui superfícies menos regulares. 

Fase 4 - Espaço: o jogador precisa adaptar habilidades já aprendidas a comportamentos físicos diferentes. 

Fase 5 - Dimensão Psicodélica: todas as mecânicas anteriores são combinadas em desafios mais complexos. 

### **5.7. Progressão de dificuldade** 

A progressão planejada é: 

- Fase 1: aprender; 

- Fase 2: aplicar; 

- Fase 3: dominar; 

- Fase 4: adaptar; 

- Fase 5: combinar tudo. 

Nenhuma fase deverá apresentar dificuldade baseada apenas em tentativa e erro injusta. O jogador deverá conseguir compreender por que caiu e como pode melhorar na próxima tentativa. 

## **6. Interface** 

### **6.1. Sistema visual** 

A câmera será apresentada em terceira pessoa, posicionada atrás do personagem e controlada pelo mouse. 

O jogador deverá conseguir observar o personagem, a plataforma atual, o próximo obstáculo, parte da região abaixo e a direção dos próximos saltos. 

Página 11 

Essa perspectiva também facilita a visualização das animações de caminhada, corrida, pulo, queda, agarrar e escalar. 

#### **HUD** 

Durante as fases, a interface deverá ser minimalista. 

No canto superior esquerdo será exibido o cronômetro da fase. 

Formato sugerido: 03:42.51. 

O cronômetro começa quando o jogador entra na fase. Pressionar R não altera o tempo. Ele termina ao concluir ou abandonar a fase. 

#### **Menu de pausa** 

Aberto com Esc. 

- Continuar; 

- Voltar ao início (reposiciona o jogador sem zerar o cronômetro); 

- Reiniciar fase (recarrega a fase do zero); 

- Menu principal. 

#### **Tela de conclusão** 

- Fase concluída; 

- Dedo recuperado; 

- Tempo final; 

- Indicação de novo recorde quando aplicável; 

- Opção de jogar novamente; 

- Opção de avançar. 

#### **Tela de seleção de fases** 

Cada fase já concluída poderá apresentar seu melhor tempo. Fases ainda não liberadas aparecem bloqueadas. 

### **6.2. Sistema de controle** 

|Comando|Ação|
|---|---|
|W|Andar para frente|
|A|Andar para esquerda|
|S|Andar para trás|
|D|Andar para direita|
|Mouse|Controlar câmera em terceira pessoa|
|Shift|Correr|
|Espaço|Pular|
|Espaço próximo de borda válida|Escalar|
|R|Voltar ao início da fase|
|Esc|Pausar|



### **6.3. Áudio, música e efeitos sonoros** 

Cada fase possuirá ambientação sonora relacionada ao cenário. 

- Fazenda: animais, vento e ambiente rural; 

- Cidade: trânsito, ambiente urbano e máquinas; 

- Montanhas: vento, água e sons naturais; 

- Espaço: sons eletrônicos e música atmosférica; 

- Psicodélica: áudio distorcido, sons incomuns e música ambiente surreal. 

Também existirão efeitos para pulos, quedas, passos, objetos, coleta de dedo e interface. 

Página 12 

### **6.4. Sistema de ajuda** 

O início da primeira fase funcionará como tutorial integrado. 

- WASD - Movimentar; 

- Mouse - Olhar; 

- Shift - Correr; 

- Espaço - Pular; 

- Espaço próximo de borda - Escalar; 

- R - Reiniciar posição. 

Depois das primeiras instruções, o jogador deverá descobrir as mecânicas por experiência própria. 

## **7. Inteligência Artificial** 

### **7.1. IA oponente** 

Não existem inimigos tradicionais. Portanto, não existe IA oponente necessária para o funcionamento principal do jogo. 

### **7.2. NPCs** 

NPCs poderão ser utilizados principalmente para ambientação. 

- Fazendeiro; 

- Moradores da cidade; 

- Animais; 

- Personagens secundários. 

Eles não precisarão participar diretamente da narrativa principal. 

### **7.3. IA de suporte** 

- NavMesh para NPCs; 

- Detecção do jogador; 

- Triggers; 

- Detecção de colisão; 

- Movimentação automática. 

## **8. Técnico** 

### **8.1. Hardware alvo** 

Plataforma principal: computadores Windows. 

Controles: teclado + mouse. 

O projeto deverá conseguir ser executado nos computadores disponíveis no laboratório da universidade. 

### **8.2. Software e desenvolvimento** 

- Engine: Unity 6; 

- Versão de referência: Unity 6000.0.8f1; 

- Template: Universal 3D - Universal Render Pipeline (URP); 

Página 13 

- Linguagem: C#; 

- Desenvolvimento: Unity Hub + Unity Editor. 

### **8.3. Requisitos de rede** 

O jogo será completamente offline. 

- Sem servidor; 

- Sem login; 

- Sem multiplayer; 

- Sem banco de dados remoto; 

- Sem necessidade de conexão constante com a internet. 

## **9. Arte do Jogo** 

A direção artística será baseada principalmente na identidade de cada fase. 

O estilo não precisa buscar realismo extremo. 

Prioridades: 

1. Clareza visual; 

2. Boa leitura das plataformas; 

3. Identidade temática; 

4. Qualidade consistente; 

5. Bom desempenho. 

Os assets utilizados deverão manter alguma coerência visual dentro de cada nível. Cada nível deverá possuir grande variedade de objetos, evitando que o cenário pareça composto apenas por plataformas genéricas. 

## **10. Tema Principal - Skills** 

Para o primeiro bimestre, o tema principal planejado será Skills. 

O conceito está diretamente relacionado ao funcionamento do jogo porque não existem pontos de atributo, melhorias compráveis, equipamentos melhores ou poderes que facilitem automaticamente o progresso. 

Quem melhora é o próprio jogador. 

A evolução ocorre através do desenvolvimento de habilidades como: 

1. Precisão de salto; 

2. Controle de movimentação; 

3. Controle de câmera; 

4. Timing; 

5. Leitura do ambiente; 

6. Memorização; 

7. Adaptação. 

Portanto, a progressão está relacionada diretamente à habilidade adquirida pelo jogador durante a experiência. 

### **10.1. Elementos do tema Skills no Nível 1** 

Cada elemento abaixo existe na fase e exige uma habilidade específica do jogador: 

| # | Elemento no jogo | Habilidade treinada |
|---|---|---|
| 1 | Rota de saltos entre props flutuantes, cada vez mais altos | Precisão de salto |
| 2 | Bordas escaláveis (agarrar e subir) | Coordenação de pulo e escalada |
| 3 | Corrida com impulso (Shift) para os saltos longos | Controle de velocidade |
| 4 | Faróis laranja nas estações | Orientação e memorização do percurso |
| 5 | Cronômetro e melhor tempo salvo | Evolução medida a cada tentativa |

Página 14 

## **11. Filosofia de Dificuldade** 

O jogo deve ser difícil, mas não injusto. 

### **11.1. Clareza** 

O jogador deve compreender o que precisa fazer. 

### **11.2. Consistência** 

As regras de movimentação devem permanecer previsíveis. 

### **11.3. Aprendizado** 

Ao errar, o jogador deve entender o motivo da falha. 

A punição principal será a perda de progresso causada pela queda. Isso significa que um mesmo salto pode provocar consequências diferentes dependendo da altura em que o jogador estiver. 

No início, errar poderá significar perder apenas alguns segundos. Mais tarde, poderá significar perder vários minutos de progresso. 

Essa característica aumenta naturalmente a tensão sem precisar aumentar artificialmente a velocidade ou precisão exigida em todos os obstáculos. 

## **12. Estrutura Estimada de Duração** 

Cada fase terá aproximadamente 5 minutos de duração para uma execução normal sem grandes erros. 

|Fase|Tempo esperado|
|---|---|
|Fazenda|~5 min|
|Cidade|~5 min|
|Montanhas|~5 min|
|Espaço|~5 min|
|Psicodélica|~5 min|



Tempo mínimo aproximado de gameplay principal: 25 minutos. 

Entretanto, considerando quedas e novas tentativas, uma primeira campanha poderá durar significativamente mais. Jogadores experientes poderão completar as fases muito mais rapidamente, permitindo também a prática de speedrun. 

## **13. Loop Principal do Jogo** 

Observar → Planejar → Preparar impulso → Pular → Controlar trajetória → Aterrissar ou agarrar → Escalar ou cair → Aprender → Tentar novamente → Subir → Encontrar o dedo. 

À medida que o jogador domina as mecânicas, poderá substituir uma abordagem cautelosa por movimentos mais rápidos e arriscados. 

Assim, o mesmo percurso poderá ser jogado de maneiras diferentes por jogadores iniciantes e experientes. 

Página 15 

## **14. Objetivo da Experiência** 

Ao finalizar uma fase, o jogador deverá sentir que venceu o desafio através de sua própria habilidade. 

A intenção não é fazer o jogador pensar: “Finalmente tive sorte.” 

Mas sim: “Agora eu sei fazer.” 

Essa sensação de evolução é o elemento central da experiência de jogo. 

## **15. Encerramento** 

Após completar os cinco níveis, uma cutscene encerra o jogo. 

O protagonista chega em casa carregando os cinco dedos recuperados. Ele se aproxima do pai e entrega os dedos. 

Após um breve momento, os dois olham pela janela. 

O pai explica que agora precisam esperar a irmã do protagonista, pois ela ainda está recuperando as pernas e o braço esquerdo, também perdidos durante a mesma noite chuvosa. 

Nesse momento, torna-se evidente que o pai havia perdido muito mais partes do corpo do que aquelas mencionadas durante a aventura. 

A missão do protagonista sempre foi apenas recuperar os dedos da mão direita. 

Essa cena cumpre três objetivos: 

1. Finaliza a missão do protagonista; 

2. Conclui a piada central da narrativa; 

3. Cria uma possibilidade de continuação protagonizada pela irmã. 

Página 16 

## **16. Requisitos Técnicos do 1º Bimestre**

Mapa de cada conteúdo dos tutoriais para o ponto do projeto onde ele aparece.

| Requisito | Onde está no jogo | Script / função |
|---|---|---|
| Colisão | Jogador empurra os objetos soltos do pátio; objetivo e tutoriais usam triggers | `PlayerPusher.OnControllerColliderHit`, `FingerCollectible.OnTriggerEnter`, `TutorialTrigger.OnTriggerEnter` |
| Movimentação com Translate | Balde do poço subindo e descendo | `MovingPlatform.Update` |
| Movimentação com AddForce | Lançamento dos objetos soltos e empurrão do jogador | `SkillChallengeSpawner.Spawn`, `PlayerPusher.OnControllerColliderHit` |
| Rotação | Pás do moinho, câmera, giro do personagem | `WindmillRotor.Update`, `ThirdPersonCamera.LateUpdate`, `PlayerMotor.Update` |
| Torque | Objetos soltos caem girando | `SkillChallengeSpawner.Spawn` (AddTorque) |
| Vetor de prefabs | `GameObject[] prefabs` com Caixa, Feno e Barril soltos (`Assets/_Game/Prefabs/Decoracao`) | `SkillChallengeSpawner` |
| Randomizar a posição no vetor | `Random.Range` sorteia o índice do prefab e do ponto de spawn | `SkillChallengeSpawner.Spawn` |
| Spawn de objetos por função | Função dedicada que instancia o objeto | `SkillChallengeSpawner.Spawn` (Instantiate) |
| Partículas (pedrinhas) | Terra/pedrinhas soltas a cada passo | `PlayerFootsteps.PlayStep` |
| Fumaça | Fumaça contínua saindo do trator | `FarmProjectBuilder.CreateSmoke` (ParticleSystem na cena) |
| Som | Passos na terra | `PlayerFootsteps` |
| Animação (parado, andando, correndo, pulando, caindo) | Blend de locomoção + pulo/queda/aterrissagem + pendurar/subir | `FarmCharacter.controller`, `PlayerAnimator` |
| Scripts | Todos em `Assets/_Game/Scripts`, documentados função a função | — |
| UI: texto e botão | Cronômetro, dicas, menus; botão **Reiniciar fase** no pause e **Jogar novamente** no fim | `GameplayHUD`, `PauseMenu.RestartLevel`, `CompletionPanel.Replay` |
| Tag | `Player` (gatilhos) e `Empurravel` (objetos soltos) | `CompareTag` em `PlayerPusher`, `FingerCollectible`, `TutorialTrigger` |
| Nível 1 com 70+ assets | Fazenda com mais de 1000 instâncias de modelos de categorias variadas (construções, animais, plantações, props, natureza, ilhas) | Cena `Farm.unity` |
