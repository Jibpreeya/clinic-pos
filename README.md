# Clinic POS — Multi-Tenant Clinic Management System

## Architecture Overview

### Tenant Safety

TenantId flows from **JWT claims only** — never from request body/query params.

```
Login → JWT issued with tenant_id claim
       ↓
CurrentTenantService reads tenant_id from HttpContext.User
       ↓
AppDbContext receives TenantId via constructor injection
       ↓
EF Core Global Query Filters → every query adds WHERE tenant_id = @tenantId
       ↓
DB unique index (tenant_id, phone_number) as final safety net
```

This means even if a developer forgets to filter by tenant, EF Core does it automatically.

### Project Structure

```
src/
├── backend/
│   ├── ClinicPOS.Domain/        — Entities, Enums (no dependencies)
│   ├── ClinicPOS.Infrastructure/ — EF Core, Auth, RabbitMQ
│   ├── ClinicPOS.Application/   — PatientService, AppointmentService
│   ├── ClinicPOS.API/           — Controllers, Program.cs, Middleware
│   └── ClinicPOS.Tests/         — xUnit tests
└── frontend/                    — Next.js 14 App Router
```

### Design Decisions

**PrimaryBranchId on Patient (not join table):** A patient is created at one primary branch. We kept a simple FK rather than a many-to-many join table since the requirement says "may visit multiple Branches" — this is modeled at the Appointment level, which has BranchId. This avoids over-engineering for v1.

**EF Core Global Query Filters:** Chosen over middleware filtering because it's the safest approach — it's impossible to accidentally expose cross-tenant data even in complex queries or eager-loaded navigation properties.

**RabbitMQ event on Appointment:** AppointmentCreated event is published after DB save (not in-transaction). For v1 this is acceptable. For production, an outbox pattern would guarantee delivery.

---

## How to Run (One Command)

```bash
git clone <repo-url>
cd clinic-pos
docker compose up --build
```

Services:
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- RabbitMQ Admin: http://localhost:15672 (clinicpos / clinicpos123)

---

## Environment Variables

Copy `.env.example` to `.env` and adjust as needed. Default values work for local Docker.

---

## Seeded Users

| Email | Password | Role | Can Create Patients |
|---|---|---|---|
| admin@demo.com | Admin1234! | Admin | ✅ Yes |
| staff@demo.com | Staff1234! | User | ✅ Yes |
| viewer@demo.com | Viewer1234! | Viewer | ❌ No (403) |

---

## API Examples (curl)

### Login
```bash
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"Admin1234!"}' | jq -r .token)
```

### Create Patient
```bash
curl -X POST http://localhost:5000/api/patients \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe","phoneNumber":"0811234567"}'
```

### List Patients
```bash
curl http://localhost:5000/api/patients \
  -H "Authorization: Bearer $TOKEN"
```

### List Patients filtered by Branch
```bash
curl "http://localhost:5000/api/patients?branchId=<BRANCH_UUID>" \
  -H "Authorization: Bearer $TOKEN"
```

### Create Appointment
```bash
curl -X POST http://localhost:5000/api/appointments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"branchId":"<BRANCH_UUID>","patientId":"<PATIENT_UUID>","startAt":"2026-03-01T09:00:00Z"}'
```

### Viewer cannot create patients (403)
```bash
VIEWER_TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"viewer@demo.com","password":"Viewer1234!"}' | jq -r .token)

curl -X POST http://localhost:5000/api/patients \
  -H "Authorization: Bearer $VIEWER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","phoneNumber":"0999999999"}'
# Returns 403 Forbidden
```

---

## How to Run Tests

```bash
cd src/backend
dotnet test ClinicPOS.Tests
```

Tests cover:
1. Tenant isolation — Tenant A cannot see Tenant B's patients
2. Duplicate phone rejected within same tenant
3. Same phone allowed across different tenants

---

## Assumptions & Trade-offs

- Auth uses JWT (stateless). No refresh token for v1 simplicity.
- RabbitMQ event is published after-commit (not outbox). Acceptable for v1.
- Migrations are auto-applied on startup via `MigrateAsync()`.
- No Redis cache in this slice (chosen C+E over D). Cache keys would be `tenant:{id}:patients:list:{branchId|all}`.
