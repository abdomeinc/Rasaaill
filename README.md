# Rasaaill Solution

Rasaaill is a modern .NET 9 solution focused on providing robust, secure, and extensible client communication services. The solution is designed with maintainability, security, and performance in mind, leveraging the latest C# (version 13.0) features and best practices.

## Solution Overview

Rasaaill is structured as a modular solution, making it easy to extend and integrate into a variety of .NET applications. Its primary focus is on secure authentication token management and efficient client communication logic.

## Projects

### 1. Client.Core

**Description:**  
The `Client.Core` project contains the core logic and service interfaces required for client communications. It defines the essential abstractions and contracts for secure token storage, retrieval, and deletion, as well as other foundational services.

**Key Features:**  
- Secure storage, retrieval, and deletion of authentication tokens
- Asynchronous APIs for non-blocking operations
- Extensible interfaces for custom implementations
- Designed for integration with modern .NET applications

**Main Components:**  
- `ITokenStorageService`: Interface for secure token management
- Core service abstractions for client communication

### 2. Client.WPF

**Description:**  
The `Client.WPF` project is a WPF-based desktop client application. It provides the user interface and interaction logic for end users, leveraging the services and abstractions defined in `Client.Core`.

**Key Features:**  
- Modern WPF UI for chat and communication
- MVVM architecture for maintainability
- Integration with core client services

### 3. Server.SignalR

**Description:**  
The `Server.SignalR` project implements the server-side logic for real-time communication using SignalR. It handles authentication, user management, and message routing between clients.

**Key Features:**  
- SignalR hubs for real-time messaging
- RESTful API controllers for authentication and verification
- Secure JWT-based authentication and refresh token management
- Integration with Entity Framework Core for data persistence

### 4. Shared.Services

**Description:**  
The `Shared.Services` project provides reusable service implementations and interfaces that are shared between the client and server. This includes email verification, file transfer, and user presence services.

**Key Features:**  
- Email sending and verification code management
- File transfer and discovery services
- User presence and management interfaces

### 5. Entities

**Description:**  
The `Entities` project defines the core data models and entity relationships used throughout the solution. It includes user, message, conversation, and token models, as well as data seeding utilities.

**Key Features:**  
- Strongly-typed entity models for users, messages, conversations, and tokens
- Data seeding for development and testing
- Entity Framework Core integration

### 6. Shared

**Description:**  
The `Shared` project contains shared enums, constants, and types used across all other projects to ensure consistency and reduce duplication.

**Key Features:**  
- Common enumerations (e.g., message state, conversation type)
- Shared constants and utility types

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 or later (recommended)

### Building the Solution

1. Clone the repository.
2. Open the solution in Visual Studio.
3. Build the solution using __Build > Build Solution__ or via command line.

### Running Unit Tests

- Use the Visual Studio __Test Explorer__.
- Or run from the command line:  
  `dotnet test`

## Usage

To use Rasaaill in your application, implement the provided interfaces (such as `ITokenStorageService`) to handle authentication tokens securely and efficiently. Integrate the core services into your client communication workflows as needed.

## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements and bug fixes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
