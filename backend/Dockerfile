FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ManConstruc.Api/ManConstruc.Api.csproj", "ManConstruc.Api/"]
RUN dotnet restore "ManConstruc.Api/ManConstruc.Api.csproj"
COPY . .
WORKDIR "/src/ManConstruc.Api"
RUN dotnet build "ManConstruc.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ManConstruc.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ManConstruc.Api.dll"]