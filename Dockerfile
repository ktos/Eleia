FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine AS base
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "Eleia.dll"]