FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*} && mv $file ${file%.*}; done
RUN dotnet restore Crnc.Oms.Notification.Push.Client/Crnc.Oms.Notification.Push.Client.csproj

# copy everything else and build app
COPY . ./dotnetapp
WORKDIR /app/dotnetapp
RUN dotnet publish Crnc.Oms.Notification.Push.Client/Crnc.Oms.Notification.Push.Client.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/dotnetapp/out ./
ENTRYPOINT ["dotnet", "Crnc.Oms.Notification.Push.Client.dll"]