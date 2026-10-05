# Architecture

Architecture: Modular Monolith + Clean Architecture

Backend:
ASP.NET Core

Frontend:
Next.js

Database:
PostgreSQL

Payment:
Manual Payment via PurchaseRequest. PurchaseRequest is not enrollment.
Online card gateways (Stripe / PayPal / ZainCash / Qi) are out of scope for v1 and are not production blockers.
See `docs/purchase-workflow.md`.

Course Access:
Per User / Per Course using CourseEnrollment.
Access decisions go through `ICourseAccessService` (active enrollment, expiration, free preview, archived-course rule).
See `docs/course-access.md`.

Activation:
Activation Codes + Direct Activation. See `docs/activation-workflow.md`.

Student Learning:
Dashboard, My Courses, lesson progress, and continue-learning. See `docs/student-learning.md`.

Admin Panel:
Staff dashboard, purchases, students, courses, CMS, settings, users, and audit logs. See `docs/admin-panel.md`.

Consultations:
Public booking + admin state machine. See `docs/consultation-workflow.md`.

Authentication:
ASP.NET Core Identity + short-lived JWT in HttpOnly cookies + rotating refresh tokens.
Frontend and API are designed for same-site cookies (`example.com` + `example.com/api`).
In development, Next.js rewrites `/api/*` to the ASP.NET API.
In production, Caddy terminates HTTPS and routes `/` to Next.js and `/api/*` to Kestrel. See `docs/production-readiness.md`.
