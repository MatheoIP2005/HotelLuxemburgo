FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HotelLux.Gateway/HotelLux.Gateway.csproj", "HotelLux.Gateway/"]
COPY ["HotelLux.Shared/HotelLux.Shared.csproj", "HotelLux.Shared/"]
RUN dotnet restore "HotelLux.Gateway/HotelLux.Gateway.csproj"

COPY HotelLux.Gateway/ HotelLux.Gateway/
COPY HotelLux.Shared/ HotelLux.Shared/
COPY docs/ docs/

WORKDIR /src/HotelLux.Gateway
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/docs/ docs/

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelLux.Gateway.dll"]
