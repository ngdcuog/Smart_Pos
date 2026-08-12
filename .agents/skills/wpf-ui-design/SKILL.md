---
name: wpf-ui-design
description: >
  Design, implement, audit, and refine SmartPOS WPF/XAML user interfaces.
  Use this skill whenever creating or modifying Views, ResourceDictionaries,
  styles, ControlTemplates, DataGrids, forms, dialogs, navigation, dashboards,
  POS layouts, attendance/camera UI, reports, AI chat UI, or other visual
  presentation in the SmartPOS WPF application.
---

# SmartPOS WPF UI Design Skill

## Mission

Create professional desktop operational UI for SmartPOS using WPF and XAML.

The UI must be:

- consistent
- efficient
- restrained
- practical
- desktop-first
- aligned with the project's visual system
- compatible with MVVM

The goal is not to make the interface decorative.

The goal is to make retail workflows fast, understandable, and visually coherent.

---

## Required reading

Before modifying UI, read:

1. `/AGENTS.md`
2. `/project_context.md`
3. `/docs/DESIGN.md`
4. `/docs/UI_SCREENS.md`

If the task targets a specific screen, read that screen's section before coding.

If existing style dictionaries are present, inspect them before creating new resources.

---

## Priority order

When requirements conflict, use this order:

1. Explicit user instruction
2. `AGENTS.md`
3. `docs/UI_SCREENS.md`
4. `docs/DESIGN.md`
5. Existing established project UI patterns
6. General WPF conventions
7. Your own design preference

Do not override project rules because a different style seems more visually interesting.

---

# Workflow A — Creating a new screen

## Step 1: Identify the task

Before writing XAML, determine:

- Who is the primary user?
- What is the primary task?
- What is the primary action?
- What information must remain visible?
- What can be secondary?
- Is the screen operational, analytical, or short interaction?

Examples:

- POS = operational / high density / speed
- Product management = operational / table-centric
- Attendance = short interaction / low density
- Reports = analytical
- AI Chat = analytical/conversational

---

## Step 2: Inspect existing resources

Check for existing:

- brushes
- typography styles
- button styles
- text input styles
- DataGrid styles
- card styles
- navigation styles
- icons

Reuse before adding.

Never create a near-duplicate style only because the new screen has a slightly different layout.

---

## Step 3: Create information hierarchy

Sketch the layout using plain structure before styling.

Example:

```text
Page
  Header
    Title
    PrimaryAction

  Toolbar
    Search
    Filters

  MainContent
    DataGrid
```

For POS:

```text
Page
  Left
    Barcode
    Search
    ProductList

  Right
    Cart
    Summary
    Checkout
```

Do not start with shadows, colors, or animation.

---

## Step 4: Map to WPF layout

Preferred WPF layout tools:

- `Grid` for primary page structure
- `DockPanel` for stable shell/toolbar layouts where appropriate
- `StackPanel` for small linear groups
- `UniformGrid` only where uniformity is desired
- `ScrollViewer` only around content that truly needs scrolling
- `ItemsControl` / `ListBox` / `DataGrid` for repeated data

Avoid deeply nested `StackPanel` trees when a Grid would express alignment better.

Avoid absolute positioning.

---

## Step 5: Use design tokens

Do not repeat magic values.

Use shared resources for:

- brushes
- fonts
- margins
- padding
- radius where feasible
- control styles

Good:

```xml
<Button
    Style="{StaticResource Style.Button.Primary}"
    Content="Thanh toán" />
```

Bad:

```xml
<Button
    Background="#2563EB"
    Foreground="White"
    Height="39"
    Padding="17,9"
    ... />
```

when equivalent shared styling exists.

---

## Step 6: Preserve MVVM

Bind UI state to ViewModel properties.

Examples:

- `ItemsSource`
- selected item
- commands
- loading state
- error state
- empty state
- visibility state

Avoid business decisions in converters if they belong in the ViewModel/Service.

Do not call Services directly from the View.

---

## Step 7: Implement interaction states

For relevant controls, ensure states exist or are inherited:

- hover
- pressed
- disabled
- focus
- validation

For pages, handle:

- normal
- loading
- empty
- error

For camera/AI work, show visible progress.

---

## Step 8: Desktop-size review

Check expected behavior at:

- 1366×768
- 1920×1080

Ask:

- Is the primary action still visible?
- Are important tables usable?
- Is content clipped?
- Is there unnecessary whitespace?
- Does the POS cart remain usable?
- Are dialogs within the window?

Do not solve desktop resizing by converting the screen into a mobile-style vertical page.

---

## Step 9: Build

Build the project.

Resolve:

- invalid XAML
- missing resources
- binding typos that are discoverable
- namespace issues
- duplicate keys
- style target mismatches

Do not report completion if the View cannot load.

---

## Step 10: Audit

Run the visual review checklist in this skill.

Fix major issues before declaring success.

---

# Workflow B — Refining an existing screen

Do not immediately rewrite the whole XAML.

First audit it.

Review:

1. hierarchy
2. spacing consistency
3. typography consistency
4. color discipline
5. component reuse
6. density
7. primary action clarity
8. table usability
9. empty/loading/error states
10. AI-slop patterns
11. MVVM integrity

Then make the smallest cohesive set of changes.

Preserve existing bindings unless a change is required.

---

# Workflow C — Creating a new reusable style

Before creating a style, determine:

- Is it reusable?
- Does an equivalent already exist?
- Is it semantic or page-specific?
- Should it be based on an existing style?

Preferred key naming:

```text
Style.Button.Primary
Style.Button.Secondary
Style.Button.Danger
Style.TextBox.Default
Style.ComboBox.Default
Style.DataGrid.Default
Style.Card.Default
```

Avoid generic numbered names.

---

# Workflow D — DataGrid design

When styling a DataGrid:

- prioritize scanability
- use compact row height
- use subtle separators
- differentiate selected row clearly
- align numeric columns right
- keep action columns narrow
- avoid excessive cell padding
- keep header readable
- provide empty-state handling outside or around the grid

Do not:

- turn each row into a card
- add large action buttons to every row
- use alternating saturated row colors
- use heavy borders around every cell

---

# Workflow E — POS screen

POS is keyboard/scanner-first.

Always evaluate:

- barcode input focus
- speed of adding an item
- behavior when same product is scanned twice
- visibility of cart
- visibility of final total
- checkout reachability
- stock error handling

Do not fill the screen with analytics or decoration.

Do not make product cards so large that only a few products are visible.

The checkout region must remain stable.

---

# Workflow F — Attendance / camera screen

Always provide explicit visual states:

- idle
- identifying employee
- camera initializing
- face detected
- verifying
- success
- failed
- camera unavailable
- fallback route

Do not make camera verification a dead-end.

Do not claim anti-spoofing or liveness unless implemented.

---

# Workflow G — Report screen

Before creating a chart, identify:

- metric
- time period
- comparison
- user question answered by the chart

Use charts only when they improve understanding.

Avoid:

- gradient fills
- 3D charts
- chart overload
- excessive animation
- meaningless pie charts

Include a supporting table when exact values matter.

---

# Workflow H — AI Chat screen

AI Chat must visually belong to SmartPOS.

Do not introduce a separate AI theme.

Avoid:

- purple gradient
- neon glow
- giant bot mascot
- overly rounded speech bubbles

Prioritize:

- suggested business questions
- readable answers
- clear loading state
- clear failure state
- database-derived figures
- visible time/context when useful

---

# WPF-specific design guidance

## ResourceDictionary

Use merged dictionaries for shared visual foundations.

Recommended:

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="Styles/Colors.xaml" />
    <ResourceDictionary Source="Styles/Typography.xaml" />
    <ResourceDictionary Source="Styles/Buttons.xaml" />
    <ResourceDictionary Source="Styles/Inputs.xaml" />
    <ResourceDictionary Source="Styles/Cards.xaml" />
    <ResourceDictionary Source="Styles/Tables.xaml" />
    <ResourceDictionary Source="Styles/Navigation.xaml" />
</ResourceDictionary.MergedDictionaries>
```

Adjust paths to the actual project structure.

---

## Grid first

For serious desktop layouts, prefer Grid.

Example:

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="360" />
    </Grid.ColumnDefinitions>
</Grid>
```

Use Star and Auto sizing thoughtfully.

Avoid dozens of fixed pixel widths unless the control truly requires them.

---

## Scroll behavior

Do not wrap an entire desktop page in a ScrollViewer by default.

Prefer local scrolling:

- DataGrid scrolls
- product list scrolls
- chat history scrolls

Keep headers/toolbars/checkout areas stable where possible.

---

## Visibility

Prefer binding to state.

If using converters, reuse shared converters.

Do not create many page-local converters for simple state that can be exposed by the ViewModel.

---

## Commands

Buttons and interaction controls should bind to `ICommand` / generated commands from CommunityToolkit.Mvvm.

Do not place business event handlers in XAML code-behind.

---

# Anti-AI-slop audit

A generated screen should be revised if it exhibits multiple patterns below:

- default purple accent without project justification
- gradient header
- huge rounded cards
- every section in a card
- giant title
- huge whitespace
- excessive tiny captions
- too many status colors
- icon decorations with no meaning
- emoji icons
- shadow on every surface
- floating action button
- excessive animated transitions
- dashboard containing too many KPI cards
- generic "Welcome back" hero area taking large space
- decorative "AI assistant" styling unrelated to SmartPOS
- phone-like narrow content centered on a wide desktop window

---

# Visual review checklist

Before completion, answer each question.

## Hierarchy

- Is the page purpose obvious?
- Is the primary action obvious?
- Are secondary actions visually secondary?
- Are important totals/results emphasized appropriately?

## Layout

- Does the page use available desktop space well?
- Is important content visible at 1366×768?
- Are columns aligned?
- Are table/list areas large enough?

## Consistency

- Are project colors used?
- Are spacing values on the design scale?
- Are radii consistent?
- Are text styles reused?
- Are existing component styles reused?

## Usability

- Is keyboard navigation reasonable?
- Is barcode workflow fast?
- Are errors actionable?
- Are loading states visible?
- Are empty states clear?
- Can the user recover from camera/API failure?

## Restraint

- Are there unnecessary gradients?
- Are there unnecessary shadows?
- Are there too many cards?
- Are there decorative elements with no function?
- Does the screen look like a generic SaaS template?

## Architecture

- Are bindings preserved?
- Is business logic outside the View?
- Are commands used appropriately?
- Are reusable styles centralized?

If any high-impact answer is negative, revise the screen.

---

# Output expectations when completing a UI task

When reporting work, summarize:

1. screen/components changed
2. reusable resources added/updated
3. bindings preserved/changed
4. loading/error/empty states handled
5. build result
6. any remaining UI limitations

Do not claim that a screen is polished merely because XAML compiles.
