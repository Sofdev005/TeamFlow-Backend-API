# 🏗️ Architecture Overview

**TeamFlow** follows a clean, decoupled layered architecture. High-level requests flow from the API Access layer down through Application Features, relying on Infrastructure interfaces and EF Core persistence to manipulate the underlying Domain Model.
 ## frontend : https://github.com/Sofdev005/TeamFlow-Project-Management-SaaS-App
---

## 🏛️ System Architecture Diagram
[![Architecture diagram of sofdev005/teamflow-backend-api](https://gitdiagram.com/sofdev005/teamflow-backend-api/diagram.png)](https://gitdiagram.com/sofdev005/teamflow-backend-api?utm_source=readme&utm_medium=picture)
[![Architecture diagram](https://gitdiagram.com/diagram-badge.svg)](https://gitdiagram.com/sofdev005/teamflow-backend-api?utm_source=readme&utm_medium=badge)

---

## 📂 System Layers Breakdown

### 1. API Access Layer
Entry point handling HTTP endpoints and WebSocket connections:
* **`AuthController.cs`**: Handles authentication and dispatches login/registration flows.
* **`Boards API` & `TasksController.cs`**: Endpoints for board column operations and task CRUD.
* **`Projects API` & `Organizations API`**: Exposes endpoints for managing workspace access, projects, and memberships.
* **`BoardHub.cs`**: SignalR hub for real-time collaboration and board updates.

### 2. Application Features Layer
Contains business logic orchestration and handlers:
* **Authentication**: Login and registration logic generating access tokens.
* **Boards & Projects**: Manages board columns and project workflows, including **Project Presence** tracking and activity recording.
* **Organization Membership & Task Management**: Enforces organization permissions and executes task creation/movement operations.

### 3. Infrastructure Layer
Supplies runtime infrastructure and abstractions:
* **Identity & Context**: `JWT Generator` handles token creation, while `Current User Context` checks permissions and identifies active users.
* **Data Access**: Operations target a `Database Contract` interface, implemented concrete by `EF Core Database`.
* **Background Tasks & Realtime**: A `Realtime Notifier` broadcasts real-time changes using `Worker.cs` background processes and `BoardHub.cs`.

### 4. Domain Model
Core entities persisted to the database:
* **`Entities`**: Base domain entities and system models.
* **`Project.cs`**: Domain model for project structure and metadata.
* **`Organization.cs`**: Domain model for organizational structures.
* **`TaskItem.cs`**: Core task behavior, column status, and movement logic.
