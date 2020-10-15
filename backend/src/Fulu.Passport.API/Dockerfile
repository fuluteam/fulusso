FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM   mcr.microsoft.com/dotnet/core/sdk:3.1-buster  AS build
VOLUME [ "/root/.nuget/packages" ]
VOLUME [ "/root/.local/share/NuGet/v3-cache" ]
WORKDIR /src
COPY . .
WORKDIR /src/src/Fulu.Passport.API
RUN dotnet publish Fulu.Passport.API.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
RUN rm -rf appsettings.Development.json
RUN cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime \
&& echo 'Asia/Shanghai' >/etc/timezone
ENTRYPOINT ["dotnet", "Fulu.Passport.API.dll"]
