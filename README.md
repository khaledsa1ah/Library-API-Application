# API Project

## Overview
This project was developed during my first week as an intern at Link Development. It is an ASP.NET Core Web API that implements various features and best practices, including authentication, logging, caching, rate limiting, and API versioning. The API includes endpoints for weather forecasting and user authentication.

## Features

### Authentication
- **JWT Authentication**: The project uses JSON Web Tokens (JWT) to handle authentication. The `UserController` provides an endpoint for user authentication, generating and returning a JWT on successful login. The JWT is then used to secure other endpoints.

### Authorization
- **Permission-Based Authorization**: Custom authorization filters are implemented to enforce permission-based access control on API endpoints.

### Logging
- **Serilog**: Serilog is used for structured logging. Logs are written to both the console and a rolling file. This helps in monitoring and debugging the application.

### API Documentation
- **Swagger/OpenAPI**: Swagger is configured to provide interactive API documentation. It includes JWT authentication configuration for testing secured endpoints directly from the Swagger UI.

### CORS
- **Cross-Origin Resource Sharing (CORS)**: Configured to allow any origin, method, and header. This is essential for enabling client applications from different origins to interact with the API.

### Database
- **Entity Framework Core**: The project uses Entity Framework Core to interact with a SQL Server database. The connection string is configured in the `appsettings.json` file.

### Caching
- **In-Memory Caching**: The project utilizes in-memory caching to improve performance by storing frequently accessed data. This is demonstrated in the `WeatherForecastController`.

### Rate Limiting
- **AspNetCoreRateLimit**: Configured to limit the number of requests to the API, protecting it from abuse and ensuring fair usage. Rate-limiting settings are defined in the `appsettings.json` file.

### API Versioning
- **API Versioning**: The project supports API versioning, allowing for version-specific routing and easier management of API evolution.

### Message Queuing
- **RabbitMQ Integration**: The project now includes RabbitMQ for message queuing. This allows for asynchronous communication and better scalability.
  - **Message Publishing**: A new endpoint in `MessageController` allows clients to publish messages to a RabbitMQ queue.
  - **Message Consuming**: A `MessageConsumer` background service continuously listens for messages on the queue, processes them, and logs the content to a file.

### Publishing and Deploying 
- **IIS**: During the development of this project, I also learned how to publish and deploy an ASP.NET Core application on Internet Information Services (IIS). This involves:

## Tools and Technologies Used

- **ASP.NET Core**: The framework used for building the Web API.
- **Entity Framework Core**: For database interactions.
- **JWT (JSON Web Tokens)**: For secure authentication.
- **Serilog**: For logging.
- **Swagger/OpenAPI**: For API documentation and testing.
- **CORS**: For handling cross-origin requests.
- **AspNetCoreRateLimit**: For implementing rate limiting.
- **API Versioning**: For managing API versions.
- **RabbitMQ**: For message queuing and asynchronous processing.


