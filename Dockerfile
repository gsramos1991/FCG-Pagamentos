# Multi-stage build para FCG.Pagamentos.API (.NET 8)

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copia solution e csproj para melhor cache do restore
COPY FCG.Pagamentos.sln ./
COPY src/FCG.Pagamentos/FCG.Pagamentos.API.csproj src/FCG.Pagamentos/
COPY src/FCG.Pagamentos.Business/FCG.Pagamentos.Business.csproj src/FCG.Pagamentos.Business/
COPY src/FCG.Pagamentos.Core/FCG.Pagamentos.Core.csproj src/FCG.Pagamentos.Core/
COPY src/FCG.Pagamentos.Infra/FCG.Pagamentos.Infra.csproj src/FCG.Pagamentos.Infra/

# copiar o csproj dos testes (correção importante)
COPY tests/FCG.Pagamentos.Tests/FCG.Pagamentos.Tests.csproj tests/FCG.Pagamentos.Tests/

# restore da solução (agora encontra todos os projetos)
RUN dotnet restore FCG.Pagamentos.sln

# copia o restante do código
COPY . .

# publica o projeto API
RUN dotnet publish src/FCG.Pagamentos/FCG.Pagamentos.API.csproj -c Release -o /app/out /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "FCG.Pagamentos.API.dll"]