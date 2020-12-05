FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001
ENV ASPNETCORE_URLS=http://*:5000

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["TestAppMetrics.csproj", "./"]
RUN dotnet restore "TestAppMetrics.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TestAppMetrics.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestAppMetrics.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestAppMetrics.dll"]
