FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HotelLux.Auth/HotelLux.Auth.API/HotelLux.Auth.API.csproj", "HotelLux.Auth/HotelLux.Auth.API/"]
COPY ["HotelLux.Auth/HotelLux.Auth.Business/HotelLux.Auth.Business.csproj", "HotelLux.Auth/HotelLux.Auth.Business/"]
COPY ["HotelLux.Auth/HotelLux.Auth.DataManagement/HotelLux.Auth.DataManagement.csproj", "HotelLux.Auth/HotelLux.Auth.DataManagement/"]
COPY ["HotelLux.Auth/HotelLux.Auth.DataAccess/HotelLux.Auth.DataAccess.csproj", "HotelLux.Auth/HotelLux.Auth.DataAccess/"]

RUN dotnet restore "HotelLux.Auth/HotelLux.Auth.API/HotelLux.Auth.API.csproj"

COPY HotelLux.Auth/ HotelLux.Auth/
COPY protos/ protos/

WORKDIR /src/HotelLux.Auth/HotelLux.Auth.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelLux.Auth.API.dll"]
