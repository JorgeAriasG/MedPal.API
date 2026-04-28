# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5126
ENV ASPNETCORE_URLS=http+:5126

# Use the SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy csproj and restore as distinct layers
COPY ["MedPal.API.csproj", "./"]
RUN dotnet restore "MedPal.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/"
RUN dotnet build "MedPal.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MedPal.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MedPal.API.dll"]
