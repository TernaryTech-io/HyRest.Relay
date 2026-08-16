# HyRest.Relay

A simple but robust example of an API server demonstrating the **HyRest Library** and its core capabilities. This sample project serves as a reference implementation for:

- **Dependency Injection Integration**: Shows how to use the `HyRest.DependencyInjection` library to configure and manage the HylandApp services
- **OpenID Connect Authentication**: Demonstrates secure authentication using OpenID Connect with Duende Identity Server, with token management and claim handling

## Overview

HyRest.Relay is an example ASP.NET Core Web API that acts as a proxy/relay layer to Hyland's OnBase platform. It uses the HyRest Dependancy Injection library to abstract the complexity of authentication & session managment, and the HyRest library to simplify interactions with Hyland Rest API.

## What It Demonstrates:

- **OpenID Connect Support**: Built-in OpenID Connect authentication using Duende libraries for secure token management with the HyRest Library.
- **Dependency Injection**: Leverages HyRest's DI extensions to simplify OnBase app configuration
- **Swagger/OpenAPI**: Integrated Swagger documentation for API exploration (in development mode)
- **Session Lifecycle Management**: Automatic handling of OnBase session connect/disconnect
- **Hyland Cookie & Licensing Header Handling**: Properly handles Set-Cookie session ids, and Hyland-License-Type headers. 

## Configuration

### appsettings.json

Configure the following section with your Hyland environment details:

```json
{
  "HylandApp": {
    "IdSUri": "https://onbase.ternarytech.io/auth",
    "ApiUri": "https://onbase.ternarytech.io/api",
    "DefaultLanguage": "en-US",
    "UseQueryMetering": true,
    "RequestTimeOut" :  180 
  }
}
```

### Environment Variables

The following environment variables must be set for OpenID Connect authentication with the Hyland Identity Server:

- `HYREST_CLIENTID`: Your Hyland Identity Server client ID
- `HYREST_CLIENTSECRET`: Your Hyland Identity Server client secret
> [See the sample client config for more details](./Sample-IdS-Client.md)

You can also use the provided `example.env` file as a template:

```bash
cp example.env .env
# Edit .env with your credentials
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK or higher
- A running Hyland OnBase environment with IDS (Identity Server)
- Valid Identity Server API client credentials (Client ID and Secret)

### Running the Application

1. **Configure your environment**:
   ```bash
   cp example.env .env
   # Edit .env with your sensitive variables and the appsettings.json for the others.
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

   The API will be available at `https://localhost:7258` by default. Trigger the authentication flow by navigating to `https://localhost:7258/account/login`

3. **Access Swagger UI** (Development only):
   Navigate to `https://localhost:7258/swagger` to explore the API endpoints and test them.

## Docker Support

This project includes Docker configuration for containerized deployment:

```bash
docker build -t hyrest-relay .
docker run -p 8080:80 hyrest-relay
```

Ensure all required environment variables are passed to the container:

```bash
docker run -p 8080:80 \
  -e HYREST_CLIENTID=your-client-id \
  -e HYREST_CLIENTSECRET=your-client-secret \
  hyrest-relay
```

## Architecture

The application follows a clean architecture pattern:

- **Program.cs**: Entry point that builds and starts the application
- **Configuration/AppConfiguration.cs**: Central configuration logic for dependency injection, authentication, and middleware setup
- **EndpointConfiguration/**: Defines API endpoints that demonstrate HyRest functionality
- **Dtos/**: Data transfer objects for request/response serialization
- **ServiceWorkers/**: Background services leveraging the HyRest library

## Authentication Flow

1. **User requests** an authenticated endpoint
2. **OpenID Connect middleware** validates the request and enforces authentication
3. **Token management** (via Duende) automatically handles token refresh
4. **HylandApp DI** injects authenticated OnBase session into endpoints
5. **Endpoints** interact with OnBase using the injected `OnBaseApp` service

## Common Tasks

### Adding a New Endpoint

1. Create a new endpoint in the `EndpointConfiguration/` folder
2. Use dependency injection to get the `OnBaseApp` service
3. Interact with OnBase APIs through the HyRest library

### Customizing the OnBase Configuration

Modify the `AddOpenIdHylandApp()` call in `AppConfiguration.cs` to adjust:
- API base URL
- IDS (Identity Server) URL
- Default language
- Query metering settings

## Resources

- [HyRest Documentation](../../docs/)
- [Duende Identity Server Documentation](https://docs.duendesoftware.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)




## License

See LICENSE.txt for license information.

