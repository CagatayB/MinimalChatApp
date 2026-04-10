FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Create a directory for the SQLite file to live in
RUN mkdir -p /app/data
EXPOSE 80
EXPOSE 443

# We use the full SDK to compile the code
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files individually to leverage Docker cache
# This way, if you change code but not packages, 'dotnet restore' is skipped
COPY ["ChatApp.API/ChatApp.API.csproj", "ChatApp.API/"]
COPY ["ChatApp.Domain/ChatApp.Domain.csproj", "ChatApp.Domain/"]
COPY ["ChatApp.Application/ChatApp.Application.csproj", "ChatApp.Application/"]
COPY ["ChatApp.Infrastructure/ChatApp.Infrastructure.csproj", "ChatApp.Infrastructure/"]

RUN dotnet restore "ChatApp.API/ChatApp.API.csproj"

COPY . .
WORKDIR "/src/ChatApp.API"
RUN dotnet build "ChatApp.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/chat.db"

ENTRYPOINT ["dotnet", "ChatApp.API.dll"]