markdown# TaskOrchestrator

TaskOrchestrator is a backend project in C#/.NET where I learn and practice **backend engineering**, not just CRUD.
This challenge is inspired by a real technical interview exercise I encountered a few months ago.

The goal is to design a small task orchestration system.  
We have **heavy simulations** and **light monitoring/alerts**, and we must respect strict constraints:

- Simulations always have **higher priority** than monitoring.
- Global concurrency is limited (≤ 4 workers).
- We need **backpressure** (don't accept more work than we can process).
- We support **retry**, **cancel**, **idempotence**.
- We expose a small API to **monitor and control** tasks.
- Later, a React dashboard will show tasks in near real time.

This is not meant to be production-ready, but to show how I think about **domains, invariants, and system behaviour**.

---

## Stack

- **Backend:** C# / .NET 10
- **Architecture:** DDD-inspired layered architecture
- **Async pipeline:** `Channel<T>` + `BackgroundService`
- **In-memory store:** `ConcurrentDictionary`
- **API:** Minimal API
- **Frontend:** React (coming)
- **Observability:** OpenTelemetry (coming)

---

## Architecture (layers)

The solution is structured in 4 main layers, following a DDD-inspired layered architecture:

- `TaskOrchestrator.Domain`  
  Pure domain model — no infrastructure or framework dependencies:
  - `OrchestratedTask` — core entity with enforced state transitions
  - `TaskType` — Simulation vs Monitoring
  - `TaskState` — Pending, Running, Succeeded, Failed, Cancelled, Archived
  - `DomainException` — Fail Fast pattern, explicit business errors
  - Domain rules:
    - Simulations have higher priority than Monitoring
    - Max attempts / retry policy enforced in domain
    - Strict allowed status transitions (Start, Succeed, Fail, Cancel, Archive, Retry)

- `TaskOrchestrator.Application`  
  Application layer — use cases and contracts:
  - `EnqueueTaskCommand` + `EnqueueTaskCommandHandler`
  - `ITaskRepository` — async interface with `CancellationToken`
  - Coming: `RetryTaskCommand`, `CancelTaskCommand`, `ITaskClassifier`

- `TaskOrchestrator.Infrastructure`  
  Infrastructure layer — concrete implementations:
  - `InMemoryTaskRepository` — `ConcurrentDictionary`, thread-safe
  - `TaskWorker` — `BackgroundService`, consumes `Channel<OrchestratedTask>`
  - Coming: priority channels, database persistence, AI classifier

- `TaskOrchestrator.API`  
  Minimal API to pilot the system:
  - `POST /tasks` — enqueue a task
  - `GET /tasks/{id}` — inspect task state
  - Coming: `POST /tasks/{id}/retry`, `POST /tasks/{id}/cancel`

**Tests:**

- `TaskOrchestrator.Domain.Tests` — domain invariants and status transitions
- `TaskOrchestrator.Application.Tests` — use cases and orchestration logic

---

## How it works

```
POST /tasks
    ↓
EnqueueTaskCommandHandler
    → creates OrchestratedTask (Pending)
    → saves to ITaskRepository
    → writes to Channel<OrchestratedTask>
    ↓
TaskWorker (BackgroundService)
    → reads from Channel
    → Start() → UpdateAsync (Running)
    → executes work
    → Succeed() or Fail() → UpdateAsync
    ↓
GET /tasks/{id}
    → returns current state from repository
```

## Roadmap

### V1 — Core orchestration (single-process, in-memory)

- [x] Domain model — `OrchestratedTask`, `TaskState`, `TaskType`, `DomainException`
- [x] Strict state transitions with Fail Fast pattern
- [x] `ITaskRepository` — async interface with `CancellationToken`
- [x] `InMemoryTaskRepository` — `ConcurrentDictionary`, thread-safe
- [x] `EnqueueTaskCommand` + `EnqueueTaskCommandHandler`
- [x] `TaskWorker` — `BackgroundService` consuming `Channel<OrchestratedTask>`
- [x] Async pipeline — `Channel<T>` with bounded capacity (backpressure)
- [x] Minimal API — `POST /tasks`, `GET /tasks/{id}`
- [x] Priority — Simulation > Monitoring (two channels)
- [ ] Max concurrency — ≤ 4 workers in parallel
- [ ] `POST /tasks/{id}/retry` and `POST /tasks/{id}/cancel` endpoints
- [ ] Unit tests — domain invariants and application use cases

### V2 — Reliability and observability

- [ ] Persist tasks in a real store (SQLite / EF Core)
- [ ] Retry and backoff policy
- [ ] Structured logging — OpenTelemetry
- [ ] Metrics — tasks by status, latency, queue depth
- [ ] Traces — end-to-end request tracing

### V3 — Dashboard (React)

- [ ] React dashboard — visualize tasks in near real time
- [ ] Filters by type and status
- [ ] Actions — retry and cancel from the UI
- [ ] Error handling and edge case UX

---

## Why this project

This project is my way to move beyond simple CRUD and practice:

- **Domain-driven thinking** — invariants, rules, explicit transitions
- **Systems behaviour** — priority, backpressure, concurrency, retry
- **Clean architecture** — explicit boundaries enforced by the compiler
- **API design** — for control and observability
- **Async patterns** — `Channel<T>`, `BackgroundService`, `CancellationToken`

It is not a tutorial copy — it grows step by step with intentional design decisions and the reasoning behind each one.