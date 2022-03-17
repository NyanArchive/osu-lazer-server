FROM mcr.microsoft.com/dotnet/sdk AS build-env
WORKDIR /app

COPY . ./
COPY ./data ./

RUN dotnet restore
RUN dotnet publish OsuLazerServer/OsuLazerServer.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "OsuLazerServer.dll" ]