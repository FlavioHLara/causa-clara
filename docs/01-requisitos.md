# Levantamento de Requisitos

**Sistema de Gestão Administrativa e Transparência Financeira para Organizações do Terceiro Setor**
Flávio Henrique Lara Santos — PUC Minas Betim — Bacharelado em Sistemas de Informação

---

## 1. Atores do sistema

- **Visitante** — qualquer pessoa não autenticada. Consulta instituições e dados públicos de transparência.
- **Apoiador** — usuário externo à organização. Registra doações e consulta a transparência das instituições.
- **Gestor Funcionário** — gestor vinculado a uma instituição, com acesso aprovado por um Dono. Opera lançamentos administrativos, financeiros e patrimoniais.
- **Gestor Dono** — criou a instituição. Executa todas as operações do Gestor Funcionário e, adicionalmente, administra o cadastro institucional e decide sobre as solicitações de vínculo.

O tipo de conta (Apoiador ou Gestor) é escolhido no cadastro e é imutável. Já o papel de vínculo decorre da forma de entrada na instituição: quem cria torna-se Dono, quem solicita acesso e é aprovado torna-se Funcionário. Essa separação entre tipo de conta e papel de vínculo é o que permite que um mesmo cadastro de gestor sirva tanto para quem abre a organização quanto para quem é admitido nela.

Como o Gestor Dono executa todas as operações do Gestor Funcionário e acrescenta outras, a relação entre os dois é de especialização: o Dono é um Funcionário com atribuições adicionais. As operações de criar instituição e de solicitar acesso são atribuídas, respectivamente, ao Dono e ao Funcionário, pois é ao concluí-las que o gestor assume cada um desses papéis; ambas têm como pré-condição o gestor ainda não possuir vínculo aprovado (RF16, RN02).

---

## 2. Requisitos Funcionais

### 2.1 Autenticação e Cadastro de Usuários

**RF01** — O sistema deve apresentar uma tela de acesso inicial contendo o formulário de autenticação e o encaminhamento para o cadastro de novos usuários.

**RF02** — O sistema deve permitir que o visitante escolha, no momento do cadastro, entre dois tipos de conta — Apoiador e Gestor — exibindo para cada tipo a descrição das permissões que serão concedidas antes da confirmação.

**RF03** — O sistema deve permitir o cadastro de usuário mediante nome completo, e-mail, senha e telefone opcional, exigindo a confirmação da senha.

**RF04** — O sistema deve impedir o cadastro de mais de um usuário com o mesmo e-mail, informando a ocorrência ao solicitante.

**RF05** — O sistema deve permitir a autenticação por e-mail e senha, concedendo acesso somente às funcionalidades correspondentes ao tipo de conta e ao papel de vínculo do usuário.

**RF06** — O sistema deve encerrar automaticamente a sessão após o período de validade da credencial de acesso, permitindo sua renovação enquanto a sessão estiver ativa.

**RF07** — O sistema deve permitir que o usuário autenticado consulte e atualize seus dados cadastrais de nome e telefone, vedada a alteração do tipo de conta.

**RF08** — O sistema deve permitir que o usuário encerre a sessão.

**RF09** — O sistema deve permitir a redefinição de senha por meio de link enviado ao e-mail cadastrado.

### 2.2 Vínculo Institucional

**RF10** — O sistema deve, ao primeiro acesso de um gestor sem vínculo, apresentar as duas alternativas de entrada: criar uma nova instituição ou solicitar acesso a uma instituição existente.

**RF11** — O sistema deve permitir que o gestor crie uma instituição informando nome, descrição, cidade, estado, e-mail de contato, telefone e site.

**RF12** — O sistema deve atribuir automaticamente o papel de Dono, com vínculo aprovado, ao gestor que criar a instituição.

**RF13** — O sistema deve impedir a criação de instituições com nome duplicado.

**RF14** — O sistema deve permitir que o gestor sem vínculo consulte a relação de instituições cadastradas e envie solicitação de acesso a uma delas.

**RF15** — O sistema deve registrar a solicitação de acesso com situação Pendente e papel pretendido de Funcionário, informando ao solicitante que o acesso depende de aprovação.

**RF16** — O sistema deve impedir que um mesmo usuário mantenha vínculo aprovado com mais de uma instituição simultaneamente.

**RF17** — O sistema deve impedir o envio de solicitação duplicada para a mesma instituição, informando a situação da solicitação anterior, seja ela pendente, aprovada ou rejeitada.

**RF18** — O sistema deve permitir que o Gestor Dono consulte a relação de membros e solicitações de sua instituição, apresentando nome, e-mail, telefone, papel, situação e as datas de solicitação e de resposta.

**RF19** — O sistema deve permitir que o Gestor Dono aprove ou rejeite solicitações pendentes, registrando a data da resposta e o responsável pela decisão.

**RF20** — O sistema deve restringir a aprovação e a rejeição de membros exclusivamente ao Gestor Dono, vedando essa operação ao Gestor Funcionário.

**RF21** — O sistema deve vedar a alteração e a remoção do vínculo do Gestor Dono da instituição.

**RF22** — O sistema deve conceder ao gestor, imediatamente após a aprovação do vínculo, acesso aos módulos administrativo, financeiro e patrimonial da respectiva instituição.

**RF23** — O sistema deve permitir que o Gestor Dono revogue o vínculo de um Gestor Funcionário, preservando o histórico dos lançamentos por ele realizados.

**RF24** — O sistema deve notificar o solicitante, por e-mail, sobre a decisão quanto à sua solicitação de acesso.

### 2.3 Cadastro Institucional

**RF25** — O sistema deve permitir que o Gestor Dono atualize os dados cadastrais da instituição à qual está vinculado.

**RF26** — O sistema deve disponibilizar publicamente, sem exigência de autenticação, a relação de instituições cadastradas, com recursos de busca, ordenação e paginação.

**RF27** — O sistema deve disponibilizar a página pública de cada instituição, contendo identificação, descrição, localidade e dados de contato.

### 2.4 Doações na visão do Apoiador

**RF28** — O sistema deve permitir que o apoiador autenticado registre uma doação destinada a uma instituição cadastrada.

**RF29** — O sistema deve permitir o registro de doações de dois tipos: financeira, com valor monetário, e material, com item, quantidade, unidade e valor estimado.

**RF30** — O sistema deve exigir valor maior que zero para doações financeiras e exigir a identificação do item ou a descrição para doações materiais.

**RF31** — O sistema deve vincular automaticamente a doação registrada ao apoiador autenticado e à data do registro.

**RF32** — O sistema deve permitir que o apoiador consulte o histórico de suas doações, apresentando instituição destinatária, tipo, valor, item e data, com ordenação e paginação.

**RF33** — O sistema deve apresentar ao apoiador um painel com os totais consolidados de suas contribuições, contendo valor total doado, quantidade de doações e instituições apoiadas.

**RF34** — O sistema deve permitir que o apoiador opte pelo anonimato na divulgação pública de cada doação registrada.

**RF35** — O sistema deve permitir que o apoiador emita comprovante da doação registrada, em formato PDF.

### 2.5 Gestão de Receitas

**RF36** — O sistema deve permitir que o gestor registre doações recebidas pela instituição, inclusive aquelas originadas fora do sistema, informando um doador cadastrado ou apenas o nome do doador.

**RF37** — O sistema deve permitir que o gestor consulte, filtre, ordene e pagine as doações recebidas pela sua instituição, apresentando a identificação do doador, o tipo, o valor e a data.

**RF38** — O sistema deve permitir que o gestor altere ou exclua lançamentos de doação da sua instituição, registrando a autoria da operação.

**RF39** — O sistema deve restringir o acesso aos lançamentos de doação exclusivamente aos gestores vinculados à instituição proprietária do registro.

**RF40** — O sistema deve recusar lançamentos de doação cujo doador indicado não possua conta do tipo Apoiador.

### 2.6 Gestão de Despesas e Comprovantes

**RF41** — O sistema deve permitir que o gestor registre despesas da instituição, informando número do documento fiscal, data de emissão, valor total, descrição e categoria.

**RF42** — O sistema deve permitir o anexo de arquivo digitalizado do comprovante a cada despesa registrada.

**RF43** — O sistema deve permitir a consulta, a alteração e a exclusão de despesas da instituição, com filtros por período, categoria e valor.

**RF44** — O sistema deve permitir a associação de uma despesa à doação que a custeou, possibilitando o rastreamento da aplicação dos recursos.

**RF45** — O sistema deve manter um cadastro de categorias de receita e de despesa, permitindo a classificação dos lançamentos.

**RF46** — O sistema deve impedir a exclusão de categoria utilizada por algum lançamento.

### 2.7 Gestão de Estoque e Bens Patrimoniais

**RF47** — O sistema deve manter um cadastro de categorias de itens de estoque, tais como alimentos básicos, medicamentos, produtos de higiene e vestuário, permitindo a classificação e o agrupamento dos bens da instituição.

**RF48** — O sistema deve permitir que o gestor cadastre itens de estoque informando nome, descrição, categoria, unidade de medida, quantidade inicial, quantidade mínima de referência e preço unitário de referência.

**RF49** — O sistema deve permitir que o gestor consulte, busque, filtre por categoria, ordene e pagine os itens de estoque da sua instituição, apresentando a quantidade atual, a quantidade mínima e a indicação de situação crítica.

**RF50** — O sistema deve permitir a alteração dos dados cadastrais de um item de estoque e a sua inativação, vedada a exclusão definitiva de item que possua movimentações registradas.

**RF51** — O sistema deve restringir o acesso aos itens de estoque e às suas movimentações exclusivamente aos gestores vinculados à instituição proprietária do registro.

**RF52** — O sistema deve permitir que o gestor registre movimentações de estoque de dois tipos, entrada e saída, informando a quantidade movimentada e observação opcional.

**RF53** — O sistema deve atualizar automaticamente a quantidade atual do item a cada movimentação registrada, somando as entradas e subtraindo as saídas.

**RF54** — O sistema deve impedir o registro de movimentação de saída cuja quantidade seja superior à quantidade disponível do item, informando a indisponibilidade ao gestor.

**RF55** — O sistema deve registrar, para cada movimentação, a data e o gestor responsável pela operação, constituindo registro imutável.

**RF56** — O sistema deve permitir que a doação material recebida seja convertida em entrada de estoque, vinculando o lançamento de doação ao item correspondente.

**RF57** — O sistema deve permitir a consulta do histórico de movimentações de cada item, apresentando tipo, quantidade, observação, data e gestor responsável, com ordenação e paginação.

**RF58** — O sistema deve gerar relatório de movimentações de estoque com filtros por período, por tipo de movimentação e por item, apresentando a relação dos registros e os totais de quantidade e de valor das entradas e das saídas.

**RF59** — O sistema deve sinalizar como crítico todo item cuja quantidade atual seja igual ou inferior à sua quantidade mínima de referência.

**RF60** — O sistema deve gerar alerta automaticamente no momento em que uma movimentação de saída fizer a quantidade atual do item alcançar ou cruzar a quantidade mínima de referência.

**RF61** — O sistema deve apresentar ao gestor a relação dos alertas de estoque não lidos e permitir que cada alerta seja marcado como lido.

**RF62** — O sistema deve apresentar a valorização do estoque, calculada a partir da quantidade atual e do preço unitário de referência, consolidada por item, por categoria e no total da instituição.

### 2.8 Transparência Pública

**RF63** — O sistema deve disponibilizar, para cada instituição, um painel público de transparência acessível sem autenticação.

**RF64** — O painel de transparência deve apresentar indicadores consolidados do período corrente: total arrecadado, quantidade de doações, total de despesas e quantidade de documentos fiscais registrados.

**RF65** — O painel de transparência deve apresentar a relação das doações recebidas pela instituição, com tipo, valor, descrição e data.

**RF66** — O sistema deve divulgar publicamente a identificação do doador de forma não integral, apresentando apenas o primeiro nome, ou como Anônimo, conforme a opção exercida no RF34.

**RF67** — O painel de transparência deve apresentar a relação das despesas da instituição, com documento fiscal, valor, data de emissão, descrição e acesso ao comprovante anexado.

**RF68** — O painel de transparência deve apresentar a evolução temporal de receitas e despesas em representação gráfica, agrupada por mês.

**RF69** — O painel de transparência deve apresentar a distribuição percentual das despesas por categoria em representação gráfica.

**RF70** — O painel de transparência deve apresentar a composição dos bens em estoque por categoria e a respectiva valorização consolidada, demonstrando a destinação dos recursos e das doações materiais recebidas.

**RF71** — O sistema deve permitir a filtragem dos dados de transparência por período e por categoria.

**RF72** — O sistema deve permitir a exportação dos dados públicos de transparência em formato aberto, CSV, e em formato de leitura, PDF.

**RF73** — O painel de transparência deve apresentar o saldo do período, correspondente às receitas menos as despesas, e o percentual de recursos aplicados nas atividades-fim.

### 2.9 Painéis e Prestação de Contas na visão interna

**RF74** — O sistema deve apresentar ao gestor um painel administrativo com os indicadores consolidados da instituição no período corrente.

**RF75** — O painel administrativo deve destacar os itens em quantidade crítica, ordenados pela menor quantidade disponível, informando a quantidade atual, a quantidade mínima e a categoria de cada item.

**RF76** — O painel administrativo deve apresentar a quantidade de alertas de estoque não lidos e a quantidade de solicitações de vínculo pendentes de decisão.

**RF77** — O sistema deve gerar relatório de doações recebidas por período e por tipo, apresentando a relação dos lançamentos e os totais por tipo e geral.

**RF78** — O sistema deve gerar relatório de despesas por período e por categoria, apresentando a relação dos lançamentos e os respectivos totais.

**RF79** — O sistema deve gerar demonstrativo consolidado de prestação de contas do período, contendo receitas, despesas, saldo, totais por categoria e valorização dos bens em estoque.

**RF80** — O sistema deve permitir a exportação dos relatórios gerados em CSV e PDF.

**RF81** — O sistema deve registrar em trilha de auditoria as operações de criação, alteração e exclusão de lançamentos financeiros e de movimentações de estoque, identificando autor, data e operação.

### 2.10 Delimitação de escopo

Não integram o escopo desta versão, por não contribuírem para o objetivo de gestão administrativa e transparência financeira: processamento de pagamentos por meio de instituição financeira ou *gateway*; gestão de vagas de voluntariado e candidaturas; emissão de documentos fiscais; integração com sistemas contábeis externos; controle de bens imóveis e cálculo de depreciação patrimonial; e aplicativo móvel nativo.

O registro de doação financeira é declaratório — o sistema registra e publica a doação informada, sem intermediar a transferência de recursos. A intermediação exigiria conformidade com as normas do setor de pagamentos, o que extrapola o objetivo do trabalho e deslocaria o foco da transparência para a transação.

---

## 3. Regras de Negócio

**RN01** — O tipo de conta, Apoiador ou Gestor, é definido no cadastro e não pode ser alterado posteriormente. *(RF02, RF07)*

**RN02** — Cada usuário pode manter, no máximo, um vínculo aprovado com uma instituição. *(RF16)*

**RN03** — O papel de Dono é concedido unicamente ao criador da instituição e não é transferível na versão avaliada. *(RF12, RF21)*

**RN04** — Toda solicitação de acesso nasce com situação Pendente e só produz efeito após a decisão de um Dono. *(RF15, RF19)*

**RN05** — Lançamentos financeiros e patrimoniais pertencem a uma única instituição e são visíveis, em sua forma integral, apenas aos gestores vinculados a ela. *(RF39, RF51)*

**RN06** — Doação financeira exige valor maior que zero; doação material exige item ou descrição. *(RF30)*

**RN07** — Na doação material, o campo valor representa a estimativa monetária do bem e é computado nos totais de arrecadação. *(RF29, RF64)*

**RN08** — Dados de transparência são sempre públicos e de leitura; nenhuma operação de escrita é permitida sem autenticação. *(RF63, RF66)*

**RN09** — A identificação integral do doador nunca é divulgada publicamente. *(RF66)*

**RN10** — Lançamentos financeiros excluídos são removidos logicamente, preservando a trilha de auditoria. *(RF38, RF81)*

**RN11** — A quantidade atual de um item é resultado exclusivo do saldo inicial e das movimentações registradas, não sendo editável de forma direta. *(RF53)*

**RN12** — A quantidade de uma movimentação de saída não pode exceder a quantidade disponível do item, de modo que o estoque nunca assuma valor negativo. *(RF54)*

**RN13** — Considera-se em situação crítica o item cuja quantidade atual seja igual ou inferior à sua quantidade mínima de referência. *(RF59, RF75)*

**RN14** — O alerta de estoque crítico é gerado uma única vez, no momento em que a quantidade cruza a quantidade mínima de referência, e não a cada movimentação subsequente, de modo a evitar a repetição de avisos para o mesmo item. *(RF60)*

**RN15** — Movimentações de estoque são registros imutáveis; eventuais correções são efetuadas por meio de nova movimentação em sentido contrário, com a devida observação. *(RF55)*

**RN16** — Item de estoque que possua movimentações registradas não pode ser excluído, admitindo-se apenas a sua inativação, de forma a preservar o histórico patrimonial. *(RF50)*

---

## 4. Requisitos Não Funcionais

### 4.1 Usabilidade

**RNF01** — A interface deve ser responsiva, operando sem perda de funcionalidade em resoluções a partir de 360 pixels de largura.

**RNF02** — O painel público de transparência deve ser compreensível por usuário sem formação contábil, apresentando os indicadores por meio de rótulos em linguagem corrente e de representações gráficas.

**RNF03** — O sistema deve obter escala SUS (*System Usability Scale*) média igual ou superior a 68 pontos em avaliação com usuários.

**RNF04** — O registro de uma doação pelo apoiador deve ser concluído em, no máximo, quatro interações a partir do painel inicial.

**RNF05** — O sistema deve apresentar mensagens de erro específicas e em português, indicando a correção necessária, sem exposição de informação técnica.

**RNF06** — Toda a interface deve ser apresentada em português do Brasil.

**RNF07** — O painel público deve possuir contraste adequado para leitura e permitir a navegação entre os elementos interativos por meio do teclado.

**RNF08** — A situação crítica de estoque deve ser comunicada por mais de um recurso visual, não se limitando ao uso de cor, de modo a permanecer perceptível a usuários com deficiência na percepção de cores.

### 4.2 Desempenho e Eficiência

**RNF09** — Toda listagem deve ser paginada no servidor, limitada a cem registros por página, vedado o retorno integral de coleções.

### 4.3 Segurança

**RNF10** — As senhas devem ser armazenadas exclusivamente como resumo criptográfico produzido por função adaptativa com sal individual, vedado o armazenamento reversível.

**RNF11** — O controle de acesso deve ser aplicado no servidor, por tipo de conta e por papel de vínculo, não bastando a restrição na interface.

**RNF12** — Toda requisição a recurso de instituição deve validar, no servidor, o vínculo do usuário autenticado com aquela instituição, impedindo o acesso a dados de terceiros por manipulação de identificadores.

**RNF13** — A autenticação deve utilizar credencial assinada, com validade máxima de oito horas e renovação explícita.

**RNF14** — Toda a comunicação entre cliente e servidor deve ocorrer sobre HTTPS.

**RNF15** — Toda entrada deve ser validada no servidor quanto a tipo, obrigatoriedade, formato e tamanho, independentemente da validação realizada no cliente.

**RNF16** — O acesso a dados deve utilizar consultas parametrizadas, de modo a evitar injeção de SQL.

**RNF17** — Os arquivos de comprovante aceitos devem restringir-se aos formatos PDF, PNG e JPEG, com limite de cinco megabytes por arquivo e verificação do tipo real do conteúdo.

**RNF18** — Credenciais e segredos de configuração devem ser providos por variáveis de ambiente, vedada sua inclusão no repositório de código.

### 4.4 Privacidade e Conformidade Legal

**RNF19** — O tratamento de dados pessoais deve observar a Lei nº 13.709/2018, Lei Geral de Proteção de Dados Pessoais, limitando-se à finalidade de gestão e transparência declarada no cadastro.

**RNF20** — Os dados divulgados publicamente devem restringir-se às informações financeiras e patrimoniais agregadas e à identificação parcial ou anônima do doador, vedada a publicação de e-mail, telefone e nome integral.

**RNF21** — O sistema deve permitir ao titular a solicitação de exclusão de seus dados pessoais, preservando os lançamentos financeiros em forma anonimizada, em razão do interesse público da prestação de contas.

**RNF22** — O sistema deve apresentar aviso de privacidade no cadastro, informando quais dados serão divulgados publicamente.

**RNF23** — Os dados de estoque divulgados publicamente devem restringir-se a quantidades e valores agregados por categoria, vedada a publicação de informação que permita identificar beneficiários das saídas de itens.

### 4.5 Confiabilidade

**RNF24** — Os valores monetários devem ser armazenados em tipo decimal de precisão fixa, vedado o uso de ponto flutuante binário.

**RNF25** — As operações que alteram mais de um registro devem ser executadas em transação atômica.

**RNF26** — O registro da movimentação, a atualização da quantidade do item e a eventual geração do alerta de estoque crítico devem ocorrer em uma única transação, de modo que o saldo do estoque nunca divirja do histórico de movimentações.

**RNF27** — O banco de dados deve declarar integridade referencial entre lançamentos, movimentações, itens, instituições, usuários e categorias.

**RNF28** — As datas e horas devem ser persistidas em UTC e apresentadas no fuso horário local do usuário.

**RNF29** — O sistema deve registrar em log as falhas de execução, com data, operação e identificação do usuário, sem registro de dados sensíveis.

### 4.6 Manutenibilidade e Portabilidade

**RNF30** — A solução deve adotar arquitetura com separação entre cliente e servidor, comunicando-se por API REST, de modo que o painel público e o painel administrativo possam evoluir de forma independente.

**RNF31** — O servidor deve expor documentação interativa da API, em conformidade com a especificação OpenAPI, no ambiente de desenvolvimento.

**RNF32** — O código deve observar a separação de responsabilidades entre as camadas de apresentação, de aplicação e de persistência, vedado o acesso direto ao banco de dados pela camada de apresentação.

**RNF33** — O ambiente completo, composto por aplicação cliente, API e banco de dados, deve ser instanciável por meio de conteinerização, com um único comando.

**RNF34** — A aplicação cliente deve operar nas versões correntes dos navegadores Chrome, Firefox e Edge.

**RNF35** — O esquema do banco de dados deve ser versionado por meio de *scripts* de migração aplicáveis de forma incremental.

**RNF36** — O código-fonte deve ser mantido em repositório sob controle de versão Git, com histórico de alterações.

### 4.7 Restrições de Implementação

**RNF37** — A interface deve ser implementada em React.js com Vite e TypeScript.

**RNF38** — A API deve ser implementada em C# com ASP.NET Core.

**RNF39** — A persistência deve utilizar o sistema gerenciador de banco de dados relacional PostgreSQL.

**RNF40** — A solução deve ser executável em infraestrutura de custo nulo ou reduzido, considerando a restrição orçamentária característica das organizações do terceiro setor.

## 5. Rastreabilidade entre objetivos específicos e requisitos

- **Objetivo ii)** identificar requisitos funcionais e não funcionais — documento integral.
- **Objetivo iii)** projetar e implementar funcionalidades para registro, organização e acompanhamento de informações administrativas e financeiras — RF01 a RF62 e RF74 a RF81.
- **Objetivo iv)** desenvolver mecanismos de apresentação e consulta das informações financeiras, favorecendo a transparência — RF63 a RF73, RNF02 e RNF20.
- **Objetivo v)** avaliar a solução quanto ao atendimento dos requisitos e à adequação ao contexto — RNF03 (escala SUS) e RNF04 (número máximo de interações).

---

## 6. Síntese quantitativa

- Requisitos funcionais: **81**
- Requisitos não funcionais: **40**
- Regras de negócio: **16**

> Esta numeração é a mesma do artigo (Entrega 3). Qualquer alteração aqui precisa ser refletida lá, e vice-versa — o documento de arquitetura cita estes números.
