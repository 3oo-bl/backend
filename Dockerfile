from mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY ProfitableViewAPI/*.csproj ./ProfitableViewAPI/
COPY ProfitableViewApp/*.csproj ./ProfitableViewApp/
COPY ProfitableViewData/*.csproj ./ProfitableViewData/
COPY ProfitableViewInfra/*.csproj ./ProfitableViewInfra/
COPY proto/*.proto ./proto/

RUN dotnet restore ProfitableViewAPI/ProfitableViewAPI.csproj

COPY ProfitableViewAPI/. ./ProfitableViewAPI/
COPY ProfitableViewApp/. ./ProfitableViewApp/
COPY ProfitableViewData/. ./ProfitableViewData/
COPY ProfitableViewInfra/. ./ProfitableViewInfra/
COPY proto/. ./proto/
WORKDIR /src/ProfitableViewAPI
RUN dotnet publish -c Debug -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "ProfitableViewAPI.dll"]