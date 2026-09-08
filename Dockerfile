# ─────────────────────────────────────────────────────────────
# Estágio 1: build
# ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copiar apenas o .csproj primeiro para aproveitar cache de camadas
COPY BibliotecaOnline/BibliotecaOnline.csproj BibliotecaOnline/
RUN dotnet restore BibliotecaOnline/BibliotecaOnline.csproj

# Copiar o restante do código-fonte
COPY BibliotecaOnline/ BibliotecaOnline/

# Publicar em modo Release, sem self-contained (runtime já estará na imagem base)
RUN dotnet publish BibliotecaOnline/BibliotecaOnline.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─────────────────────────────────────────────────────────────
# Estágio 2: runtime (imagem mínima)
# ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Criar pasta para o banco de dados persistido em volume
RUN mkdir -p /data && chown -R app:app /data 2>/dev/null || mkdir -p /data

# Copiar os artefatos publicados
COPY --from=build /app/publish .

# Variáveis de ambiente padrão
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV BIBLIOTECA_DB_PATH=/data/BibliotecaOnline.db

# Porta exposta (Cloudflare Tunnel, Railway e Render usam 8080 por padrão)
EXPOSE 8080

# Entrada da aplicação
ENTRYPOINT ["dotnet", "BibliotecaOnline.dll"]
