# Use the official .NET ASP.NET 8.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Use the .NET SDK 8.0 for build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy your .csproj file and restore dependencies
COPY ["LoginRegistrationApp/LoginRegistrationApp.csproj", "LoginRegistrationApp/"]
RUN dotnet restore "LoginRegistrationApp/LoginRegistrationApp.csproj"

# Copy the rest of the source code
COPY . .

# Set working directory to your project folder
WORKDIR "/src/LoginRegistrationApp"

# Build the project
RUN dotnet build "LoginRegistrationApp.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "LoginRegistrationApp.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app

# Copy published output
COPY --from=publish /app/publish .

# Start the app
ENTRYPOINT ["dotnet", "LoginRegistrationApp.dll"]

