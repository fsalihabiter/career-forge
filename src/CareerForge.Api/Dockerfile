FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/CareerForge.Api/CareerForge.Api.csproj", "src/CareerForge.Api/"]
COPY ["NuGet.Config", "."]
RUN dotnet restore "src/CareerForge.Api/CareerForge.Api.csproj" --configfile NuGet.Config
COPY . .
RUN dotnet publish "src/CareerForge.Api/CareerForge.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CareerForge.Api.dll"]
