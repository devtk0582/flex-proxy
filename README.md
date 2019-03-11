# Flex Proxy

Flex Proxy is built as a flexible proxy to serve contents seamlessly from any other website. Embeded Javascript engine allows you to write custom scripts to manipulate the contents returned from remote endpoint such as changing images or updating HTML elements etc. It can be easily expanded to integrate more components like database or file system to get more control over the pipeline.

## Getting Started

You would need to download the source code, compile it using dotnet CLI or Visual Studio and configure the settings in appsettings.json file before you use it

### Prerequisites

You would need 
* A computer
* Internet
* .net core runtime

### Running the application

Get started by running the following command at root folder of the project.

```
dotnet restore
dotnet build
dotnet run
```

To deploy the project, use following command

```
dotnet publish -o "your-destination-path"
```

And in the root folder of the artifacts

```
dotnet FlexProxy.dll
```

## Features

* Kestrel web server as web host with configurable urls and ports
* Html DOM tree parsing and manipulation using HtmlAgilityPack and selective wrappers
* Use ChakraCore as Javascript engine to dynamically execute C# code on the fly
* Extensible logging endpoints to write custom logs with external api or to files
* Request trace logs to keep track of traffic
* Disable robot scanning
* Allow to add custom session cookies in the response
* Javascript engine pool to allow scalable structure to support high-performance Javascript execution
* Multiple built-in apis (such as Request Api, Response Api, Html Document Api, Form Api, Log Api, Javascript Api, Json Api etc. )to interact with Javascript engine to manipulate contents with different content types 
* Intelligently build request and response based on comprehensive and expandible builders

