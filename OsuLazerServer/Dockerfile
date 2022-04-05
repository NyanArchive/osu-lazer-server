FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY ./out .
COPY ./data ./data

ENTRYPOINT [ "dotnet", "OsuLazerServer.dll" ]