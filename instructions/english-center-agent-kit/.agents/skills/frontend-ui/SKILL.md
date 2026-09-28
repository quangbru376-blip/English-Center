---
name: frontend-ui
description: Frontend and UI rules for the English Center system (React, Vite, TypeScript, Tailwind). Use for ANY page, component, layout, form, table, style, colour, icon, responsive or accessibility work, and when connecting a screen to the API, even for small tweaks, so the look stays consistent.
---
# Frontend and UI

The owner has no frontend background and must explain this code orally. Use few patterns, plain and repeated.

## What we are building
An internal management tool (dashboards, tables, forms), mostly used on desktop and usable on phones. It is NOT a marketing site. Calm, dense and clear beats decorative.

## Allowed libraries (ask before adding any other)
react, react-dom, react-router-dom, tailwindcss, lucide-react, @tanstack/react-query. Use `fetch` through one wrapper in lib/http.ts. No UI kit, animation library, form library or state library.

## Code structure
1. A page (features/<module>/pages) fetches data through a React Query hook in api.ts, handles loading, error and empty, and composes components.
2. Components are small and receive data by props. Shared building blocks live in components/ui: Button, Input, Select, Table, Modal, Badge, PageHeader, EmptyState, ErrorMessage, Spinner. Build each once and reuse; never re-implement them in a page.
3. API types in features/<module>/types.ts mirror the backend DTOs. No `any`.
4. Token handling only in lib/auth. Route guards by role; the sidebar shows only what the role may use. Hiding UI is convenience only; the backend enforces access.
5. Forms: controlled inputs, validate on submit, show the server ProblemDetails message at the field or form level.

## Design tokens (single source: styles/tokens.css + Tailwind theme)
Define once before building screens; afterwards use only tokens:
- 1 primary colour, 1 accent used sparingly, a neutral grey scale, semantic success / warning / danger.
- 4px spacing steps, 2–3 corner radii, at most 2 shadow levels.
- One font family with Vietnamese support (for example Be Vietnam Pro or Noto Sans); check diacritics render. A fixed type scale of 5–6 sizes.
- If the `ui-ux-pro-max` skill is installed, use it only once to propose tokens into docs/design-tokens.md. This file wins on any conflict.

## Avoid the "AI template" look
Do not use: purple/blue gradients, glassmorphism, emoji as icons, gradient text, decorative blobs, oversized hero banners, shadowed cards everywhere, invented statistics, lorem ipsum, chatty greetings, animation that does not help the task.
Do use: a clear page title with the primary action beside it, tables with sticky header and right-aligned numbers, consistent spacing, plain solid surfaces, borders over shadows, one icon set (lucide-react), realistic sample data (Vietnamese names, IELTS/TOEIC classes, VND amounts).

## Every screen has
Loading, empty, error and submitting/disabled states; visible keyboard focus; text contrast at least 4.5:1; labels on inputs; alt text on images; a confirm dialog before destructive actions.

## Responsive
Mobile-first. Check 375px, 768px and 1280px. The sidebar collapses to a menu on small screens. Tables scroll horizontally inside their own container or become stacked rows below 768px. No horizontal page scroll. Touch targets at least 40px.

## Vietnamese copy and formats
All user-facing text in natural Vietnamese, short verbs on buttons (Lưu, Hủy, Thêm học viên). Dates dd/MM/yyyy; money via `Intl.NumberFormat('vi-VN')` in VND. Keep repeated strings in one place per feature.

## Before finishing
Run build and lint. Open the page at the three widths and walk through the loading, empty and error states by hand.
