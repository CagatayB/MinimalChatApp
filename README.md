# MinimalChatApp (.NET 10 & Blazor WASM)
A real-time, private messaging application built with ASP.NET Core SignalR and Blazor WebAssembly. This project demonstrates a Clean Architecture approach to handling real-time data persistence and bi-directional communication.

🌟 Key Features
- Real-Time Messaging: Instant one-to-one communication using SignalR Hubs.
- Clean Architecture: Strict separation of concerns (Domain, Application, Infrastructure, API).
- Data Persistence: Automatic message history logging using Entity Framework Core and SQLite.
- Typing Indicators: Real-time feedback when the other user is composing a message.
- Strongly Typed Hubs: Use of Hub<IChatClient> to ensure type safety and maintainable code.
- Minimal API: High-performance, lightweight backend endpoints.

## 🏗️ Tech Stack
- Backend: .NET 10, SignalR, Minimal APIs, Docker.
- Frontend: Blazor WebAssembly.
- Database: Entity Framework Core (SQLite).
- Architecture: Clean Architecture.

## Project Structure
- ChatApp.Domain: Contains core entities (e.g., ChatMessage) and domain logic. No dependencies.
- ChatApp.Application: Defines interfaces (IChatClient) and business contracts.
- ChatApp.Infrastructure: Handles data persistence, DbContext, and repository implementations.
- ChatApp.API: The entry point. Contains SignalR Hubs, Middleware configuration, and Minimal API endpoints.
- ChatApp.Client: The Blazor WASM frontend that consumes the SignalR service.

## Installation & Run

1. Clone repo: <br/>
```
git clone https://github.com/yourusername/MinimalChatApp.git
cd ChatApp
```

2. Quick Start (Docker)
```
docker-compose up -d --build 
```
 
3. Run the Blazor Client: <br/>
```
dotnet run --project src/ChatApp.Client
```

4. Test the App: <br/>
Open two browser tabs. Connect as "UserA" in one and "UserB" in the other to start a real-time conversation.

### CORS & Security
The project is configured with a strict CORS policy to allow secure credential sharing between the Blazor WASM origin and the SignalR backend, preventing unauthorized cross-site access.

## Improvements
- [x] Backend: .NET 10 Clean Architecture.
- [x] Real-time: SignalR with Typed Hubs and Typing Indicators.
- [x] Identity & JWT: Secure connections with Bearer Tokens.
- [x] Persistence: SQLite with Docker Volumes.
- [ ] Redis Backplane: Scale the SignalR hub across multiple server instances.
- [ ] File Sharing: Allow users to send images/attachments over SignalR.
- [x] Unit Testing: Implement XUnit tests for the Application and Hub logic.
