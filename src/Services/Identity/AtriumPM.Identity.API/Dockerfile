# ── Build Stage ────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY AtriumPM.slnx .
COPY src/Shared/AtriumPM.Shared/AtriumPM.Shared.csproj src/Shared/AtriumPM.Shared/
COPY src/Services/Identity/AtriumPM.Identity.API/AtriumPM.Identity.API.csproj src/Services/Identity/AtriumPM.Identity.API/

RUN dotnet restore src/Services/Identity/AtriumPM.Identity.API/AtriumPM.Identity.API.csproj

# Copy everything and build
COPY src/Shared/AtriumPM.Shared/ src/Shared/AtriumPM.Shared/
COPY src/Services/Identity/AtriumPM.Identity.API/ src/Services/Identity/AtriumPM.Identity.API/
RUN dotnet publish src/Services/Identity/AtriumPM.Identity.API/AtriumPM.Identity.API.csproj \
    -c Release -o /app/publish

# ── Runtime Stage ─────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AtriumPM.Identity.API.dll"]
