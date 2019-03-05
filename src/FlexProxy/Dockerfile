FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["FlexProxy/FlexProxy.csproj", "FlexProxy/"]
COPY ["FlexProxy.RequestTracerMiddleware/FlexProxy.RequestTracerMiddleware.csproj", "FlexProxy.RequestTracerMiddleware/"]
COPY ["FlexProxy.Core/FlexProxy.Core.csproj", "FlexProxy.Core/"]
COPY ["FlexProxy.HealthCheckMiddleware/FlexProxy.HealthCheckMiddleware.csproj", "FlexProxy.HealthCheckMiddleware/"]
COPY ["FlexProxy.ExceptionHandlerMiddleware/FlexProxy.ExceptionHandlerMiddleware.csproj", "FlexProxy.ExceptionHandlerMiddleware/"]
COPY ["FlexProxy.RobotsMiddleware/FlexProxy.RobotsMiddleware.csproj", "FlexProxy.RobotsMiddleware/"]
COPY ["FlexProxy.WebProxyMiddleware/FlexProxy.WebProxyMiddleware.csproj", "FlexProxy.WebProxyMiddleware/"]
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