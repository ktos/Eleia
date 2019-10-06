FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS base
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "Eleia.dll"]