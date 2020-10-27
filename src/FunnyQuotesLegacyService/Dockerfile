FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
#COPY *.sln .
#COPY src/FunnyQuotesLegacyService/*.csproj ./FunnyQuotesLegacyService/
#COPY src/FunnyQuotesLegacyService/*.config ./FunnyQuotesLegacyService/
COPY . .
WORKDIR /app/src
RUN nuget restore

# copy everything else and build app
#COPY aspnetmvcapp/. ./aspnetmvcapp/
WORKDIR /app/src/FunnyQuotesLegacyService
RUN dir
RUN msbuild /p:Configuration=Release


FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime
WORKDIR /inetpub/wwwroot
COPY --from=build /app/src/FunnyQuotesLegacyService/. ./