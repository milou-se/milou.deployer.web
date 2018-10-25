FROM microsoft/dotnet:2.1-sdk AS build

WORKDIR /app

COPY src/*.sln mytemp/
COPY src/Milou.Deployer.Web.Core/*.csproj mytemp/Milou.Deployer.Web.Core/
COPY src/Milou.Deployer.Web.IisHost/*.csproj mytemp/Milou.Deployer.Web.IisHost/
COPY src/Milou.Deployer.Web.Marten/*.csproj mytemp/Milou.Deployer.Web.Marten/
COPY src/Milou.Deployer.Web.Tests.Integration/*.csproj mytemp/Milou.Deployer.Web.Tests.Integration/
COPY src/Milou.Deployer.Web.Tests.Unit/*.csproj mytemp/Milou.Deployer.Web.Tests.Unit/
COPY NuGet.config mytemp/

WORKDIR mytemp

RUN dotnet restore

COPY src/. ./

RUN pwd

WORKDIR /app/mytemp

RUN ls

RUN dotnet publish -c release -o out

FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=build /app/mytemp/out ./
ENTRYPOINT ["dotnet", "Milou.Deployer.Web.Iishost.dll"]
