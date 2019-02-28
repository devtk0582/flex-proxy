FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["FlexProxy/FlexProxy.csproj", "FlexProxy/"]
RUN dotnet restore "FlexProxy/FlexProxy.csproj"
COPY . .
WORKDIR "/src/FlexProxy"
RUN dotnet build "FlexProxy.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "FlexProxy.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FlexProxy.dll"]