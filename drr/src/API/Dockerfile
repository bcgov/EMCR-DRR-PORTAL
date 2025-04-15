FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
ARG VERSION
ENV VERSION=$VERSION

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["EMCR.DRR/EMCR.DRR.API.csproj", "EMCR.DRR/"]
COPY ["EMCR.DRR.Dynamics/EMCR.DRR.Dynamics.csproj", "EMCR.DRR.Dynamics/"]
COPY ["EMCR.Utilities/EMCR.Utilities.csproj", "EMCR.Utilities/"]
COPY ["EMCR.Test.Integration.DRR.Api/EMCR.Tests.Integration.DRR.Api.csproj", "EMCR.Test.Integration.DRR.Api/"]
COPY ["EMCR.DRR.sln", ".editorconfig", "./"]
RUN dotnet restore .
COPY . .

FROM build AS publish
RUN dotnet publish "EMCR.DRR/EMCR.DRR.API.csproj" -c Release -o /app/publish --no-restore 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "EMCR.DRR.API.dll"]
