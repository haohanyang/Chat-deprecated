FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Chat.Server/Chat.Server.csproj", "Chat.Server/"]
RUN dotnet restore "Chat.Server/Chat.Server.csproj"
COPY . .
WORKDIR "/src/Chat.Server"
RUN dotnet build "Chat.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chat.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.Server.dll"]
