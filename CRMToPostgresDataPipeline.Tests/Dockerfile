FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./CRMToPostgresDataPipeline.sln ./
COPY ./CRMToPostgresDataPipeline/CRMToPostgresDataPipeline.csproj ./CRMToPostgresDataPipeline/
COPY ./CRMToPostgresDataPipeline.Tests/CRMToPostgresDataPipeline.Tests.csproj ./CRMToPostgresDataPipeline.Tests/

RUN dotnet restore ./CRMToPostgresDataPipeline/CRMToPostgresDataPipeline.csproj
RUN dotnet restore ./CRMToPostgresDataPipeline.Tests/CRMToPostgresDataPipeline.Tests.csproj

# Copy everything else and build
COPY . ./

RUN dotnet build -c debug -o out CRMToPostgresDataPipeline.Tests/CRMToPostgresDataPipeline.Tests.csproj

CMD dotnet test