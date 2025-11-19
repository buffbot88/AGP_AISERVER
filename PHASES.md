# Roadmap: Phased Implementation Plan for ASHATAIServer AI Coding Agent

This PHASES.md defines an ordered, actionable roadmap split into small phases so Copilot (or contributors) can implement each piece independently. Each phase contains: objective, concrete tasks, exact deliverables (file paths / endpoints), acceptance criteria, and an estimate. Implement phases in order, but lower-priority/parallel work is possible after Phase 0.

---

## Phase 0 — Prep & CI baseline
- Objective
  - Make the repo easy to iterate on, testable, and safe to change.
- Tasks
  - Add GitHub Actions CI to build and run tests.
  - Add .editorconfig, CONTRIBUTING.md, CODE_OF_CONDUCT.md.
  - Add a placeholder Dockerfile and .dockerignore.
  - Add a test harness using TestServer with a MockRuntime to exercise health/process endpoints.
- Deliverables (files to add)
  - `.github/workflows/ci.yml`
  - `.editorconfig`
  - `CONTRIBUTING.md`
  - `CODE_OF_CONDUCT.md`
  - `Dockerfile` (placeholder)
  - `tests/ASHATAIServer.Tests/BasicApiTests.cs`
- Acceptance criteria
  - CI runs build and tests successfully (dotnet restore, build, test).
  - Basic health endpoint test passes locally and in CI.
- Estimate
  - 1–2 days

---

## Phase 1 — Pluggable Inference Runtime (core)
- Objective
  - Replace simulated processing with a pluggable runtime abstraction so real model backends can be integrated.
- Tasks
  - Add `IModelRuntime` interface (GenerateAsync, StreamAsync, LoadModel, UnloadModel).
  - Implement `MockRuntime` for tests.
  - Create `LlamaCppAdapter` skeleton (shell-out or client to local runtime).
  - Wire DI to resolve and call an `IModelRuntime` in `LanguageModelService`.
  - Add runtime configuration in `appsettings` and DI registration.
  - Unit tests using `MockRuntime`.
- Deliverables (files to add / modify)
  - `ASHATAIServer/Runtime/IModelRuntime.cs`
  - `ASHATAIServer/Runtime/MockRuntime.cs`
  - `ASHATAIServer/Runtime/LlamaCppAdapter.cs` (skeleton)
  - `ASHATAIServer/Services/LanguageModelService.cs` (update to use IModelRuntime via DI)
  - `appsettings.Development.json` (runtime config)
  - `tests/Runtime/MockRuntimeTests.cs`
- Acceptance criteria
  - Server compiles.
  - `ProcessPromptAsync` delegates to an `IModelRuntime` implementation.
  - Mock runtime tests confirm expected outputs and streaming behavior.
- Estimate
  - 5–10 days

---

## Phase 2 — Streaming & generate-project endpoint
- Objective
  - Support streaming model output and add a dedicated endpoint that returns structured code projects (files).
- Tasks
  - Add `POST /api/ai/generate-project` that accepts generate options and returns structured JSON `{ files: { "<path>": "<content>" }, metadata: {...} }`.
  - Implement token-level streaming for `/api/ai/process` via SSE or WebSocket when runtime supports it.
  - Add request/response models: `GenerateProjectRequest`, `GenerateProjectResponse`.
  - Implement server-side file sanitization and limits (allowed extensions, size).
- Deliverables
  - `ASHATAIServer/Controllers/ProjectController.cs`
  - `ASHATAIServer/Models/GenerateProjectRequest.cs`
  - `ASHATAIServer/Models/GenerateProjectResponse.cs`
  - `ASHATAIServer/Streaming/SseMiddleware.cs` or `WebSocketHandler.cs`
  - Documentation snippet in `README.md` describing the endpoint and schema
- Acceptance criteria
  - `/api/ai/generate-project` returns valid JSON with `files` and `metadata` when called against MockRuntime.
  - Token streaming emits SSE events that clients can consume.
- Estimate
  - 4–8 days

---

## Phase 3 — Security & auth hardening
- Objective
  - Harden authentication and protect endpoints for distribution.
- Tasks
  - Replace SHA256 password hashing with Argon2 or bcrypt with per-user salts.
  - Add API-key middleware and admin endpoints to create/revoke keys (secure storage).
  - Add rate-limiting middleware (per-IP and per-key).
  - Add TLS configuration guidance and environment options for enabling HTTPS in Kestrel.
- Deliverables
  - `ASHATAIServer/Services/Auth/ApiKeyService.cs`
  - `ASHATAIServer/Middleware/ApiKeyMiddleware.cs`
  - Updated `AuthenticationService` to use Argon2/Bcrypt
  - `docs/SECURITY.md`
- Acceptance criteria
  - Passwords stored with secure hashing algorithm.
  - API key middleware blocks unauthorized requests to protected endpoints (configurable).
  - Rate limiting in place and configurable.
- Estimate
  - 3–6 days

---

## Phase 4 — Packaging & deployment
- Objective
  - Make server easy to download, run, and update via Docker and platform-specific artifacts.
- Tasks
  - Create production `Dockerfile` (multi-stage) and `docker-compose.yml` examples (mount models).
  - Add `publish` scripts for single-file Windows/Linux builds (dotnet publish options).
  - Provide Windows install/run guidance (PowerShell script, optional MSI/MSIX instructions).
  - Add mount examples and env var docs for `ModelsDirectory` and TLS locations.
- Deliverables
  - `Dockerfile` (production)
  - `docker-compose.yml` (example)
  - `scripts/publish.ps1` and `scripts/publish.sh`
  - `docs/DEPLOYMENT.md`
- Acceptance criteria
  - Docker image builds and runs; hitting `http://localhost:7077/api/ai/health` returns healthy.
  - Publish scripts produce artifacts consistent with docs.
- Estimate
  - 3–7 days

---

## Phase 5 — Client tooling (WinForms, CLI, templates)
- Objective
  - Provide official clients so users can connect, request projects, and save artifacts easily.
- Tasks
  - Finalize WinForms terminal client: persistent secure settings, syntax highlighting, token storage (Windows Credential Manager).
  - Create CLI (`dotnet` tool) to call `/api/ai/generate-project` and save files.
  - Provide project templates (.csproj, .sln, .gitignore) and examples for prompt instructions.
- Deliverables
  - `AGPClientWinForms/` project (complete)
  - `agp-cli/` project + `dotnet tool` manifest
  - `templates/` (WinForms, Console, Web API)
  - `docs/CLIENTS.md`
- Acceptance criteria
  - WinForms client connects and saves a generated project that builds with `dotnet build`.
  - CLI fetches project and saves to disk.
- Estimate
  - 4–8 days

---

## Phase 6 — Tests, monitoring, observability, release
- Objective
  - Ensure quality, provide monitoring, and deliver release artifacts.
- Tasks
  - Add unit and integration tests to reach baseline coverage.
  - Add metrics endpoint (Prometheus) and OpenTelemetry instrumentation (optional).
  - Add release workflow to build Docker image, run tests, and create GitHub Release artifacts.
  - Create an integration test that generates a small WinForms app and builds it inside a container.
- Deliverables
  - `.github/workflows/release.yml`
  - `tests/integration/GenerateProjectIntegrationTests.cs`
  - `Monitoring/` (metrics middleware)
  - `docs/RELEASE.md`
- Acceptance criteria
  - Release pipeline produces Docker image and zipped publish artifacts.
  - Integration test builds the generated sample app successfully in CI.
- Estimate
  - 3–6 days

---

## Phase 7 — Optional advanced features (long-term)
- Objective
  - Add advanced capabilities for power users and larger deployments.
- Candidate tasks
  - GPU acceleration backends and multi-runtime support.
  - Fine-tuning / LoRA endpoints (careful with resource/security).
  - Plugin/skill architecture for multi-step workflows.
  - Model marketplace / template gallery and UI for model management.
  - WebSocket-based collaborative coding sessions / streaming.
- Deliverables
  - TBD by feature; design docs and proposals first.
- Estimate
  - Large / ongoing

---

## Small first task (recommended)
- Create `IModelRuntime` and `MockRuntime` with unit tests so Copilot can continue building runtimes and adapters.
- Files:
  - `ASHATAIServer/Runtime/IModelRuntime.cs`
  - `ASHATAIServer/Runtime/MockRuntime.cs`
  - `tests/Runtime/MockRuntimeTests.cs`

---

## How to use this roadmap with Copilot
- Convert each deliverable into a single issue/PR target.
- Use single-file prompts for Copilot (e.g., "Create IModelRuntime.cs with interface X") so it produces small, reviewable PRs.
- Start with Phase 0 CI so PRs validate automatically.

---

## Notes & safety reminders
- Do not execute generated code automatically on the host—always run builds inside containers or user-controlled environments during early testing.
- Require explicit user confirmation before executing generated artifacts.
- Add rate-limiting and API authentication before public distribution.

---
