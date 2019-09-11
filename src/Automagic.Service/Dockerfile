FROM microsoft/aspnetcore:2.0-stretch AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0-stretch AS build
WORKDIR /src
COPY ["Automagic.Service/Automagic.Service.csproj", "Automagic.Service/"]
COPY ["Automagic.Dialog/Automagic.Dialog.csproj", "Automagic.Dialog/"]
COPY ["Automagic.Core/Automagic.Core.csproj", "Automagic.Core/"]
RUN dotnet restore "Automagic.Service/Automagic.Service.csproj"
COPY . .
WORKDIR "/src/Automagic.Service"
RUN dotnet build "Automagic.Service.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Automagic.Service.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Automagic.Service.dll"]