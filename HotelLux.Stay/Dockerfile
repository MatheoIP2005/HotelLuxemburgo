FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HotelLux.Stay/HotelLux.Stay.API/HotelLux.Stay.API.csproj", "HotelLux.Stay/HotelLux.Stay.API/"]
COPY ["HotelLux.Stay/HotelLux.Stay.Business/HotelLux.Stay.Business.csproj", "HotelLux.Stay/HotelLux.Stay.Business/"]
COPY ["HotelLux.Stay/HotelLux.Stay.DataManagement/HotelLux.Stay.DataManagement.csproj", "HotelLux.Stay/HotelLux.Stay.DataManagement/"]
COPY ["HotelLux.Stay/HotelLux.Stay.DataAccess/HotelLux.Stay.DataAccess.csproj", "HotelLux.Stay/HotelLux.Stay.DataAccess/"]

RUN dotnet restore "HotelLux.Stay/HotelLux.Stay.API/HotelLux.Stay.API.csproj"

COPY HotelLux.Stay/ HotelLux.Stay/
COPY protos/ protos/

WORKDIR /src/HotelLux.Stay/HotelLux.Stay.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelLux.Stay.API.dll"]
