# ==========================================
# 1. Estágio de Base (Runtime)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

# Instala pacotes para globalização e fuso horário
RUN apk add --no-cache \
    icu-data-full \
    icu-libs \
    tzdata

# Configurações regionais e Timezone de Brasília
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=America/Sao_Paulo \
    LC_ALL=pt_BR.UTF-8 \
    LANG=pt_BR.UTF-8

EXPOSE 8080

# ==========================================
# 2. Estágio de Build (SDK)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copia arquivos de projeto primeiro para cache de pacotes NuGet
COPY ["FCG.Pagamentos.sln", "./"]
COPY ["src/FCG.Pagamentos/FCG.Pagamentos.API.csproj", "src/FCG.Pagamentos/"]
COPY ["src/FCG.Pagamentos.Business/FCG.Pagamentos.Business.csproj", "src/FCG.Pagamentos.Business/"]
COPY ["src/FCG.Pagamentos.Core/FCG.Pagamentos.Core.csproj", "src/FCG.Pagamentos.Core/"]
COPY ["src/FCG.Pagamentos.Infra/FCG.Pagamentos.Infra.csproj", "src/FCG.Pagamentos.Infra/"]
COPY ["tests/FCG.Pagamentos.Tests/FCG.Pagamentos.Tests.csproj", "tests/FCG.Pagamentos.Tests/"]

RUN dotnet restore "FCG.Pagamentos.sln"

# Copia o código fonte e publica
COPY . .
RUN dotnet publish "src/FCG.Pagamentos/FCG.Pagamentos.API.csproj" \
    -c Release \
    -o /app/out \
    --no-restore \
    /p:UseAppHost=false

# ==========================================
# 3. Estágio Final (Produção)
# ==========================================
FROM base AS final
WORKDIR /app

# Copia os binários compilados
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "FCG.Pagamentos.API.dll"]