# Base image with ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Build image with SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ✅ Copy csproj file correctly from root
COPY ["LoginRegistrationApp.csproj", "./"]
RUN dotnet restore "LoginRegistrationApp.csproj"

# ✅ Copy rest of the source code
COPY . .

# ✅ Build project
RUN dotnet build "LoginRegistrationApp.csproj" -c Release -o /app/build

# ✅ Publish project
FROM build AS publish
RUN dotnet publish "LoginRegistrationApp.csproj" -c Release -o /app/publish

# Final stage: runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "LoginRegistrationApp.dll"]

