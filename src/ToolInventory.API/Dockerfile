FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/ToolInventory.API/ToolInventory.API.csproj", "src/ToolInventory.API/"]
COPY ["src/ToolInventory.Core/ToolInventory.Core.csproj", "src/ToolInventory.Core/"]
COPY ["src/ToolInventory.Infrastructure/ToolInventory.Infrastructure.csproj", "src/ToolInventory.Infrastructure/"]
COPY ["src/ToolInventory.Shared/ToolInventory.Shared.csproj", "src/ToolInventory.Shared/"]
RUN dotnet restore "src/ToolInventory.API/ToolInventory.API.csproj"
COPY . .
RUN dotnet build "src/ToolInventory.API/ToolInventory.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/ToolInventory.API/ToolInventory.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToolInventory.API.dll"]
