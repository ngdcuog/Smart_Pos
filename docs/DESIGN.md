# SmartPOS Design System

## 1. Design direction

SmartPOS is a desktop operational application for retail staff and managers.

The interface should feel:

- clean
- calm
- practical
- modern
- trustworthy
- efficient
- information-dense where work requires it
- visually consistent
- easy to learn
- fast to operate

The product is not a marketing website.

Visual inspiration can come from modern productivity and enterprise applications such as Microsoft desktop products, GitHub, Linear, Stripe Dashboard, and professional retail software, but SmartPOS must not copy another product's branding.

---

## 2. Core visual principles

### 2.1 Function before decoration

Every visual element must support:

- hierarchy
- grouping
- status
- action
- navigation
- feedback

If an element is purely decorative and makes the interface noisier, remove it.

### 2.2 Consistency before novelty

A button, table, input, badge, card, dialog, or toolbar should look and behave consistently across screens.

### 2.3 Desktop operational density

Manager screens should display enough useful information without feeling cramped.

POS screens should optimize speed and scanability.

Attendance kiosk screens can use lower information density and larger feedback because the interaction is short and focused.

### 2.4 Clear primary action

Every screen should have one obvious primary action when applicable.

Avoid several buttons competing at the same visual strength.

---

## 3. Explicit anti-patterns

Do not use:

- gradients
- glassmorphism
- frosted glass
- blurred translucent backgrounds
- neon palettes
- glowing borders
- purple AI-themed default styling
- huge rounded rectangles
- radius values above 12 px without a specific need
- excessive shadows
- heavy 3D effects
- giant dashboard titles
- hero banners
- floating blobs
- ornamental illustrations in operational screens
- emoji for app icons
- random icon families
- card inside card inside card
- one card per table row
- excessive KPI cards
- huge gaps between sections
- animated decorations
- unnecessary hover motion
- web/mobile-style floating action buttons
- unlabelled icon-only actions when meaning is ambiguous

---

## 4. Color system

Use semantic design tokens rather than directly scattering hex values in Views.

### 4.1 Core palette

| Token | Value | Usage |
|---|---:|---|
| Primary | `#2563EB` | primary actions, selected state, focus |
| PrimaryHover | `#1D4ED8` | hover |
| PrimaryPressed | `#1E40AF` | pressed |
| AppBackground | `#F8FAFC` | application shell background |
| Surface | `#FFFFFF` | cards, panels, dialogs |
| SurfaceSecondary | `#F1F5F9` | subtle grouped background |
| Border | `#E2E8F0` | normal borders |
| BorderStrong | `#CBD5E1` | stronger separators |
| TextPrimary | `#0F172A` | primary text |
| TextSecondary | `#64748B` | supporting text |
| TextMuted | `#94A3B8` | low-priority text |
| Success | `#16A34A` | successful status |
| Warning | `#D97706` | warning |
| Danger | `#DC2626` | destructive/error |
| Info | `#0284C7` | informational state |

### 4.2 Tinted status surfaces

Status badges and subtle alerts may use light semantic backgrounds.

Keep them restrained and readable.

Examples:

- success foreground + very light green background
- warning foreground + very light amber background
- danger foreground + very light red background
- info foreground + very light blue background

Do not cover large parts of the UI with saturated status colors.

### 4.3 Color discipline

Do not introduce a new color for a single page without a functional reason.

Do not use random product-category colors unless the design explicitly needs a categorical visual encoding.

---

## 5. Typography

Primary font family:

`Segoe UI`

Use the system font whenever possible to keep the desktop app native and readable.

### 5.1 Type scale

| Role | Size | Weight |
|---|---:|---|
| Page title | 24 | SemiBold |
| Section title | 18 | SemiBold |
| Panel/Card title | 16 | SemiBold |
| Body | 14 | Regular |
| Body emphasized | 14 | SemiBold |
| Secondary | 13 | Regular |
| Small | 12 | Regular |
| Large POS total | 26–30 | Bold/SemiBold |

Avoid unnecessary size variants.

### 5.2 Typography rules

- Prefer sentence case.
- Avoid all-caps except very small compact labels where justified.
- Titles should be concise.
- Supporting descriptions should usually use `TextSecondary`.
- Important totals and operational results should use weight and spacing before using extra color.

---

## 6. Spacing system

Use this spacing scale:

```text
4
8
12
16
20
24
32
40
48
```

Do not introduce arbitrary values such as:

```text
13
17
23
27
29
```

unless alignment with a platform control truly requires it.

### 6.1 Recommended usage

- Icon/text gap: 8
- Tight inline controls: 8
- Label to input: 6–8
- Form row spacing: 12–16
- Card inner padding: 16–20
- Section spacing: 24–32
- Page outer padding: 24
- Major layout column gap: 16–24

---

## 7. Corner radius

Use a small, disciplined radius scale.

| Component | Radius |
|---|---:|
| Small chips/status | 4 |
| Button | 6 |
| Input | 6 |
| Toolbar control | 6 |
| Card/panel | 8 |
| Dialog | 10 |

Never use radius > 12 without explicit justification.

Avoid pill-shaped controls unless the component is semantically a compact chip/tag.

---

## 8. Borders and shadows

### Borders

Prefer subtle borders for structure.

Typical:

```text
1px Border
```

Use stronger separators only when necessary.

### Shadows

Default: no shadow.

Dialogs, popups, or a small number of elevated surfaces may use a restrained shadow.

Never use heavy or multiple layered shadows.

A border is preferred when it provides enough separation.

---

## 9. Icons

Use one icon family consistently.

Preferred options:

- Segoe Fluent Icons
- MaterialDesignIcons.WPF if already installed/approved

Typical icon sizes:

```text
16
18
20
24
```

Rules:

- no emoji
- no mixing several icon libraries
- icon-only buttons need tooltip if meaning is not universally obvious
- icons support text; they should not become decoration

---

## 10. Buttons

### 10.1 Primary button

Use for the strongest action on a screen.

Characteristics:

- solid Primary background
- white text
- radius 6
- height 36–40
- clear hover and pressed states
- no gradient
- no shadow

Examples:

- `Thanh toán`
- `Thêm sản phẩm`
- `Lưu`
- `Xác nhận chấm công`

### 10.2 Secondary button

Use for supportive actions.

Characteristics:

- white or transparent surface
- normal border
- primary or primary-text foreground depending on context

Examples:

- `Hủy`
- `Xuất báo cáo`
- `Làm mới`

### 10.3 Danger button

Use only for destructive actions.

Examples:

- `Xóa sản phẩm`
- `Vô hiệu hóa nhân viên`

Do not use red for ordinary actions.

### 10.4 Button sizing

Typical height:

```text
36–40
```

Compact table action:

```text
30–32
```

Do not make all buttons oversized.

---

## 11. Inputs and forms

Typical height:

```text
36–40
```

Use:

- 1px border
- radius 6
- clear focus state
- persistent label where ambiguity exists
- supporting/error text below field when needed

Placeholder text should be an example or hint, not the only label for an important field.

### Validation

Validation errors must provide:

- visual error state
- short Vietnamese explanation
- actionable guidance where possible

Do not show only a red border with no explanation.

---

## 12. Cards and panels

Cards are for grouping related content, not for wrapping every object.

Default card:

- Surface background
- Border
- radius 8
- padding 16–20
- no shadow

Good use:

- KPI group
- checkout summary
- camera verification panel
- report filters

Bad use:

- each DataGrid row inside a card
- card inside card for ordinary grouping
- five separate cards when one table would be clearer

---

## 13. Tables and DataGrid

Tables are a primary SmartPOS pattern.

### 13.1 Table visual rules

- compact but readable rows
- subtle horizontal separators
- clear header
- numeric values aligned right
- text left aligned
- status shown as compact badge or text
- selected row visibly distinct
- hover state subtle
- no card-per-row
- no huge row heights

Recommended row height:

```text
40–44
```

### 13.2 Actions

Keep row actions compact.

Prefer:

- one primary quick action when needed
- optional `...` menu for secondary actions

Do not place 4–5 large buttons in every row.

### 13.3 Empty table

Display:

- short explanation
- optional relevant action

Example:

`Chưa có sản phẩm nào. Thêm sản phẩm đầu tiên để bắt đầu quản lý kho.`

---

## 14. Navigation shell

### 14.1 Desktop shell

Recommended:

```text
Left Sidebar: 220–240 px
Main Content: remaining width
```

Sidebar:

- app identity near top
- navigation groups
- current user near bottom
- settings/logout near bottom

### 14.2 Navigation items

Typical height:

```text
40–44
```

Active state:

- subtle tinted Primary background
- Primary icon/text
- no glow
- no giant pill shape

Inactive state:

- transparent background
- TextSecondary/TextPrimary depending on hierarchy

---

## 15. Page layout pattern

Management screens should generally follow:

```text
Page Header
  Title
  Optional description
  Primary action

Toolbar
  Search
  Filters
  Secondary actions

Main content
  DataGrid / chart / operational panel

Footer/status/pagination when needed
```

Do not reinvent the structure per page without a workflow reason.

---

## 16. Dashboard design

Dashboard should answer operational questions quickly.

Use at most 3–4 primary KPI cards above the fold.

Examples:

- Doanh thu hôm nay
- Đơn hàng hôm nay
- Sản phẩm sắp hết
- Nhân viên đang trong ca

Below KPI:

- revenue chart
- top-selling products
- low-stock list
- recent orders

Avoid:

- 8–12 KPI cards
- decorative charts with no decision value
- giant banner greeting

---

## 17. POS visual language

POS prioritizes speed over decoration.

Recommended:

```text
Left region: 60–68%
Product/search/scan

Right region: 32–40%
Cart/summary/checkout
```

The cart should always keep:

- current items
- quantity
- price
- total
- checkout action

visible and easy to scan.

The final amount is the strongest monetary element on screen.

Barcode input should visually remain an operational control, not a decorative search box.

---

## 18. Attendance visual language

Attendance interaction should be simpler and lower-density.

Center the experience on:

- employee identification
- camera preview
- verification state
- check-in/check-out result

Feedback must be unmistakable:

- success text
- time
- employee name
- next-step instruction

Do not rely on color only.

Camera failure must show fallback action.

---

## 19. Reports visual language

Charts support decisions; they are not decoration.

Use consistent chart typography and restrained legends.

Keep filters near the chart/table they affect.

Recommended report structure:

```text
Header
Date range + filters
KPI summary
Main chart
Supporting table
```

Avoid 3D charts, pie charts with many slices, gradient fills, or unnecessary animation.

---

## 20. AI chat visual language

AI Chat is part of the same application, not a separate futuristic product.

Do not switch to:

- purple
- neon
- glowing chat bubbles
- gradient backgrounds

Use the normal SmartPOS palette.

Suggested layout:

```text
Header
Suggested business questions
Conversation area
Composer
```

AI responses using database-derived numbers should visually distinguish:

- answer
- key figures
- time period/context

Keep the UI professional and business-oriented.

---

## 21. Dialogs

Use dialogs for short focused tasks:

- confirm destructive action
- edit compact entity
- checkout confirmation
- error requiring acknowledgement

Avoid opening dialogs for long workflows better suited to a page/panel.

Dialog:

- Surface
- radius 10
- restrained shadow
- clear title
- concise body
- actions aligned consistently

Primary action should normally appear to the right.

---

## 22. Empty, loading, error, and success states

Every relevant async/data screen should consider four states.

### Empty

Explain why the screen is empty and what the user can do.

### Loading

Use:

- progress indicator
- skeleton only if practical in WPF
- disabled duplicate-submit actions

### Error

Show:

- what failed
- whether data was saved
- recovery action

### Success

Use lightweight feedback.

Avoid interrupting users with modal success dialogs for routine operations unless confirmation is necessary.

---

## 23. Responsive desktop behavior

Primary supported windows:

- 1366×768
- 1920×1080

At smaller desktop widths:

- preserve core actions
- allow secondary metadata to compress
- allow tables to scroll horizontally when necessary
- avoid collapsing the entire UI into a phone layout

POS checkout controls must remain accessible.

---

## 24. WPF implementation mapping

Translate design concepts into WPF primitives.

| Design concept | WPF implementation |
|---|---|
| design tokens | ResourceDictionary |
| palette | SolidColorBrush resources |
| spacing tokens | Thickness resources |
| type styles | TextBlock Styles |
| component styles | Style / ControlTemplate |
| variants | keyed Styles |
| reusable layout | UserControl |
| interaction states | Triggers / VisualStateManager |
| view state | ViewModel properties + DataTriggers |
| validation | WPF Validation + templates/messages |

Avoid duplicating inline styles across Views.

---

## 25. Naming guidance

Prefer semantic names.

Good:

```text
Brush.Background.App
Brush.Background.Surface
Brush.Text.Primary
Brush.Text.Secondary
Brush.Border.Default
Brush.Action.Primary
Brush.Status.Success

Style.Button.Primary
Style.Button.Secondary
Style.Button.Danger
Style.Text.PageTitle
Style.Text.SectionTitle
Style.DataGrid.Default
Style.TextBox.Default
```

Avoid:

```text
BlueBrush
GrayBrush2
ButtonStyle1
NiceCard
CoolText
```

---

## 26. Final visual test

Before accepting a screen, ask:

1. Is the primary task immediately obvious?
2. Is the main action immediately obvious?
3. Is anything decorative without function?
4. Are spacing values consistent?
5. Are radius values consistent?
6. Are colors from the defined palette?
7. Is the page too empty for a desktop operational app?
8. Are there too many cards?
9. Does it resemble an AI-generated SaaS dashboard?
10. Is the screen usable at 1366×768?
11. Can a cashier complete common actions quickly?
12. Are loading/error/empty states handled?

If the answer exposes a problem, revise before completion.
