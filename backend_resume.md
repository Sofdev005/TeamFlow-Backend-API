# TeamFlow Backend Project Resume

> Scope: .NET solution only. The `teamflow-web` frontend is intentionally excluded. This document distinguishes the API's active runtime wiring from a parallel persistence project so their schemas and behavior are not mistaken for one another.

## 1. Backend Overview & Tech Stack

### Architecture

- ASP.NET Core Web API using controllers, with a Clean Architecture / CQRS-style separation:
  - `TeamFlow.Api`: HTTP controllers, startup, middleware and SignalR hub.
  - `TeamFlow.Application`: MediatR commands/queries, handlers, validators and application interfaces.
  - `TeamFlow.Domain`: entities, enums, domain behavior, events and exceptions.
  - `TeamFlow.Infrastructure`: active API database context, PostgreSQL/JWT wiring and infrastructure services.
  - `TeamFlow.Infrastructure.Persistence`: a second, richer DbContext/configuration/migration set. Its registration method is not called by either host's startup.
- Handlers use `IApplicationDbContext`; there are no repository interfaces/implementations in the scanned backend. Mapping is primarily manual; Mapster is registered but not prominent in handlers.
- `TeamFlow.BackgroundJobs` is a separate Worker Service host, currently a logger loop, not a domain job processor.

### Exact stack

| Concern | Implementation |
|---|---|
| Runtime | .NET 8 (`net8.0`), nullable reference types and implicit usings enabled |
| HTTP | ASP.NET Core controller APIs (`[ApiController]`) |
| Database | PostgreSQL 16 in `docker-compose.yml`; EF Core 8.0.11 + `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11 in `TeamFlow.Infrastructure` |
| CQRS/mediator | MediatR 12.4.1 |
| Validation | FluentValidation 11.10.0, registered through MediatR `ValidationBehavior<TRequest,TResponse>` |
| Mapping | Mapster 7.4.0 and Mapster.DependencyInjection 1.0.1 |
| Authentication | JWT bearer (`Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.31); HMAC-SHA256 tokens |
| Passwords | PBKDF2 using SHA-512, implemented in-house with .NET cryptography APIs |
| API docs | Swashbuckle.AspNetCore 6.6.2 in `TeamFlow.Api`, enabled only in Development |
| Realtime | SignalR types and notifier implementation exist, but are not registered or mapped by startup |
| Cache | `InMemoryCacheService` exists; it is not registered. Docker has Redis, but no Redis client integration is wired |

`TeamFlow.Infrastructure.Persistence` separately references EF Core/Npgsql 8.0.10. The API references both Infrastructure projects, but its `Program.cs` calls `AddInfrastructureServices`, not `AddPersistenceServices`.

## 2. Domain Models & Database Schema

### Active API persistence

`TeamFlow.Api/Program.cs` resolves `TeamFlow.Infrastructure.Persistence.ApplicationDbContext` from `TeamFlow.Infrastructure` by calling `AddInfrastructureServices`. It applies migrations at startup and then calls `ApplicationDbContextSeeder.SeedAsync`, which currently inserts no seed data. Its migration assembly is the `TeamFlow.Infrastructure` assembly.

This active context exposes six DbSets: `Organizations`, `OrganizationMembers`, `Users`, `Tasks`, `Projects`, and `BoardColumns`. Its active migration creates those six tables. It applies configurations from the `TeamFlow.Infrastructure` assembly; the explicit `IEntityTypeConfiguration` classes are in the separate `TeamFlow.Infrastructure.Persistence` assembly and therefore are not picked up by this context. The active migration consequently reflects conventions rather than those explicit constraints/configurations.

| Entity/table | Properties (CLR name: type) | Notes |
|---|---|---|
| `User` / `Users` | `Id: Guid`; `FirstName: string`; `LastName: string`; `Email: string`; `PasswordHash: string`; `OrganizationId: Guid`; `CreatedAt: DateTime` | `Organization` navigation; active migration has an FK to `Organizations`. |
| `Organization` / `Organizations` | `Id: Guid`; `Name: string`; `Slug: string`; `PlanTier: string` (default `"Free"`); `CreatedAt: DateTime` | Domain constructor validates name/slug and normalizes slug to lowercase. |
| `OrganizationMember` / `OrganizationMembers` | `Id: Guid`; `OrganizationId: Guid`; `UserId: Guid`; `Role: OrgRole`; `Status: MemberStatus`; `InvitedByUserId: Guid?`; `JoinedAt: DateTime` | In active migration role/status are PostgreSQL `integer`; no navigation properties. |
| `Project` / `Projects` | `Id: Guid`; `OrganizationId: Guid`; `TeamId: Guid?`; `Name: string`; `Description: string?`; `IsArchived: bool`; `CreatedAt: DateTime` | Domain constructor rejects blank name; `Archive`/`Unarchive` toggle `IsArchived`. |
| `BoardColumn` / `BoardColumns` | `Id: Guid`; `BoardId: Guid`; `Name: string`; `Order: int` | The CLR model refers to a board, not a project. The active six-DbSet context does not expose a `Boards` DbSet. |
| `TaskItem` / `Tasks` | `Id: Guid`; `ProjectId: Guid`; `BoardColumnId: Guid`; `ParentTaskId: Guid?`; `Title: string`; `Description: string?`; `Priority: TaskPriority`; `AssigneeId: Guid?`; `ReporterId: Guid`; `DueDate: DateTime?`; `Order: double`; `CreatedAt: DateTime`; `UpdatedAt: DateTime` | Active migration stores `Priority` as integer and `Order` as double precision. No status property/enum exists. |

All listed entity primary keys are `Guid`; constructors replace `Guid.Empty` with a new Guid for aggregate/domain entities. There are no integer primary keys. Dates are generally assigned with `DateTime.UtcNow`. `SaveChangesAsync` in both contexts simply delegates to EF Core; there is no audit-field stamping or soft-delete filter in the active context. `TaskItem` deletion uses `DbSet.Remove` (hard delete); project archival is an explicit `IsArchived` flag, not a global soft-delete scheme.

**Domain enum values:**

| Enum | Exact values |
|---|---|
| `TeamFlow.Domain.Enums.TaskPriority` | `Low = 1`, `Medium = 2`, `High = 3`, `Urgent = 4` |
| `TeamFlow.Domain.Enums.OrgRole` | `Owner = 1`, `Admin = 2`, `Manager = 3`, `Developer = 4`, `Viewer = 5` |
| `TeamFlow.Domain.Enums.MemberStatus` | `Invited = 1`, `Active = 2`, `Suspended = 3` |
| `TeamFlow.Domain.Enums.NotificationType` | `TaskAssigned = 1`, `UserMentioned = 2`, `TaskDueSoon = 3`, `TaskOverdue = 4`, `OrgInvitation = 5` |

`TaskPriority`, `OrgRole`, and `MemberStatus` are enum-valued entity properties and are represented as PostgreSQL integers by EF Core convention. `NotificationType` is defined but is not referenced by a persisted entity in this model. There is **no `TaskStatus` enum** in the Domain project. `TaskPriority` is not zero-based; do not assume the sample values in the request.

### Parallel persistence model (not active in API startup)

`TeamFlow.Infrastructure.Persistence.TeamFlowDbContext` includes additional DbSets: `Teams`, `TeamMembers`, `Boards`, `Comments`, `Attachments`, `Labels`, `TaskLabels`, and `RefreshTokens`, alongside the six above. It adds query filters for `Project` and `Label` based on `ICurrentUserService.ActiveOrgId`. Its own migration/snapshot lives under `TeamFlow.Infrastructure.Persistence` and defines this broader model. `AddPersistenceServices` registers this context, but is not called by `TeamFlow.Api/Program.cs` or `TeamFlow.BackgroundJobs/Program.cs`.

The explicit configuration classes in that project set `Organization.Name` max 150, `Slug` max 100 and unique; `User.Email` max 256 and unique; a unique `(UserId, OrganizationId)` index for `OrganizationMember`; and `TaskItem.Title` max 250 plus `(ProjectId, BoardColumnId, Order)` index. These are **not** the configurations applied to the active API context. The active migration's `Users.Email` and `Tasks.Title` are text columns, and it does not include the above unique/index configuration. Do not treat the two migration histories as one schema.

Additional Domain entities in the broader context:

| Entity | Properties |
|---|---|
| `Board` | `Id: Guid`, `ProjectId: Guid`, `Name: string` |
| `Team` | `Id: Guid`, `OrganizationId: Guid`, `Name: string`, `Description: string?` |
| `TeamMember` | `Id: Guid`, `TeamId: Guid`, `UserId: Guid` |
| `Comment` | `Id: Guid`, `TaskId: Guid`, `AuthorId: Guid`, `Body: string`, `CreatedAt: DateTime`, `EditedAt: DateTime?` |
| `Attachment` | `Id: Guid`, `TaskId: Guid`, `FileName: string`, `BlobUrl: string`, `UploadedByUserId: Guid`, `SizeBytes: long`, `ContentType: string` |
| `Label` | `Id: Guid`, `OrganizationId: Guid`, `Name: string`, `Color: string` |
| `TaskLabel` | `Id: Guid`, `TaskId: Guid`, `LabelId: Guid` |
| `RefreshToken` | `Id: Guid`, `UserId: Guid`, `TokenHash: string`, `ExpiresAt: DateTime`, `CreatedByIp: string`, `RevokedAt: DateTime?`, `ReplacedByTokenId: Guid?`; computed `IsExpired`, `IsRevoked`, `IsActive` |

Most model relationships are represented only by Guid fields, not EF navigation properties. The active migration only creates the `Users.OrganizationId` FK; do not infer database FKs or cascade relationships for other Guid references from the CLR property names alone.

## 3. API Controllers & Routing

All route paths below are case-insensitive in ASP.NET Core. Routes derived from `[controller]` expand to the controller name, e.g. `api/[controller]` becomes `api/Projects`.

| Controller / method | Route | Authorization | Request and response |
|---|---|---|---|
| `AuthController.Register` | `POST /api/Auth/register` | `[AllowAnonymous]` at controller | Body `RegisterCommand(FirstName, LastName, Email, Password, OrganizationName)`; `200` with `AuthenticationResult(UserId, FirstName, LastName, Email, OrgId, Token)`. |
| `AuthController.Login` | `POST /api/Auth/login` | `[AllowAnonymous]` at controller | Body `LoginQuery(Email, Password)`; same `200` result. |
| `ProjectsController.CreateProject` | `POST /api/Projects` | `[Authorize]` | Body `CreateProjectCommand(Name, Description?, TeamId?)`; `201` with `ProjectResponse(Id, OrganizationId, TeamId, Name, Description, IsArchived, CreatedAt)`. `CreatedAtAction` targets the collection `GetProjects` and supplies an `id` route value. |
| `ProjectsController.GetProjects` | `GET /api/Projects` | `[Authorize]` | No body; `200 List<ProjectResponse>`. |
| `BoardsController.CreateColumn` | `POST /api/Boards/columns` | `[Authorize]` | Body `CreateBoardColumnCommand(ProjectId, Name, Order)`; controller expects a `BoardColumnResponse`. No matching command handler is present in the scanned Application source, so dispatch is expected to fail at runtime. |
| `BoardsController.GetColumns` | `GET /api/Boards/columns/{projectId:guid}` | `[Authorize]` | Controller passes the path Guid to `GetBoardColumnsQuery(Guid BoardId)`; handler filters by `BoardColumn.BoardId`, not ProjectId. Returns `List<BoardColumnResponse>` if dispatched. |
| `BoardsController.ReorderColumns` | `PUT /api/Boards/columns/reorder` | `[Authorize]` | Body `ReorderBoardColumnsCommand(BoardId, ColumnOrders)` where each `ColumnOrderDto` is `(ColumnId, NewOrder)`; `204`. |
| `TasksController.CreateTask` | `POST /api/tasks` | `[Authorize]` | Body `CreateTaskCommand(ProjectId, BoardColumnId, Title, Description?, Priority=Medium, AssigneeId?, DueDate?, Order=0, ParentTaskId?)`; `200 TaskResponse`. |
| `TasksController.MoveTask` | `PATCH /api/tasks/{taskId}/move` | `[Authorize]` | Body fields `TargetColumnId`, `NewOrder` (`TaskId` is overwritten from the route); `204`. Handler returns a `TaskResponse`, which controller discards. |
| `TasksController.UpdateTask` | `PUT /api/tasks/{taskId}` | `[Authorize]` | Intended body `UpdateTaskCommand(Id, BoardColumnId, Order, AssigneeId?, DueDate?)`; intended `200 TaskResponse`. **Currently does not compile:** controller uses `command with { TaskId = taskId }`, but `UpdateTaskCommand` has `Id`, not `TaskId`. |
| `TasksController.DeleteTask` | `DELETE /api/tasks/{taskId}` | `[Authorize]` | No body; `204`; handler hard-deletes task. |
| `OrganizationsController.InviteMember` | `POST /api/Organizations/members/invite` | `[Authorize]` | Body `InviteMemberCommand(Email, Role)`; `200 { memberId }`. |
| `OrganizationsController.GetMembers` | `GET /api/Organizations/members` | `[Authorize]` | No body; `200 List<MemberResponse(MemberId, UserId, Role, Status, JoinedAt)>`. |
| `WeatherForecastController.Get` | `GET /WeatherForecast` | `[Authorize]` | Template is `[Route("[controller]")]`, not under `/api`; sample endpoint, not TeamFlow business API. |

Application request/response models not currently exposed by a controller include `GetTasksQuery(ProjectId) -> List<TaskResponse>`; there is no GET tasks action. `TaskResponse` fields are `(Id, ProjectId, BoardColumnId, Title, Description, Priority-as-string, AssigneeId, ReporterId, DueDate, Order, CreatedAt, UpdatedAt)`. `GetBoardColumnsQuery` and its handler do exist, despite the separate missing create-column handler.

## 4. Core Business Logic & Validation

### Authentication

- Registration checks for an exact-match existing email, creates a new `Organization` and `User`, hashes the submitted password and persists both, then returns a JWT. It does not create an `OrganizationMember` row for the registering user.
- Login finds the user by exact email and verifies the password; missing user or mismatch throws `UnauthorizedAccessException`.
- JWT claims: `sub` (user Guid), `given_name`, `family_name`, `email`, `org_id` (organization Guid), and `jti`. No role claim or tenant membership/role claim is emitted.
- JWT signing: HMAC-SHA256 with `JwtSettings:Secret`; issuer/audience from `JwtSettings:Issuer` and `JwtSettings:Audience`; lifetime from `JwtSettings:ExpiryMinutes`, default 120 minutes. Bearer validation checks issuer, audience, expiry and signing key.
- Password format is `<base64 salt>.<base64 hash>`; salt 16 bytes, PBKDF2-SHA512, 100,000 iterations, 32-byte output. Verification uses `CryptographicOperations.FixedTimeEquals`.
- `RefreshToken` and `ITokenService` declarations exist, but no refresh-token service or refresh/login-refresh endpoint is wired. `ITokenService` has no implementation in the scanned code.
- Configuration keys are in appsettings/environment variables. `appsettings.json` and `docker-compose.yml` contain development/example credentials; do not treat them as production secrets.

### Tasks and ordering

- Create requires a current `UserId` from `ICurrentUserService`, sets the caller as `ReporterId`, constructs the task with the supplied `Order`, and optionally assigns a user or sets a due date.
- Move loads the task and target column, then calls `TaskItem.MoveToColumn(TargetColumnId, NewOrder)`, which directly sets `BoardColumnId`, `Order`, and `UpdatedAt`.
- There is no server-side neighbor renumbering, sorting-gap calculation, fractional-index algorithm, or collision handling. The client supplies the new `double Order`.
- `GetTasksQueryHandler` sorts all tasks for a project by `Order` but has no controller endpoint.
- `UpdateTaskCommandHandler` also calls `MoveToColumn`; non-null assignee assigns the task and records a domain event; non-null due date validates and updates it. Since null is the default, the current update command cannot clear an existing assignee or due date.
- `TaskItem.SetDueDate` rejects a date earlier than `UtcNow - 5 minutes`. `TaskItem.AssignTo` emits `TaskAssignedEvent`. No code in `SaveChangesAsync` dispatches/clears domain events.
- The task create validator requires nonempty Title (max 200), limits Description to 2000 when present, and checks `Priority.IsInEnum()`. The EF configuration in the inactive persistence assembly specifies Title max 250, so validator and alternate mapping do not agree exactly.

### Other business logic

- Creating a project requires `ActiveOrgId` from the JWT/current-user service. `GetProjectsQueryHandler` has no explicit organization filter. Since the active context does not install a tenant query filter, a logged-in caller may query projects across organizations.
- Inviting a member looks up an existing user by lowercased request email, rejects an existing `(organization,user)` membership, then adds an `OrganizationMember` with `MemberStatus.Active`. It is an immediate membership insert, not an email invitation workflow.
- Reordering board columns updates only matching columns in the specified `BoardId`; unknown column IDs are ignored. It does not enforce a complete/unique order list.

### Validation and error responses

- `AddApplicationServices` registers FluentValidation validators and the MediatR `ValidationBehavior`; the behavior runs all validators and throws `FluentValidation.ValidationException` for failures.
- Only `CreateTaskCommandValidator` was found in the Application source. Other command/domain checks are ad hoc constructors/handlers, not a global FluentValidation ruleset.
- `GlobalExceptionMiddleware` maps FluentValidation exceptions to 400 `application/problem+json` with an `errors` extension dictionary grouped by property; `DomainException` to 422; `KeyNotFoundException` to 404; `UnauthorizedAccessException` to 401; and other exceptions to generic 500.
- **The middleware is not registered in `TeamFlow.Api/Program.cs`** (`app.UseMiddleware<GlobalExceptionMiddleware>()` is absent). Therefore that custom mapping is not active. `[ApiController]` can still produce automatic 400 responses for model-binding / DataAnnotations model-state errors, but that is separate from MediatR FluentValidation exceptions.

## 5. Crucial Services & Interfaces

| Interface / class | Actual role and methods |
|---|---|
| `IApplicationDbContext` | `DbSet` properties for `Organizations`, `OrganizationMembers`, `Users`, `Tasks`, `Projects`, `BoardColumns`; `SaveChangesAsync(CancellationToken)`. Implemented by two separate contexts. |
| `ICurrentUserService` / `CurrentUserService` | `UserId`, `ActiveOrgId`, `IsAuthenticated`; reads JWT/HTTP claims (`sub`/name identifier and `org_id`). Registered scoped in Infrastructure. |
| `IJwtTokenGenerator` / `JwtTokenGenerator` | `GenerateToken(userId, firstName, lastName, email, orgId)`; registered singleton. |
| `IPasswordHasher` / `PasswordHasher` | `HashPassword`, `VerifyPassword`; registered singleton. |
| `ICacheService` / `InMemoryCacheService` | Async get/set/remove over `IMemoryCache`, default absolute expiry five minutes. Implementation is not registered and `AddMemoryCache` is not called. |
| `IEmailSender` | `SendEmailAsync(toEmail, subject, body, cancellationToken)`; no implementation found. |
| `IRealtimeNotifier` / `SignalRRealtimeNotifier<THub>` | Task created/updated/deleted/moved group notifications. Implementation is not registered; no `AddSignalR` or `MapHub<BoardHub>` in host startup. |
| `ITokenService` | Access/refresh token and hash method declarations; no implementation found. |
| `GlobalExceptionMiddleware` | Custom exception-to-ProblemDetails mapper; defined but not added to API pipeline. |

There are no `ITaskRepository`, `IProjectRepository`, or other repository abstractions in the scanned code. The API CORS policy is named `AllowFrontend`, allows only origin `http://localhost:3000`, any header/method, and credentials. The pipeline calls CORS, authentication, authorization and controller mapping. Swagger/UI are Development-only. Database migrations/seeding are attempted on API startup; exceptions are logged and swallowed so startup can continue without a migrated database.

## 6. Current Gaps / Onboarding Warnings

- `dotnet build TeamFlow.sln --no-restore` currently fails in `TeamFlow.Api/Controllers/TasksController.cs`: CS0117, `UpdateTaskCommand` has no `TaskId` property. The record exposes `Id`.
- Board column creation has a command and API action but no MediatR handler. The `CreateBoardColumnCommand` also contains `ProjectId`, while `BoardColumn` requires `BoardId`.
- Board-column GET names its URL parameter `projectId`, but the query/handler treats that Guid as `BoardId`; response DTO names its second field `ProjectId` although handlers pass `BoardId`.
- A task-list query/handler exists but no API GET route exposes it.
- Active and parallel persistence projects have different DbContexts, migration histories, entity coverage and configuration; API startup currently uses only `ApplicationDbContext` from `TeamFlow.Infrastructure`.
- Exception middleware, SignalR, cache, email, and refresh-token services are not fully wired. Do not describe their declarations as active runtime features.
- No general tenant authorization/query filtering is active on `ApplicationDbContext`; protected routes alone do not scope all reads/writes to `org_id`.
- Test projects contain a small `TaskItem` domain test and a `MoveTaskCommand` record-construction test; API/Application placeholder tests are empty. No meaningful API integration test coverage was identified.
