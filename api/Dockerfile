FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV DOTNET_gcServer=1
ARG VERSION
ENV VERSION=$VERSION

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /src

COPY ["api/api.csproj", "api/"]
COPY ["db/db.csproj", "db/"]
RUN dotnet restore api
RUN dotnet restore db
COPY . .
RUN dotnet build "api/api.csproj" -c Release
# build
FROM build AS publish
RUN dotnet publish "api/api.csproj" -c Release -o /app/publish --runtime linux-musl-x64 --no-self-contained

FROM base AS final

# copy app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api.dll"]
