FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HotelLux.Finance/HotelLux.Finance.API/HotelLux.Finance.API.csproj", "HotelLux.Finance/HotelLux.Finance.API/"]
COPY ["HotelLux.Finance/HotelLux.Finance.Business/HotelLux.Finance.Business.csproj", "HotelLux.Finance/HotelLux.Finance.Business/"]
COPY ["HotelLux.Finance/HotelLux.Finance.DataManagement/HotelLux.Finance.DataManagement.csproj", "HotelLux.Finance/HotelLux.Finance.DataManagement/"]
COPY ["HotelLux.Finance/HotelLux.Finance.DataAccess/HotelLux.Finance.DataAccess.csproj", "HotelLux.Finance/HotelLux.Finance.DataAccess/"]

RUN dotnet restore "HotelLux.Finance/HotelLux.Finance.API/HotelLux.Finance.API.csproj"

COPY HotelLux.Finance/ HotelLux.Finance/
COPY protos/ protos/

WORKDIR /src/HotelLux.Finance/HotelLux.Finance.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelLux.Finance.API.dll"]
