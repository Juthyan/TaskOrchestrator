# TaskOrchestrator

TaskOrchestrator is a backend project in C#/.NET where I learn and practice **backend engineering**, not just CRUD.
This challenge is inspired by a real technical interview exercise I encountered a few months ago.

The goal is to design a small task orchestration system.  
we have **heavy simulations** and **light monitoring/alerts**, and we must respect strict constraints:

- Simulations always have **higher priority** than monitoring.
- Global concurrency is limited (≤ 4 workers).
- We need **backpressure** (don't accept more work than we can process).
- We support **retry**, **cancel**, **idempotence**.
- We expose a small API to **monitor and control** tasks.
- Later, a React dashboard will show tasks in near real time.

This is not meant to be production-ready, but to show how I think about **domains, invariants, and system behaviour**.

---

## Architecture (layers)

The solution is structured in 4 main layers, following a DDD-inspired, layered architecture:

- `TaskOrchestrator.Domain`  
  Pure domain model:
  - `TaskDefinition` and `TaskInstance`
  - `TaskType` (Simulation vs Monitoring)
  - `TaskStatus` (Pending, Running, Succeeded, Failed, Cancelled)
  - Domain rules:
    - simulations must be prioritized
    - max attempts / retry policy
    - allowed status transitions
  - No infrastructure or framework dependencies.

- `TaskOrchestrator.Application`  
  Application layer:
  - use cases as commands/handlers:
    - `EnqueueTask`
    - `StartNextTask`
    - `RetryTask`
    - `CancelTask`
  - Orchestration of domain logic + repositories
  - Interfaces like `ITaskScheduler`, `ITaskRepository`

- `TaskOrchestrator.Infrastructure`  
  Infrastructure layer:
  - concrete `InMemoryTaskRepository` (first implementation)
  - concrete `TaskScheduler` strategy
  - later: database, queues, logging, metrics

- `TaskOrchestrator.Api`  
  Minimal API to pilot the system:
  - `POST /tasks` to enqueue tasks
  - `GET /tasks` and `GET /tasks/{id}` to inspect them
  - `POST /tasks/{id}/retry` and `POST /tasks/{id}/cancel`
  - later: endpoints for real-time dashboards

Tests:

- `TaskOrchestrator.Domain.Tests`:  
  focus on domain invariants and status transitions.

- `TaskOrchestrator.Application.Tests`:  
  focus on use cases and orchestration logic.

---

## Roadmap

### V1 — Core orchestration (single-process, in-memory)

- [ ] Domain model for tasks, types, priorities, status transitions
- [ ] Simple in-memory repository
- [ ] Basic scheduling logic respecting:
  - priority: Simulation > Monitoring
  - max concurrency: ≤ 4 running tasks
- [ ] Minimal API (enqueue, list, retry, cancel)
- [ ] Unit tests on domain and application

### V2 — Reliability and observability

- [ ] Persist tasks in a real store (e.g. SQLite/EF Core)
- [ ] Retry and backoff policy
- [ ] Backpressure configuration (queue depth limits)
- [ ] Basic metrics (number of tasks by status, latency)
- [ ] Structured logging for executions

### V3 — Dashboard (React)

- [ ] React dashboard to visualize tasks in near real time
- [ ] Filters by type/status
- [ ] Actions (retry/cancel) from the UI
- [ ] Better UX around errors and edge cases

---

## Why this project

This project is my way to move beyond simple CRUD and practice:

- **Domain-driven thinking** (invariants, rules, transitions)
- **Systems behaviour** (priority, backpressure, concurrency)
- **API design** for control and observability
- **Clean architecture** with explicit boundaries

It is not a tutorial copy: it grows step by step with tests and intentional design.
