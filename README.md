# OrderFlow API

API REST mínima para criar, consultar e atualizar o status de pedidos de e-commerce. O projeto serve como experiência prática de portfólio alinhada a vagas de estágio que valorizam C#, .NET, APIs web, orientação a objetos, Git, testes automatizados e domínio de e-commerce.

## Stack

- .NET 8 e C#
- ASP.NET Core Minimal API
- Persistência local em JSON
- xUnit

## Estrutura

```text
orderflow-dotnet-api/
├── src/OrderFlow.Api/       # API, domínio, serviço e repositório JSON
├── tests/OrderFlow.Tests/   # Testes unitários do serviço
├── OrderFlow.sln
└── README.md
```

## Como executar

Pré-requisito: SDK do .NET 8 instalado. Na raiz do projeto, execute:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/OrderFlow.Api
```

A API fica disponível em `http://localhost:5000`. Os pedidos são mantidos em `src/OrderFlow.Api/Data/orders.json` entre execuções.

## Como testar

Execute toda a suíte:

```bash
dotnet test
```

Os testes cobrem criação com status inicial, validação de valor, rejeição de status inválido e consulta de pedido inexistente.

## Exemplos de requisições

Criar um pedido:

```bash
curl -i -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"cliente":"Ana Silva","valorTotal":149.90}'
```

Listar pedidos:

```bash
curl -i http://localhost:5000/orders
```

Consultar um pedido (substitua o valor de `ID` pelo identificador retornado na criação):

```bash
ID="00000000-0000-0000-0000-000000000000"
curl -i "http://localhost:5000/orders/$ID"
```

Atualizar o status:

```bash
ID="00000000-0000-0000-0000-000000000000"
curl -i -X PATCH "http://localhost:5000/orders/$ID/status" \
  -H "Content-Type: application/json" \
  -d '{"status":"Processing"}'
```

Status aceitos: `Pending`, `Processing`, `Shipped` e `Cancelled`.

## Decisões técnicas

- O serviço concentra validações e regras, permitindo testes sem iniciar o servidor.
- O repositório serializa enums como texto, protege acessos concorrentes no processo e substitui o JSON somente após escrever um arquivo temporário.
- Um arquivo inexistente é criado com `[]`. Arquivo vazio ou JSON inválido gera erro explícito e não é sobrescrito silenciosamente.
- Datas são registradas em UTC e nomes JSON usam `camelCase`. Status são aceitos sem diferenciar maiúsculas de minúsculas.
- Um middleware único converte falhas inesperadas em resposta curta, sem expor stack trace.

## Limitações

A persistência em arquivo é adequada apenas ao uso local e a uma instância da API. Não há autenticação, paginação, histórico ou regras de transição entre status, conforme o escopo do MVP.

## Melhoria futura

Como evolução, a persistência poderia ser migrada para um banco relacional e os endpoints cobertos por testes de integração, preservando as regras do serviço.
