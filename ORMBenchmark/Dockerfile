FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /src
COPY . .
RUN dotnet restore ORMBenchmark.csproj

RUN dotnet publish  -c Release -o /src/bin/publish

WORKDIR /src/bin/publish
ENTRYPOINT ["dotnet", "ORMBenchmark.dll"]