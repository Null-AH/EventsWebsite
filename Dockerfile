FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5206

ENV ASPNETCORE_URLS=http://+:5206

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["EventApi/EventApi.csproj", "EventApi/"]
RUN dotnet restore "EventApi/EventApi.csproj"
COPY . .
WORKDIR "/src/EventApi"
RUN dotnet build "EventApi.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "EventApi.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventApi.dll"]
