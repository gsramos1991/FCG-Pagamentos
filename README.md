# FCG Payments – API de Pagamentos

API ASP.NET Core responsável por receber solicitações de compra, consultar/cancelar pagamentos e registrar a trilha de eventos de cada pagamento. O projeto é organizado em camadas (API, Business, Infra e Core) e utiliza Entity Framework Core com SQL Server.

## ✨ Principais recursos
- **Vínculo com Pedidos**: Cada pagamento agora está associado a um `OrderId`, garantindo rastreabilidade entre os sistemas.
- **Gestão de Pagamentos**: Solicitar compra, consultar e cancelar pagamentos.
- **Trilha de Eventos**: Registro de eventos de domínio em cada operação (consulta, cancelamento, etc.).
- **Swagger**: Documentação e teste rápido dos endpoints.

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
A solução segue uma arquitetura em camadas, separando responsabilidades de API, Domínio, Infraestrutura e utilitários compartilhados.

Visão geral do fluxo:
- `Controller` recebe a requisição → valida/mapeia DTOs → chama o `Service` de domínio.
- `Service` aplica regras de negócio, registra eventos de domínio e orquestra a persistência.
- `Repositórios` acessam o banco via EF Core (`DbContext`) para leitura/escrita.
- `Middlewares` tratam correlação de logs, medem tempo e padronizam erros.

🗂️ Estrutura de pastas
```
fcg-pagamentos/
├── 📁 src/
│   ├── 📁 FCG.Pagamentos/                 # 🚀 API (ASP.NET Core)
│   │   ├── 📁 Controllers/                # 🌐 Endpoints REST
│   │   ├── 📁 MappingDtos/                # 🔁 Mapeamento DTO ↔ Domínio
│   │   ├── 📁 Middlewares/                # 🧰 Logging de requests e erros
│   │   ├── 📁 Models/                     # 🧩 DTOs de entrada/saída
│   │   └── Program.cs                     # ⚙️ Bootstrap/DI + Swagger
│   ├── 📁 FCG.Pagamentos.Business/        # 🧠 Camada de Domínio
│   │   ├── 📁 Interfaces/                 # 📝 Contratos (serviços/repositórios)
│   │   ├── 📁 Model/                      # 🏷️ Entidades (Payment, Event, Item)
│   │   └── 📁 Services/                   # 📦 Regras de negócio
│   ├── 📁 FCG.Pagamentos.Infra/           # 🗄️ Infraestrutura (EF Core)
│   │   ├── 📁 Data/                       # 💾 DbContext, Mappings e Repositórios
│   │   ├── 📁 Ioc/                        # 🧩 Registro de dependências (DI)
│   │   └── 📁 Migrations/                 # 🧬 Migrations (histórico do banco)
│   └── 📁 FCG.Pagamentos.Core/            # 🔧 Utilitários compartilhados
├── 📁 tests/
│   └── 📁 FCG.Pagamentos.Tests/           # 🧪 Testes unitários
├── Dockerfile                             # 🐳 Imagem da API
├── docker-compose.yml                     # 🛠️ Orquestra API + SQL Server
└── README.md
```

## 🔌 Endpoints e Estruturas

**Cabeçalhos comuns (opcional)**
- `X-Correlation-Id`: GUID/UUID para rastrear logs do request. Se não informado, a API gera um e o devolve no mesmo header.

**Tabela de rotas**

| Método | Rota                                                        | Body (request)             | 200 OK (response)             |
|--------|-------------------------------------------------------------|----------------------------|-------------------------------|
| POST   | `api/Payment/SolicitacaoCompra`                             | `PaymentDto`               | `PaymentResponse`             |
| POST   | `api/Payment/CancelarPagamento`                             | `PaymentRequestDto`        | `PaymentResponse`             |
| GET    | `api/Payment/ConsultarPagamento/{paymentId}/{userId}`       | —                          | `PaymentResponse`             |
| GET    | `api/Payment/ListarComprasUsuario/{paymentId}/{userId}`     | —                          | `Payment`                     |
| GET    | `api/PaymentEvents`                                         | —                          | `IEnumerable<PaymentEvent>`   |
| GET    | `api/PaymentEvents/type/{eventType}`                        | —                          | `IEnumerable<PaymentEvent>`   |

**Estruturas**

- `PaymentDto` (solicitação de compra)
  ```json
  {
    "orderId": "GUID",
    "userId": "GUID",
    "currency": "BRL",
    "items": [
      {
        "jogoId": "GUID",
        "description": "string",
        "unitPrice": number,
        "quantity": number
      }
    ]
  }
  ```

- `PaymentRequestDto` (cancelamento)
  ```json
  {
    "paymentId": "GUID",
    "userId": "GUID"
  }
  ```

- `PaymentResponse` (resposta padrão)
  ```json
  {
    "orderId": "GUID",
    "paymentId": "GUID",
    "statusPayment": "string",
    "success": true,
    "message": "string"
  }
  ```

- `Payment` (modelo de domínio completo)
  ```json
  {
    "orderId": "GUID",
    "paymentId": "GUID",
    "userId": "GUID",
    "currency": "string",
    "statusPayment": "string",
    "items": [
      {
        "paymentItemId": "GUID",
        "paymentId": "GUID",
        "jogoId": "GUID",
        "description": "string",
        "unitPrice": number,
        "quantity": number,
        "totalPrice": number
      }
    ],
    "totalAmount": number,
    "createdAt": "ISO-8601"
  }
  ```

## 🧩 Eventos e Versionamento
- Tipos de evento/status são mapeados no domínio (ex: `CONSULTA_SITUACAO`, `CANCELED`).
- O serviço de eventos controla a versão para evitar a duplicação consecutiva do mesmo tipo de evento, simulando a progressão do status do pagamento.

## 🛠️ Comandos úteis
- Rodar API: `dotnet run --project src/FCG.Pagamentos`
- Testes: `dotnet test`
- Restaurar: `dotnet restore`
- Build: `dotnet build`

## 🧱 Migrations (EF Core)

**Pré‑requisito (opcional):**
- `dotnet tool install --global dotnet-ef`

**Gerar uma nova migration:**
- Na raiz do repositório:
  ```bash
  dotnet ef migrations add <NomeDaMigration> -p src/FCG.Pagamentos.Infra -s src/FCG.Pagamentos -c ApplicationDbContext
  ```

**Atualizar o banco de dados:**
- ```bash
  dotnet ef database update -p src/FCG.Pagamentos.Infra -s src/FCG.Pagamentos -c ApplicationDbContext
  ```
- **Observações:**
  - `-p` aponta para o projeto da Infra (onde ficam as Migrations e o DbContext).
  - `-s` aponta para o projeto de inicialização (API), que contém a `connection string`.

## 👥 Idealizadores do Projeto (Discord)
- 👨‍💻 Clovis Alceu Cassaro (`cloves_93258`)
- 👨‍💻 Gabriel Santos Ramos (`_gsramos`)
- 👨‍💻 Júlio César de Carvalho (`cesarsoft`)
- 👨‍💻 Marco Antonio Araujo (`_marcoaz`)
- 👩‍💻 Yasmim Muniz Da Silva Caraça (`yasmimcaraca`)
