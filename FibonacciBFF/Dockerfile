# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY FibonacciBFF/FibonacciBFF.csproj FibonacciBFF/
COPY Fibonacci.Calculator/Fibonacci.Calculator.csproj Fibonacci.Calculator/

# Copy proto files
COPY Fibonacci.Calculator/Protos Fibonacci.Calculator/Protos/

# Restore dependencies
RUN dotnet restore FibonacciBFF/FibonacciBFF.csproj

# Copy everything else (source code, etc.)
COPY . .

# Build the project
WORKDIR /src/FibonacciBFF
RUN dotnet build FibonacciBFF.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish FibonacciBFF.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FibonacciBFF.dll"]