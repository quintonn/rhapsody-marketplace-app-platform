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

# final stage/image
FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /app
COPY --from=build /app ./
RUN mkdir -p /app/Data
ENV ASPNETCORE_URLS=http://+:5000;https://+:5001
EXPOSE 5000
EXPOSE 5001
ENTRYPOINT ["dotnet", "Marketplace.dll"]
