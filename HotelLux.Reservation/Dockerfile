FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HotelLux.Reservation/HotelLux.Reservation.API/HotelLux.Reservation.API.csproj", "HotelLux.Reservation/HotelLux.Reservation.API/"]
COPY ["HotelLux.Reservation/HotelLux.Reservation.Business/HotelLux.Reservation.Business.csproj", "HotelLux.Reservation/HotelLux.Reservation.Business/"]
COPY ["HotelLux.Reservation/HotelLux.Reservation.DataManagement/HotelLux.Reservation.DataManagement.csproj", "HotelLux.Reservation/HotelLux.Reservation.DataManagement/"]
COPY ["HotelLux.Reservation/HotelLux.Reservation.DataAccess/HotelLux.Reservation.DataAccess.csproj", "HotelLux.Reservation/HotelLux.Reservation.DataAccess/"]

RUN dotnet restore "HotelLux.Reservation/HotelLux.Reservation.API/HotelLux.Reservation.API.csproj"

COPY HotelLux.Reservation/ HotelLux.Reservation/
COPY protos/ protos/

WORKDIR /src/HotelLux.Reservation/HotelLux.Reservation.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelLux.Reservation.API.dll"]
