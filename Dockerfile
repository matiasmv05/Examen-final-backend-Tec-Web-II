FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore AMAZON.sln
RUN dotnet publish AMAZON.sln -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Amazon.api.dll"]