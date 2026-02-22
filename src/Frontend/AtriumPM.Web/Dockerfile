FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY AtriumPM.slnx .
COPY src/Frontend/AtriumPM.Web/AtriumPM.Web.csproj src/Frontend/AtriumPM.Web/

RUN dotnet restore src/Frontend/AtriumPM.Web/AtriumPM.Web.csproj

COPY src/Frontend/AtriumPM.Web/ src/Frontend/AtriumPM.Web/

RUN dotnet publish src/Frontend/AtriumPM.Web/AtriumPM.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AtriumPM.Web.dll"]
