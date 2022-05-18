# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY marketplace/*.csproj ./marketplace/
RUN dotnet restore

# copy everything else and build app
COPY marketplace/. ./marketplace/
WORKDIR /source/marketplace
RUN dotnet publish -c release -o /app --no-restore

COPY marketplace/wwwroot. ./app/wwwroot/

# final stage/image
FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /app
COPY --from=build /app ./
RUN mkdir -p /app/Data
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Marketplace.dll"]
