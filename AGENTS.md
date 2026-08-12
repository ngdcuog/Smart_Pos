# SmartPOS — Codex Agent Rules

## 1. Project identity

SmartPOS is a WPF desktop retail/POS application built for PRN212.

Core stack:

- WPF on .NET 8
- MVVM
- CommunityToolkit.Mvvm
- Entity Framework Core + SQL Server
- OpenCvSharp/OpenCV for optional local face verification
- ZXing.Net for barcode scanning
- OpenAI or Gemini REST API for AI analysis
- LiveCharts2 for charts
- QuestPDF or WPF PrintDialog for invoices

Primary user-facing language: Vietnamese.

---

## 2. Mandatory context

Before implementing or modifying code, inspect the relevant project files.

For UI work, always read:

1. `project_context.md`
2. `docs/DESIGN.md`
3. `docs/UI_SCREENS.md` for the target screen
4. `docs/UI_REVIEW_CHECKLIST.md` if present
5. `.agents/skills/wpf-ui-design/SKILL.md`

For business logic, also consult:

- `pos_project_detailed_plan.md`

If generated code conflicts with these documents, the project-specific documents win.

---

## 3. Architecture rules

- Follow MVVM.
- Use `CommunityToolkit.Mvvm` for observable properties and commands when practical.
- Business logic belongs in Services.
- ViewModels coordinate state, commands, and Services.
- Views contain presentation only.
- Do not place business rules in `.xaml.cs`.
- Code-behind is allowed only for strictly visual/platform concerns that cannot reasonably be expressed through binding/behavior.
- Do not create tightly coupled View-to-Service calls.
- Use dependency injection where the existing project architecture supports it.
- Avoid static global state.
- Camera/barcode processing must not block the UI thread.
- UI updates from background work must be marshalled safely to the UI thread.

---

## 4. UI source of truth

`docs/DESIGN.md` is the visual source of truth.

`docs/UI_SCREENS.md` is the UX/layout source of truth for individual screens.

Do not invent a new visual language for a single page.

Prefer reusable resources and controls over page-local styling.

---

## 5. Non-negotiable visual rules

Never introduce these unless the user explicitly requests them:

- gradients
- glassmorphism
- neon effects
- glowing controls
- blurred translucent panels
- oversized rounded cards
- excessive drop shadows
- giant hero headings
- marketing landing-page layouts
- decorative floating shapes
- emoji as application icons
- mixed icon families
- arbitrary colors outside the project palette
- arbitrary corner radius values
- arbitrary spacing values
- unnecessary animation
- card-inside-card nesting without functional reason
- overuse of KPI cards
- oversized empty whitespace typical of web landing pages

SmartPOS must look like a professional operational desktop application, not an AI-generated SaaS landing page.

---

## 6. WPF resource rules

Visual constants must be centralized whenever reusable.

Preferred structure:

```text
Styles/
  Colors.xaml
  Typography.xaml
  Buttons.xaml
  Inputs.xaml
  Cards.xaml
  Tables.xaml
  Navigation.xaml
  Dialogs.xaml
```

Use semantic resource names, for example:

- `Brush.Background.App`
- `Brush.Background.Surface`
- `Brush.Text.Primary`
- `Brush.Border.Default`
- `Style.Button.Primary`
- `Style.Button.Secondary`
- `Style.Text.PageTitle`

Avoid repeated hardcoded values such as:

```xml
Background="#2563EB"
Margin="17"
CornerRadius="13"
```

inside many Views.

If a visual value appears repeatedly, move it into a ResourceDictionary.

---

## 7. Interaction rules

All important interactive controls must define or inherit appropriate states:

- default
- hover
- pressed
- disabled

Inputs should additionally support:

- focused
- validation error
- read-only where applicable

Important async operations should expose:

- loading/progress
- success
- recoverable error

Never leave a screen visually frozen while work is occurring.

---

## 8. Desktop-first constraints

Target the application primarily for:

- 1366×768
- 1920×1080

Critical POS actions must remain reachable without unnecessary scrolling.

Do not optimize primarily for phone-like layouts.

Avoid excessive whitespace.

Tables and data-management screens should use practical information density.

---

## 9. Navigation and roles

Respect role-specific workflows.

Manager/Admin commonly needs:

- Tổng quan
- Sản phẩm
- Kho hàng
- Nhân viên
- Chấm công
- Báo cáo
- Trợ lý AI

Cashier commonly needs:

- Bán hàng
- selected operational views only

Do not expose manager-only controls to cashier screens when authorization/state already indicates the role.

---

## 10. Data presentation rules

For monetary values:

- use consistent Vietnamese formatting
- align numeric columns to the right where practical
- make final totals visually prominent in checkout

For dates/times:

- use one consistent Vietnamese display format

For status:

- use compact semantic status badges
- do not rely only on color when text can clarify meaning

For tables:

- keep row actions compact
- avoid card-per-row designs
- preserve scanability

---

## 11. Accessibility and usability

- Use readable contrast.
- Do not use light gray text on white for important content.
- Do not use color as the only indicator of failure/success.
- Ensure logical keyboard focus order.
- Barcode workflows should be operable without constant mouse interaction.
- Do not remove labels when placeholders alone would make a form ambiguous.
- Error messages must explain what the user can do next.

---

## 12. Implementation workflow for UI tasks

For any View or visual component task:

1. Read `docs/DESIGN.md`.
2. Read the target section of `docs/UI_SCREENS.md`.
3. Inspect existing styles/resources.
4. Reuse existing components before creating new ones.
5. Identify primary task and primary action.
6. Implement hierarchy before decoration.
7. Preserve MVVM bindings/contracts.
8. Build the project.
9. Fix XAML errors and resource-resolution errors.
10. Review the result against the UI rules in this file.

Do not mark a UI task complete solely because the project compiles.

---

## 13. Change discipline

When modifying an existing screen:

- preserve working ViewModel contracts unless the task explicitly requires changing them
- avoid unrelated refactors
- avoid renaming bindings casually
- avoid introducing new dependencies without justification
- reuse existing styles before adding new ones
- do not overwrite the entire View when a focused patch is enough

---

## 14. Definition of done

A UI task is complete only when:

- the XAML builds
- bindings are preserved or intentionally updated
- no business logic was moved into the View
- project design tokens are used
- the screen follows `DESIGN.md`
- the screen follows its `UI_SCREENS.md` specification
- empty/loading/error states are handled where relevant
- primary actions are obvious
- the layout remains usable at 1366×768
- visual styling is consistent with the rest of SmartPOS
