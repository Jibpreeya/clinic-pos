# AI_PROMPTS.md — How I Used Claude in This Build

## Strategy

I used Claude as a pair programmer, not a code generator. Every output was reviewed for:
- Tenant safety correctness
- Concurrency edge cases
- Over-engineering (rejected several suggestions)

---

## Prompt 1 — Architecture Design

**Prompt:**
> "I'm building a multi-tenant clinic POS with .NET 10, PostgreSQL, Next.js, Redis, RabbitMQ.
> 1 Tenant → many Branches. 1 Patient belongs to 1 Tenant.
> What's the safest way to implement tenant isolation that prevents developer mistakes?"

**Claude's output:**
- Option A: Middleware that checks TenantId on every request
- Option B: EF Core Global Query Filters injected via DbContext
- Option C: Row-level security at PostgreSQL level

**My decision:** Option B — EF Core Global Query Filters.

**Why I accepted it:** It's the only approach where isolation is automatic and compile-time safe. Middleware (Option A) relies on every developer remembering to filter. RLS (Option C) is powerful but adds ops complexity not justified for v1.

**What I rejected:** Claude initially suggested also adding middleware as a "defense in depth" layer. I rejected this as over-engineering — the global filter is sufficient for v1.

---

## Prompt 2 — Tenant Isolation Pattern

**Prompt:**
> "Show me the EF Core DbContext with global query filters for Patient, Branch, Appointment.
> TenantId must come from JWT claims, never from the request body."

**Accepted:** The ICurrentTenantService pattern — inject into DbContext constructor.

**Rejected:** Claude first suggested putting TenantId in a static field. Rejected immediately — not thread-safe in async contexts.

**Iteration:** Asked Claude to also handle the Seeder use case where we need to bypass the filter to write seed data across tenants. Led to SeederDbContext with SeederTenantService returning Guid.Empty.

---

## Prompt 3 — Duplicate Phone & Concurrency

**Prompt:**
> "Phone must be unique per tenant, not globally. How do I handle concurrency safely?
> Two requests creating the same patient simultaneously must not both succeed."

**Claude's suggestion:** Application-level check + DB unique index as the final guard.

**My validation:** The application check alone is NOT safe under concurrency (TOCTOU race). The DB unique index on (TenantId, PhoneNumber) is the actual guarantee. The application check is just for a friendlier error message.

**What I added:** Catch `DbUpdateException` and check for the index name to return a clean 409 Conflict instead of a 500.

---

## Prompt 4 — Appointment Duplicate Prevention

**Prompt:**
> "Prevent duplicate appointments: same PatientId + BranchId + StartAt within same Tenant.
> Must be safe under concurrent requests."

**Accepted:** DB unique index on (TenantId, PatientId, BranchId, StartAt).

**Rejected:** Claude suggested using a pessimistic lock (`SELECT FOR UPDATE`). This is correct but adds complexity. The unique index is simpler, equally safe, and lets PostgreSQL handle the race condition atomically.

---

## Prompt 5 — RabbitMQ Event Design

**Prompt:**
> "When an appointment is created, publish an AppointmentCreated event to RabbitMQ.
> Payload must include TenantId. Should this be in-transaction or after-commit?"

**Claude's output:** Explained outbox pattern (transactional) vs. after-commit.

**My decision:** After-commit for v1. Noted the trade-off in README — if the process crashes between DB save and publish, the event is lost. Acceptable for v1.

**What I rejected:** Full outbox implementation. Too complex for the timebox.

---

## Prompt 6 — Authorization

**Prompt:**
> "Viewer role cannot create patients but can view them.
> Admin and User can create. How do I enforce this with minimal code?"

**Accepted:** `[Authorize(Roles = "Admin,User")]` on POST, `[Authorize(Roles = "Admin,User,Viewer")]` on GET.

**Rejected:** Claude suggested a custom policy with IAuthorizationRequirement. Overkill for 3 static roles. Attribute-based is simpler and more readable.

---

## What I Did NOT Use AI For

- Trade-off decisions (after-commit vs outbox, simple FK vs join table for branches)
- Reviewing output for tenant safety correctness
- Deciding what NOT to build within the timebox
