# Imagem da API. Multiestágio: o SDK compila, a imagem final só executa.

# ---------- compilação ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS compilacao
WORKDIR /origem

# A árvore inteira entra de uma vez e a restauração acontece dentro do publish.
#
# A versão anterior copiava cada .csproj antes, para reaproveitar a camada de
# restauração. Economizava alguns segundos e criou uma armadilha: acrescentar um
# projeto à solução quebrava a imagem com "assets file not found", erro que não
# aponta para a causa e que só aparece na hora de subir o ambiente. Lista que
# precisa ser lembrada é lista que será esquecida.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/ src/

RUN dotnet publish src/CausaClara.Api/CausaClara.Api.csproj \
      --configuration Release \
      --output /publicacao

# ---------- execução ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS execucao

# O Npgsql tenta carregar GSSAPI ao negociar autenticação e, sem esta biblioteca,
# registra "Cannot load library libgssapi_krb5.so.2" a cada inicialização. A
# conexão funciona assim mesmo, mas um erro recorrente e inofensivo no log ensina
# a ignorar o log — que é justamente o que o RNF29 não quer. Um megabyte resolve.
USER root
RUN apt-get update \
    && apt-get install --yes --no-install-recommends libgssapi-krb5-2 \
    && rm --recursive --force /var/lib/apt/lists/*

WORKDIR /aplicacao

COPY --from=compilacao /publicacao .

# Usuário sem privilégio, definido pela imagem base. Processo de API não roda
# como root. Declarado depois do COPY para que a cópia não dependa das
# permissões do usuário final.
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "CausaClara.Api.dll"]
