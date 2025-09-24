# FCG Payments – API de Pagamentos

API ASP.NET Core responsável por receber solicitações de compra, consultar/cancelar pagamentos e registrar a trilha de eventos de cada pagamento. O projeto é organizado em camadas (API, Business, Infra e Core) e utiliza Entity Framework Core com SQL Server.

## ✨ Principais recursos
- Solicitar compra, consultar e cancelar pagamentos.
- Atualização incremental do status com base na versão do último evento.
- Registro de eventos de domínio em cada operação (consulta, atualização, cancelamento etc.).
- Swagger para documentação e teste rápido dos endpoints.

## 📦 Requisitos
- .NET 8 SDK
- SQL Server (para execução local da API)

## ▶️ Como rodar (dev)
- Restaurar e compilar
  - `dotnet restore`
  - `dotnet build`
- Configurar a conexão com o banco
  - Ajuste a connection string `DefaultConnection` em `src/FCG.Pagamentos/appsettings.json`.
- Executar a API
  - `dotnet run --project src/FCG.Pagamentos`
  - Swagger: `https://localhost:{PORT}/swagger` (porta exibida no console)
- Rodar os testes
  - `dotnet test`

## 🐳 Docker
- Build da imagem da API
  - `docker build -t fcg-pagamentos:latest .`
- Subir com docker-compose (API + SQL Server)
  - `docker compose up -d`
  - API em `http://localhost:8080` (Swagger em `/swagger`)
  - SQL Server exposto em `localhost:1433` (senha default de dev em `docker-compose.yml`)
- Variáveis importantes
  - `ConnectionStrings__DefaultConnection` aponta para `sqlserver` (container) com `TrustServerCertificate=True`
  - `ASPNETCORE_ENVIRONMENT=Development` habilita detalhes e Swagger
- Parar/limpar
  - `docker compose down` (use `-v` para remover volume do banco)

## 🗂️ Arquitetura e Pastas
- `src`
  - `FCG.Pagamentos` (API)
    - Controllers: endpoints REST (solicitar, atualizar, listar/consultar, cancelar)
    - MappingDtos: mapeamentos entre DTOs e domínio e criação de eventos
    - Middlewares: request logging + error handling
    - Configuração: Program/Swagger e appsettings/serilog.json
  - `FCG.Pagamentos.Business` (Domínio)
    - Model: entidades de domínio
    - Services: regras de negócio (pagamentos e eventos)
    - Interfaces: contratos de serviços e repositórios
    - Logging: `Logging/LoggingScopes.cs` (helper para escopos de log)
  - `FCG.Pagamentos.Infra` (Infraestrutura)
    - Data: ApplicationDbContext e mapeamentos do EF Core
    - Repositories: implementações de acesso a dados (Payment e PaymentEvent)
    - IoC: registro de dependências (DbContext, repositórios, serviços)
  - `FCG.Pagamentos.Core` (Core)
    - Utilitários de apoio (evolução de eventos, etc.)
- `tests`
  - `FCG.Pagamentos.Tests`
    - TestClasses: testes unitários de controllers, serviços e middlewares
    - Infra: fábrica de DbContext para testes

### Atualizações recentes (logging e middlewares)
- API (`src/FCG.Pagamentos`)
  - `Middlewares/RequestLoggingMiddleware.cs`: adiciona `CorrelationId`, `PaymentId` e `UserId` ao LogContext durante todo o request; logs de início/fim com tempo.
  - `Middlewares/ErrorHandlingMiddleware.cs`: captura exceções e retorna `ProblemDetails` com `correlationId`; registra erro com contexto.
  - `serilog.json` + `Program.cs`: configuração do Serilog (console JSON, níveis, enrichers, LogContext) e registro dos middlewares.
- Business (`src/FCG.Pagamentos.Business`)
  - `Logging/LoggingScopes.cs`: helper `BeginPaymentScope(...)` para incluir `ClassName`, `MethodName`, `PaymentId`, `UserId`.
  - `Services/PaymentService.cs`, `Services/PaymentEventService.cs`: uso de `ILogger<T>` + helper para logs padronizados.
- Tests
  - Ajustados para injetar `NullLogger<T>.Instance` e novos testes de middlewares.

## 🔌 Endpoints e Estruturas

Cabeçalhos comuns (opcional)
- `X-Correlation-Id`: GUID/UUID para rastrear logs do request. Se não informado, a API gera e devolve no mesmo header.

Tabela de rotas

| Método | Rota                                                        | Body (request)             | 200 OK (response)             |
|--------|-------------------------------------------------------------|----------------------------|-------------------------------|
| POST   | `api/Payment/SolicitacaoCompra`                             | `PaymentDto`               | `PaymentResponse`             |
| POST   | `api/Payment/CancelarPagamento`                             | `PaymentRequestDto`        | `PaymentResponse`             |
| PUT    | `api/Payment/AtualizarPagamento`                            | `Guid` (PaymentId)         | `void` (200)                  |
| GET    | `api/Payment/ListarComprasUsuario/{paymentId}/{userId}`     | —                          | `Payment`                     |
| GET    | `api/Payment/ConsultarPagamento/{paymentId}/{userId}`       | —                          | `PaymentResponse`             |
| GET    | `api/Payment/PagamentoAnalise`                              | —                          | `List<Payment>`               |
| GET    | `api/PaymentEvents`                                         | —                          | `IEnumerable<PaymentEvent>`   |
| GET    | `api/PaymentEvents/type/{eventType}`                        | —                          | `IEnumerable<PaymentEvent>`   |
Estruturas
- `PaymentDto`
  {
    "userId": "GUID",
    "currency": "BRL",
    "items": [ { "jogoId": "GUID", "description": "string", "unitPrice": number, "quantity": number } ]
  }
- `PaymentRequestDto`
  {
    "paymentId": "GUID",
    "userId": "GUID",
    "statusPayment": "string" // opcional
  }
- `PaymentResponse`
  {
    "paymentId": "GUID",
    "statusPayment": "string",
    "success": true,
    "message": "string"
  }
- `Payment`
  {
    "paymentId": "GUID",
    "userId": "GUID",
    "currency": "string",
    "statusPayment": "string",
    "items": [ { "paymentItemId": "GUID", "paymentId": "GUID", "jogoId": "GUID", "description": "string", "unitPrice": number, "quantity": number, "totalPrice": number } ],
    "totalAmount": number,
    "createdAt": "ISO-8601"
  }

## 🧩 Eventos e Versionamento
- Tipos de evento/status mapeados no domínio.
- Criação de eventos descritivos (CONSULTA, ATUALIZACAO, CANCELED etc.).
- Serviço de eventos controla versão e evita duplicidade consecutiva do mesmo tipo.
- Heurística de status final por versão para simular progressão do pagamento.

## ✅ Critérios de Aceite (resumo)
- Solicitação de compra: 200 com `PaymentResponse`; evento inicial registrado.
- Atualização: 200 e evento `ATUALIZACAO`; 404 quando não encontrar.
- Consulta/Listagem: 200 e evento `CONSULTA`; 404 quando não encontrar.
- Cancelamento: 200 com `CANCELED`; 400 em falhas; sem evento em erro.

## 🛠️ Comandos úteis
- Rodar API: `dotnet run --project src/FCG.Pagamentos`
- Testes: `dotnet test`
- Restaurar: `dotnet restore`
- Build: `dotnet build`

## 🧱 Migrations (EF Core)

Pré‑requisito (opcional):
- `dotnet tool install --global dotnet-ef`
- Verificar: `dotnet ef --version`

Gerar uma nova migration (ex.: Initial):
- Na raiz do repositório:
  - `dotnet ef migrations add Initial -p src/FCG.Pagamentos.Infra -s src/FCG.Pagamentos -c ApplicationDbContext`

Atualizar o banco de dados:
- `dotnet ef database update -p src/FCG.Pagamentos.Infra -s src/FCG.Pagamentos -c ApplicationDbContext`

Observações:
- `-p` aponta para o projeto da Infra (Migrations/DbContext).
- `-s` aponta para o projeto de inicialização (API) que contém a connection string.
- Ajuste a connection string em `src/FCG.Pagamentos/appsettings.json` ou via `ConnectionStrings__DefaultConnection`.



