# OrderFlow

Projeto pessoal de estudo e portfólio que demonstra uma solução web pequena para gerenciamento de pedidos de e-commerce. A aplicação permite criar pedidos, consultar a lista persistida e atualizar o status de cada pedido.

O projeto foi pensado para praticar competências em: C#, .NET, APIs REST, integração HTTP, Angular, TypeScript, HTML, SCSS, orientação a objetos e testes automatizados. 

**Demonstração online:** [coletivopindorama.com.br/orderflow/](https://coletivopindorama.com.br/orderflow/)
## Stack

**Backend**

- .NET 8 e C#
- ASP.NET Core Minimal API
- Persistência local em JSON
- xUnit

**Interface web**

- Angular 22 com componentes standalone
- TypeScript e Reactive Forms
- Angular HttpClient
- Angular Material e Angular CDK
- SCSS e tema Material próprio
- Vitest e HttpTestingController

Angular 22 foi adotado porque é a versão compatível com o Node.js 24 disponível no ambiente de desenvolvimento. O escopo e as APIs utilizadas permanecem equivalentes ao MVP solicitado para Angular 19.

## Arquitetura resumida

```text
orderflow-dotnet-api/
├── src/OrderFlow.Api/
│   ├── Contracts/          # Corpos das requisições
│   ├── Domain/             # Pedido e status
│   ├── Repositories/       # Persistência no arquivo JSON
│   ├── Services/           # Validações e regras do pedido
│   ├── Data/orders.json
│   └── Program.cs          # Endpoints da Minimal API
├── tests/OrderFlow.Tests/  # Testes unitários do backend
└── frontend/orderflow-web/
    ├── proxy.conf.json     # Proxy para a API local
    └── src/
        ├── themes/         # Tema próprio do Angular Material
        └── app/
            ├── core/       # Modelos TypeScript e OrderService
            └── features/   # Formulário e listagem de pedidos
```

O front-end não contém pedidos fixos. Toda informação exibida vem da API real.

## Pré-requisitos

- SDK .NET 8
- Node.js 22.22 ou 24.15 ou superior compatível
- npm 11 ou versão compatível

## Como executar

Inicie o backend antes da interface.

Terminal 1, na raiz do repositório:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/OrderFlow.Api
```

Terminal 2:

```bash
cd frontend/orderflow-web
npm install
npm start
```

URLs locais:

- Backend: `http://localhost:5000`
- Interface web: `http://localhost:4200`

Durante o desenvolvimento, o Angular chama apenas rotas relativas como `orders`. O navegador resolve a chamada como `/orders`, e o proxy a encaminha para `http://localhost:5000`; portanto, não foi necessário alterar o backend nem habilitar CORS.

Em produção, a demonstração também pode ser compilada para o subcaminho `/orderflow/`:

```bash
cd frontend/orderflow-web
npm run build:hostinger
```

Esse build mantém as chamadas HTTP relativas ao endereço da página, permitindo que a hospedagem sirva a interface e uma API compatível sob `https://coletivopindorama.com.br/orderflow/`.

Como a hospedagem compartilhada da demonstração não executa processos .NET, a produção utiliza um adaptador PHP isolado com os mesmos contratos HTTP e persistência JSON. A implementação principal e a execução local continuam usando a API ASP.NET Core deste repositório.

## Testes e build

Backend, a partir da raiz:

```bash
dotnet test
```

Front-end:

```bash
cd frontend/orderflow-web
npm test
npm run build
```

Os testes do front-end verificam as requisições `GET`, `POST` e `PATCH` do `OrderService` com `HttpTestingController`, além de confirmar que o formulário inválido não tenta criar um pedido.

## Fluxo de dados

```text
Formulário Angular Material
→ componente Angular
→ OrderService
→ requisição HTTP pelo proxy
→ ASP.NET Core Minimal API
→ serviço de domínio
→ repositório e arquivo JSON
→ resposta HTTP
→ atualização da interface
→ feedback com MatSnackBar
```

## Interface web

A página única possui:

- Barra superior e apresentação do projeto.
- Formulário reativo com validações de cliente e valor.
- Listagem responsiva em cards, sem rolagem horizontal em telas pequenas.
- Valor em reais, data formatada, identificador reduzido e status textual.
- Seleção dos quatro status aceitos pela API.
- Estados de carregamento, lista vazia e erro com nova tentativa.
- Feedback de sucesso ou falha por `MatSnackBar`.

O Angular Material fornece toolbar, cards, campos, botões, selects, chips, ícones, indicadores de progresso e snackbars. O tema Material fica isolado em `src/themes/orderflow-theme.scss`; estilos globais e variáveis ficam em `src/styles.scss`; cada componente mantém apenas seu layout e responsividade em SCSS local.

### Captura de tela

> Espaço reservado: adicionar uma captura da interface executando em `http://localhost:4200`.

## Endpoints

| Método | Rota | Ação |
|---|---|---|
| `POST` | `/orders` | Cria um pedido com status `Pending` |
| `GET` | `/orders` | Lista os pedidos |
| `GET` | `/orders/{id}` | Consulta um pedido |
| `PATCH` | `/orders/{id}/status` | Atualiza o status |

Status aceitos: `Pending`, `Processing`, `Shipped` e `Cancelled`.

Exemplo de criação direta pela API:

```bash
curl -i -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"cliente":"Ana Silva","valorTotal":149.90}'
```

## Decisões técnicas

- O front-end reflete os nomes reais do backend: `cliente`, `valorTotal`, `status` e `criadoEm`.
- O `OrderService` centraliza o prefixo `/orders`, as chamadas HTTP e mensagens seguras para erros comuns.
- Status e seus rótulos ficam em um único modelo TypeScript, evitando mapeamentos duplicados.
- Após criar um pedido, o componente principal solicita nova leitura da API; após atualizar status, a resposta substitui apenas o pedido correspondente.
- Em falha na atualização, o pedido conserva o status anterior.
- O backend usa escrita temporária e bloqueio local para reduzir o risco de corrupção do JSON.
- Datas são registradas em UTC no backend e exibidas no formato brasileiro pela interface.

## Limitações

- A persistência em arquivo é adequada somente a uma instância local da API.
- Não existem autenticação, paginação, busca, exclusão ou regras de transição entre status, conforme o escopo do MVP.
- O proxy é uma configuração de desenvolvimento; um deploy real exigiria configuração própria de hospedagem e origem.
- A demonstração Hostinger usa uma camada de compatibilidade PHP; ela não substitui o backend .NET usado como implementação principal do projeto.
- As fontes e os ícones Material são carregados pelo Google Fonts e precisam de conexão para a aparência completa.

Uma evolução possível seria substituir o JSON por banco relacional e adicionar testes de integração dos endpoints, preservando as regras do serviço.
