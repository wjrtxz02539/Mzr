#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Crawler/Crawler.csproj", "Crawler/"]
RUN dotnet restore "Crawler/Crawler.csproj"
COPY . .
WORKDIR "/src/Crawler"
RUN dotnet build "Crawler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crawler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mzr.Service.Crawler.dll"]