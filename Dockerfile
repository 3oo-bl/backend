from mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY ProfitableViewAPI/*.csproj ./ProfitableViewAPI/
COPY ProfitableViewApp/*.csproj ./ProfitableViewApp/
COPY ProfitableViewCore/*.csproj ./ProfitableViewCore/
COPY ProfitableViewData/*.csproj ./ProfitableViewData/

RUN dotnet restore

COPY ProfitableViewAPI/. ./ProfitableViewAPI/
COPY ProfitableViewApp/. ./ProfitableViewApp/
COPY ProfitableViewCore/. ./ProfitableViewCore/
COPY ProfitableViewData/. ./ProfitableViewData/
WORKDIR /src/ProfitableViewAPI
RUN dotnet publish -c Debug -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "ProfitableViewAPI.dll"]