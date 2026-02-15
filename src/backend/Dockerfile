FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /app

COPY ClinicPOS.Domain/ClinicPOS.Domain.csproj ClinicPOS.Domain/
COPY ClinicPOS.Infrastructure/ClinicPOS.Infrastructure.csproj ClinicPOS.Infrastructure/
COPY ClinicPOS.Application/ClinicPOS.Application.csproj ClinicPOS.Application/
COPY ClinicPOS.API/ClinicPOS.API.csproj ClinicPOS.API/

RUN dotnet restore ClinicPOS.API/ClinicPOS.API.csproj

COPY . .
RUN dotnet publish ClinicPOS.API/ClinicPOS.API.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /publish .

# Install EF migrations tool
RUN apt-get update && apt-get install -y curl

EXPOSE 8080
ENTRYPOINT ["dotnet", "ClinicPOS.API.dll"]
