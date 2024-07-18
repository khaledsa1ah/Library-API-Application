# API Project - Library Management System

## Overview
This project, developed during my internship at Link Development, is an ASP.NET Core Web API that implements a Library Management System. It showcases various features and best practices in API development, including authentication, authorization, logging, caching, rate limiting, API versioning, and message queuing.

## Features

### Library Management
- **Books**: CRUD operations for managing books in the library.
- **Authors**: Endpoints for managing author information.
- **Categories**: API for handling book categories.

### Authentication and Authorization
- **JWT Authentication**: Uses JSON Web Tokens for secure user authentication.
- **Permission-Based Authorization**: Custom filters enforce role-based access control on API endpoints.

### Data Persistence
- **Entity Framework Core**: Utilizes EF Core for database interactions with SQL Server.
- **Code-First Migrations**: Includes database schema management through EF Core migrations.

### Caching
- **In-Memory Caching**: Implements caching for frequently accessed data like book lists and author information.

### Logging
- **Serilog**: Structured logging to console and rolling file for comprehensive application monitoring.

### API Documentation
- **Swagger/OpenAPI**: Interactive API documentation with JWT authentication support for testing secured endpoints.

### CORS
- **Cross-Origin Resource Sharing**: Configured to allow specified origins, methods, and headers.

### Rate Limiting
- **AspNetCoreRateLimit**: Protects the API from abuse and ensures fair usage.

### API Versioning
- Supports versioning to manage API evolution and backward compatibility.

### Message Queuing
- **RabbitMQ Integration**: 
  - Message publishing endpoint for asynchronous operations.
  - Background service for consuming and processing queued messages.

### Controllers
- **BooksController**: Manages book-related operations (GET, POST, PUT, DELETE).
- **AuthorsController**: Handles author data management.
- **CategoriesController**: Manages book categories.
- **UserController**: Handles user authentication and token generation.
- **MessageController**: Facilitates message publishing to RabbitMQ.

## Tools and Technologies
- ASP.NET Core
- Entity Framework Core
- JWT (JSON Web Tokens)
- Serilog
- Swagger/OpenAPI
- CORS
- AspNetCoreRateLimit
- API Versioning
- RabbitMQ
- SQL Server

## Deployment
- **IIS Deployment**: The application is configured for deployment on Internet Information Services (IIS), enabling easy hosting and management on Windows servers.

## Getting Started
1. Clone the repository
2. Update the connection string in `appsettings.json` to point to your SQL Server instance
3. Run Entity Framework migrations to set up the database
4. Configure RabbitMQ connection settings if using message queuing features
5. Build and run the application

## API Endpoints
- `/api/v1/books`: Book management endpoints
- `/api/v1/authors`: Author management endpoints
- `/api/v1/categories`: Category management endpoints
- `/api/v1/user/auth`: User authentication endpoint
- `/api/v1/message`: Message publishing endpoint

For detailed API documentation, refer to the Swagger UI available at `/swagger` when running the application.
