FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HotelLux.Accommodation/HotelLux.Accommodation.API/HotelLux.Accommodation.API.csproj", "HotelLux.Accommodation/HotelLux.Accommodation.API/"]
COPY ["HotelLux.Accommodation/HotelLux.Accommodation.Business/HotelLux.Accommodation.Business.csproj", "HotelLux.Accommodation/HotelLux.Accommodation.Business/"]
COPY ["HotelLux.Accommodation/HotelLux.Accommodation.DataManagement/HotelLux.Accommodation.DataManagement.csproj", "HotelLux.Accommodation/HotelLux.Accommodation.DataManagement/"]
COPY ["HotelLux.Accommodation/HotelLux.Accommodation.DataAccess/HotelLux.Accommodation.DataAccess.csproj", "HotelLux.Accommodation/HotelLux.Accommodation.DataAccess/"]

RUN dotnet restore "HotelLux.Accommodation/HotelLux.Accommodation.API/HotelLux.Accommodation.API.csproj"

COPY HotelLux.Accommodation/ HotelLux.Accommodation/
COPY protos/ protos/

WORKDIR /src/HotelLux.Accommodation/HotelLux.Accommodation.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelLux.Accommodation.API.dll"]
