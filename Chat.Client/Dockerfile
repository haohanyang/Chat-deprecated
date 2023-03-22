FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Chat.Client/Chat.Client.csproj", "Chat.Client/"]
RUN dotnet restore "Chat.Client/Chat.Client.csproj"
COPY . .
WORKDIR "/src/Chat.Client"
RUN dotnet build "Chat.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chat.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.Client.dll"]
