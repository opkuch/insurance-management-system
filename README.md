# Insurance Management System

A backend API that acts as the "engine" for an insurance agent's book of business: customers,
insurance policies (Auto / Health / Life), and the relationship between them — with issuance,
visibility/filtering, and lifecycle (endorse / cancel / activate / expire).

**Stack:** ASP.NET Core (.NET 9) · EF Core 9 · SQLite · xUnit

---

## Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)

### Run locally

```bash
dotnet run --project InsuranceManagementService
```

EF Core migrations are applied automatically on startup. The SQLite database file
(`insurance.db`) is created in the working directory (git-ignored).


| Resource   | URL                                                            |
| ---------- | -------------------------------------------------------------- |
| Swagger UI | [http://localhost:5280/swagger](http://localhost:5280/swagger) |
| Health     | [http://localhost:5280/health](http://localhost:5280/health)   |


Ports come from `InsuranceManagementService/Properties/launchSettings.json` (HTTP `5280`, HTTPS `7286`).

### Tests

```bash
dotnet test
```

Domain unit tests live under `InsuranceManagement.Tests/Models`. API integration tests
(`WebApplicationFactory` + in-memory SQLite) live under `InsuranceManagement.Tests/Integration`.

---

## Architecture

### Design

Single Web API project with inward-pointing layers (dependencies point toward the domain):

```
Controllers / ExceptionHandler / Host
        │
   Application (services, DTOs, abstractions, lifecycle worker)
        │
     Domain (entities, value objects, invariants)
        ▲
Infrastructure (EF, repos, generators) implements Application ports
```


| Layer              | Responsibility                                                                                                                                       |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Domain**         | Aggregates (`Customer`, `Policy`), value objects (`Money`, `DateRange`, `PolicyNumber`, `Address`), lifecycle invariants. No framework dependencies. |
| **Application**    | Use-case services, DTOs, repository ports, `IUnitOfWork`, paging. Hosted `PolicyLifecycleWorker` for time-driven status transitions.                 |
| **Infrastructure** | EF Core `DbContext` (also `IUnitOfWork`), configurations, repositories, transactional policy-number generator, system clock.                         |
| **Host edge**      | Controllers, RFC 7807 `ProblemDetails` handler, correlation middleware, health checks, `Program.cs` composition root.                                |


Application services keep request paths readable at this size. Domain rules throw typed exceptions; a global handler maps them to `400` / `404` / `409` / `422` / `500`.

### Data modeling

```
Customer (1) ──────── (*) Policy
                          │
                          ├── (*) Coverage
                          └── (*) PolicyTransaction  (append-only audit)
```

- **Customer** — policyholder identity (unique national ID), contact, optional address, Active/Inactive status. Policies may only be issued to **active** customers.
- **Policy** — contract aggregate: product type, term, premium, billing frequency, status (`Issued` → `Active` → `Cancelled` / `Expired`), version (incremented on endorsement). Owns coverages and an immutable transaction log.
- **Coverage** — type, limit, deductible (same currency as the policy; deductible ≤ limit).
- **PolicyTransaction** — Issued / Endorsed / Cancelled / Expired events recorded with the aggregate.
- **PolicyNumber** — `{LOB}-{YYYY}-{sequence}` (e.g. `AUTO-2026-000123`), reserved transactionally per product/year with a unique index as backstop.

**Integrity highlights:** referential restrict on customer delete while policies exist; coverages/transactions cascade with the policy; all timestamps UTC; premiums positive; monetary amounts on a policy share one currency.

**Core API surface:** customer create/list/update/activate/deactivate; issue policy; list/filter policies (product, status, customer) + customer book; get detail; endorse; cancel.

---

## Assumptions

Business and scope choices made for this challenge:

1. **Single-tenant agent book** — one shared book of business; no authentication, authorization, or multi-agent isolation. Any caller can use the API
2. **Three lines of business** — Auto, Health, and Life are enough to demonstrate product typing; no full product catalog or rating engine.
3. **Active-customer gate** — inactive customers cannot receive new policies; existing policies are not auto-cancelled on deactivate.
4. **Lifecycle transitions** — API status and list filters are derived from term dates and cancellation (`CurrentStatus`). Stored status is materialized by detail refresh / an hourly background sweep so expiry can still append an audit transaction.
5. **Endorsement model** — mid-term changes update premium and/or coverages and append an audit row; no separate endorsement documents or pending-approval workflow.
6. **Cancellation** — requires a reason; cancelled/expired policies cannot be endorsed further. Soft lifecycle over hard deletes.
7. **Billing frequency is stored, not executed** — no invoices, payments, or premium schedules in this iteration.
8. **Claims, renewals, documents, and notifications** are out of scope — future bounded contexts, not part of the Policy aggregate.
9. **SQLite + in-process worker** — chosen for zero-infra local demo; production would typically move to a server database, externalize the sweep, and add concurrency tokens for multi-instance endorse.

