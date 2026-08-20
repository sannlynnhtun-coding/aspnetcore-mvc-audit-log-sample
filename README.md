# Audit Log Sample – ASP.NET Core MVC

Clean sample showing:

- **CRUD + Approve/Reject** → simple `Controller` + `Service` with Dependency Injection
- **Audit Log** → **MediatR only** (`AuditEvent` → `AuditEventHandler`)
- **SQLite persistence** → EF Core stores users and audit logs in `AuditLogSample.db`
- **Target resolution** → audit events can pass a user id, email, or mobile number and the handler stores the resolved user name/id
- Tailwind CSS for modern UI

## Architecture

```
Controller (UserController)
      │
      ▼
UserService  (simple DI service)
      │  1. Business logic (Create / Update / Delete / Approve / Reject)
      │  2. TrackChanges (your original helper)
      │  3. _mediator.Publish(AuditEvent)
      ▼
MediatR
      ▼
AuditEventHandler : INotificationHandler<AuditEvent>
      │  1. Resolve target user from target id / email / mobile lookup key
      │  2. Write resolved target name, user id, and mobile into the log
      ▼
IAuditLogService → SQLite AuditLog store
```

## Features

| Action   | Flow                                      | Audit Action |
|----------|-------------------------------------------|--------------|
| Create   | Status = Pending                          | CREATE       |
| Approve  | Pending → Approved                        | APPROVE      |
| Reject   | Pending → Rejected                        | REJECT       |
| Update   | TrackChanges (Name/Email/Limit)           | UPDATE       |
| Delete   | Remove user                               | DELETE       |

## Audit Log Fields

- `actor_user_id` / `actor_role`
- `action` (CREATE / UPDATE / DELETE / APPROVE / REJECT)
- `resource_type` / `target_id` / `target_lookup_key`
- `target_user_id` / `target_user_name` / `target_user_mobile_no`
- `changes` (Before → After)
- `reason`
- `action_date_time`

## Run

```bash
cd AuditLogSample
dotnet restore
dotnet run
```

Open https://localhost:5xxx (or the port shown).

The app creates `AuditLogSample.db` automatically on startup and seeds the demo
user when the `Users` table is empty. The default connection string is in
`appsettings.json`.

## Key files

- `Services/UserService.cs` – business logic + publishes `AuditEvent`
- `Events/AuditEvent.cs` – MediatR notification
- `Handlers/AuditEventHandler.cs` – writes the final audit log
- `Services/ObjectModifier.cs` – your original `TrackChanges`
- `Controllers/UserController.cs` – thin controller
- `Controllers/AuditController.cs` – list all audit logs

## Notes

- Current user is hardcoded as `maker-001` / `Admin` for demo.
  In production replace `CurrentUser` with `IHttpContextAccessor` + claims.
- Data is stored in SQLite and persists across restarts.
- Tailwind is loaded via CDN for simplicity.
