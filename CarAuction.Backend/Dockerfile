# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["CarAuction.Presentation/CarAuction.Presentation.csproj", "CarAuction.Presentation/"]
COPY ["CarAuction.Application/CarAuction.Application.csproj", "CarAuction.Application/"]
COPY ["CarAuction.Domain/CarAuction.Domain.csproj", "CarAuction.Domain/"]
COPY ["CarAuction.Infrastructure/CarAuction.Infrastructure.csproj", "CarAuction.Infrastructure/"]

RUN dotnet restore "CarAuction.Presentation/CarAuction.Presentation.csproj"

# Copy remaining source code and publish
COPY . .
WORKDIR "/src/CarAuction.Presentation"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5000

ENTRYPOINT ["dotnet", "CarAuction.Presentation.dll"]
