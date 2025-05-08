# Base stage (for running the app in production)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Fibonacci.Calculator.csproj", "."]
RUN dotnet restore "./Fibonacci.Calculator.csproj"

# Copy the rest of the project files (including Protos folder)
COPY . .

# Change to the project directory and build the project
RUN dotnet build "./Fibonacci.Calculator.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fibonacci.Calculator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage (for production use)
FROM base AS final
WORKDIR /app

# Copy the published output from the publish stage
COPY --from=publish /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "Fibonacci.Calculator.dll"]