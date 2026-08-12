# SmartPOS UI Review Checklist

Use this checklist after implementing or substantially modifying any screen.

## Visual consistency

- [ ] Uses SmartPOS design tokens/colors
- [ ] No gradients
- [ ] No glassmorphism/neon/glow
- [ ] No excessive shadows
- [ ] Radius values follow the design scale
- [ ] Spacing values follow the design scale
- [ ] Typography follows the defined hierarchy
- [ ] Icons come from one consistent family
- [ ] No emoji icons
- [ ] Reusable styles are centralized

## Layout

- [ ] Primary user task is obvious
- [ ] Primary action is obvious
- [ ] Main content uses desktop space effectively
- [ ] Layout works at 1366×768
- [ ] Layout works at 1920×1080
- [ ] Critical controls are not hidden by unnecessary scrolling
- [ ] Tables/lists have enough visible working area
- [ ] No excessive web-style empty space

## UX

- [ ] Default state is clear
- [ ] Loading state exists where needed
- [ ] Empty state exists where needed
- [ ] Error state is actionable
- [ ] Success feedback is appropriate
- [ ] Destructive actions require appropriate confirmation
- [ ] Keyboard focus order is reasonable
- [ ] Barcode workflow avoids unnecessary mouse use
- [ ] Camera/API failures provide a recovery route

## Data presentation

- [ ] Monetary values use consistent formatting
- [ ] Numeric columns are aligned appropriately
- [ ] Dates/times use consistent formatting
- [ ] Status includes readable text, not color only
- [ ] DataGrid row actions are compact
- [ ] No card-per-row anti-pattern

## Architecture

- [ ] MVVM boundaries are preserved
- [ ] No business logic added to code-behind
- [ ] ViewModel contracts are preserved unless intentionally changed
- [ ] Commands/bindings are used correctly
- [ ] Reusable visual constants are not duplicated inline
- [ ] UI thread is not blocked by camera/API/database work

## Anti-AI-slop

- [ ] No giant hero section
- [ ] No purple AI-style default theme
- [ ] No decorative floating shapes
- [ ] No excessive KPI cards
- [ ] No card-inside-card nesting without reason
- [ ] No huge rounded controls
- [ ] No decorative animation
- [ ] Screen looks like an operational desktop app, not a landing page
