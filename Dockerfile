FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "Eleia.dll"]