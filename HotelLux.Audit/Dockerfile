FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HotelLux.Audit/HotelLux.Audit.API/HotelLux.Audit.API.csproj", "HotelLux.Audit/HotelLux.Audit.API/"]
COPY ["HotelLux.Audit/HotelLux.Audit.DataAccess/HotelLux.Audit.DataAccess.csproj", "HotelLux.Audit/HotelLux.Audit.DataAccess/"]
COPY ["HotelLux.Shared/HotelLux.Shared.csproj", "HotelLux.Shared/"]

RUN dotnet restore "HotelLux.Audit/HotelLux.Audit.API/HotelLux.Audit.API.csproj"

COPY HotelLux.Audit/ HotelLux.Audit/
COPY HotelLux.Shared/ HotelLux.Shared/
COPY protos/ protos/

WORKDIR /src/HotelLux.Audit/HotelLux.Audit.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelLux.Audit.API.dll"]
