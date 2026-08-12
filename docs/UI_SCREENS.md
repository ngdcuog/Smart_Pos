# SmartPOS UI Screen Specifications

## 1. Purpose

This document defines the expected information architecture, layout, hierarchy, and core interactions of each SmartPOS screen.

It complements `DESIGN.md`.

`DESIGN.md` answers:

> How should SmartPOS look and behave visually?

This document answers:

> What should each screen contain and how should it be arranged?

---

# 2. MainWindow / Application Shell

## Goal

Provide stable navigation and preserve orientation across the application.

## Layout

```text
┌────────────── Sidebar 230 ──────────────┬───────────────────────────────┐
│ SmartPOS                               │ Page content                  │
│                                        │                               │
│ Tổng quan                              │                               │
│ Bán hàng                               │                               │
│ Sản phẩm                               │                               │
│ Kho hàng                               │                               │
│ Nhân viên                              │                               │
│ Chấm công                              │                               │
│ Báo cáo                                │                               │
│ Trợ lý AI                              │                               │
│                                        │                               │
│ user / role                            │                               │
│ Đăng xuất                              │                               │
└────────────────────────────────────────┴───────────────────────────────┘
```

## Rules

- Sidebar width: 220–240.
- Sidebar remains visually stable between pages.
- Active navigation item is obvious but restrained.
- Do not use a top web-style mega navbar.
- Main content uses app background with page padding around 24.
- User name and role can appear near the bottom of the sidebar.
- Manager/Admin and Cashier navigation items should respect permissions.
- Do not show manager-only modules to Cashier if authorization data is available.

---

# 3. Dashboard / Tổng quan

## Primary user

Manager/Admin.

## Goal

Answer:

- Hôm nay bán được bao nhiêu?
- Có bao nhiêu đơn?
- Mặt hàng nào cần chú ý?
- Nhân sự/chấm công có vấn đề gì?
- Xu hướng doanh thu đang như thế nào?

## Recommended structure

```text
Page Header
"Tổng quan"
optional date/status

KPI row
[Doanh thu hôm nay]
[Đơn hàng hôm nay]
[Sản phẩm sắp hết]
[Nhân viên đang trong ca]

Main grid
[Revenue chart             ][Low stock]
[Revenue chart             ][Low stock]

Bottom
[Top products              ][Recent orders]
```

## KPI rules

Maximum 4 primary KPI cards above the fold.

Each KPI card:

- short label
- main value
- small useful comparison/status if available

Do not add decorative metrics without business value.

## Revenue chart

Default useful range:

- today by hour or
- last 7 days

Chart must have:

- clear units
- restrained legend
- no gradients
- no 3D effects

## Low stock

Show a compact list/table:

- product
- current stock
- minimum threshold
- status/action

Primary action can navigate to inventory/product management.

## Recent orders

Columns:

- Order ID
- Time
- Cashier
- FinalAmount
- PaymentMethod

Keep it compact.

---

# 4. POS / Bán hàng

## Primary user

Cashier.

## Goal

Complete a sale with minimal mouse interaction.

## Priority

This is the most operationally important screen.

Optimize for:

- barcode scanning
- product lookup
- cart visibility
- quantity adjustments
- final total visibility
- fast checkout

## Layout

Recommended:

```text
┌────────────────────────────── 64% ─────────────────────────┬──── 36% ────┐
│ Barcode / Search                                          │ Giỏ hàng     │
│ Category quick filters                                    │              │
│                                                           │ Item rows    │
│ Product list/grid                                         │              │
│                                                           │              │
│                                                           │ Subtotal     │
│                                                           │ Discount     │
│                                                           │ Tổng cộng    │
│                                                           │ THANH TOÁN   │
└───────────────────────────────────────────────────────────┴──────────────┘
```

Allowed range:

- left 60–68%
- right 32–40%

## Barcode area

Top-most operational control.

Contains:

- barcode input
- scan status if webcam mode exists
- optional manual product search

Behavior:

- scanner input triggers lookup quickly
- after successful scan, focus returns to barcode field
- if same product is scanned again, quantity increments
- invalid barcode produces concise feedback without breaking workflow

## Product results

Prefer compact product list or moderate card grid.

Each product should expose only useful data:

- image if available
- product name
- selling price
- stock

Avoid unnecessary description text.

Clicking product adds it to cart.

## Cart row

Each cart item:

- ProductName
- UnitPrice
- quantity decrement
- quantity
- quantity increment
- LineTotal
- remove action

Quantity controls must be compact.

## Checkout summary

Show:

- Tạm tính
- Giảm giá
- Tổng thanh toán

`Tổng thanh toán` is the strongest monetary element on screen.

Primary button:

`Thanh toán`

Button should remain clearly reachable.

## Insufficient stock

Do not allow checkout beyond available stock.

Display:

- product name
- requested quantity
- available quantity
- corrective action/instruction

## Empty cart

Do not display a large illustration.

Use concise guidance:

`Quét mã vạch hoặc chọn sản phẩm để bắt đầu đơn hàng.`

---

# 5. Checkout Dialog / Thanh toán

## Goal

Confirm payment without slowing the cashier.

## Suggested contents

- subtotal
- discount
- final amount
- payment method

Payment method options:

- Tiền mặt
- Chuyển khoản
- other methods only if implemented

For cash:

- amount received
- change due

Actions:

- `Hủy`
- `Xác nhận thanh toán`

After successful checkout:

- reset current cart
- return focus to barcode scan
- optionally offer print invoice

Avoid mandatory success modal if the workflow can use lightweight confirmation.

---

# 6. Product Management / Sản phẩm

## Primary user

Manager/Admin.

## Goal

Search, filter, add, edit, activate/deactivate, and inspect products.

## Layout

```text
Header
"Sản phẩm"
[Thêm sản phẩm]

Toolbar
[Search........................]
[Danh mục v]
[Trạng thái v]
[Kho thấp v]
[Refresh]

DataGrid
```

## Recommended columns

- image thumbnail
- product name
- barcode
- category
- cost price
- selling price
- stock quantity
- status
- actions

Avoid showing every database field by default.

## Row actions

Primary compact action:

- edit

Secondary actions can go into menu:

- view detail
- deactivate/activate
- delete only if business policy truly allows physical deletion

## Add/Edit Product

Prefer a dialog for a compact form or a side panel if many fields exist.

Fields:

- ProductName
- Category
- Barcode
- CostPrice
- SellingPrice
- StockQuantity when appropriate
- MinStockAlert
- ImagePath/image selection

Use explicit labels.

---

# 7. Inventory / Kho hàng

## Primary user

Manager/Admin.

## Goal

Understand stock status and record stock movement.

## Page header

Title:

`Kho hàng`

Primary action:

`Nhập kho`

## Summary

A small summary area may show:

- tổng sản phẩm
- sắp hết hàng
- hết hàng

Avoid more than 3 summary cards unless necessary.

## DataGrid columns

- product
- category
- stock quantity
- MinStockAlert
- stock status
- last stock transaction date if available

Status:

- Đủ hàng
- Sắp hết
- Hết hàng

## Stock import flow

Dialog/panel:

- Product
- Quantity
- optional note if schema later supports it

On save:

- update stock
- record StockTransaction
- show concise success feedback

## Transaction history

If implemented:

- date/time
- product
- type
- quantity

Use tabs only if they materially simplify the page.

---

# 8. Employee Management / Nhân viên

## Primary user

Manager/Admin.

## Goal

Manage employee accounts and operational status.

## Header

`Nhân viên`

Primary action:

`Thêm nhân viên`

## DataGrid columns

- FullName
- Email
- Phone
- Role
- IsActive/status
- face enrollment status if useful
- actions

Do not display password/hash values.

## Status

Use text:

- Đang hoạt động
- Đã vô hiệu hóa

## Row actions

- Edit
- Enrollment khuôn mặt
- Activate/deactivate

Destructive/account actions require confirmation.

---

# 9. Face Enrollment / Đăng ký khuôn mặt

## Primary user

Manager/Admin assisting employee.

## Goal

Collect 15–20 usable face samples for the selected employee.

## Layout

```text
Header
Employee identity

[ Camera preview                 ][ Enrollment status ]
[                                ][ 7 / 20 samples     ]
[                                ][ guidance           ]
[                                ][ quality/status     ]
```

## Guidance

Display concise Vietnamese instructions:

- Nhìn thẳng camera
- Giữ khuôn mặt trong khung
- Xoay nhẹ sang trái/phải khi được yêu cầu
- Đảm bảo đủ ánh sáng

## Progress

Show:

- captured sample count
- target sample count
- current detection status

Do not rely on green/red border only.

## Failure handling

If camera cannot open:

- explain failure
- provide retry action
- do not block unrelated employee management

---

# 10. Attendance / Chấm công

## Primary user

Employee.

## Goal

Identify employee and record check-in/check-out reliably.

## Important business behavior

The project supports QR/employee-code identification with face verification/fallback behavior.

The UI must not make face recognition the only possible route.

## Layout

Recommended low-density screen:

```text
Header
"Chấm công"

[ Employee code / QR area ]

[ Camera preview ]

[ Current verification status ]

[ Primary check-in/check-out action if needed ]

Recent confirmation/status
```

## Verification states

### Idle

`Quét mã QR hoặc nhập mã nhân viên để bắt đầu.`

### Employee identified

Show:

- employee name
- employee code
- next face verification step

### Face checking

Show progress:

`Đang xác thực khuôn mặt...`

### Success

Show:

- employee name
- action: check-in/check-out
- exact time
- clear success text

Example:

`Chấm công thành công lúc 08:03.`

### Face verification failed

Explain:

- verification failed
- retry count if implemented
- fallback route

Example:

`Không thể xác thực khuôn mặt. Vui lòng thử lại hoặc tiếp tục bằng mã nhân viên theo quy trình dự phòng.`

### Camera unavailable

Show:

- camera unavailable
- retry
- fallback method

Never dead-end the user.

---

# 11. Attendance Management / Lịch sử chấm công

## Primary user

Manager/Admin.

## Goal

Review attendance records.

## Toolbar

- date/date range
- employee search
- status filter

## DataGrid columns

- Employee
- Date
- CheckInTime
- CheckOutTime
- Status

Status examples:

- Đúng giờ
- Đi trễ
- Chưa checkout

If the underlying data model does not support a status, do not invent persisted fields.

---

# 12. Reports / Báo cáo

## Primary user

Manager/Admin.

## Goal

Analyze actual sales data.

## Structure

```text
Header
"Báo cáo"

Filter row
[Date range]
[Optional category]
[Apply]

KPI summary

Main chart

Supporting table
```

## Suggested KPI

- revenue
- number of orders
- average order value
- units sold

Only calculate metrics that data supports.

## Suggested reports

- revenue by date
- top-selling products
- sales by category
- cashier performance only if appropriate and supported

## Chart rules

- no 3D
- no gradient
- no decorative animations
- axis units clearly visible
- chart colors consistent with design system

---

# 13. AI Chat / Trợ lý AI

## Primary user

Manager/Admin.

## Goal

Ask business questions based on real sales data.

Example questions:

- `Tuần này bán chạy nhất mặt hàng gì?`
- `Doanh thu 7 ngày gần nhất thay đổi như thế nào?`
- `Những sản phẩm nào sắp hết hàng?`

## Layout

```text
Header
"Trợ lý AI"
short explanation

Suggested prompts

Conversation history

Composer
[Nhập câu hỏi..............................][Gửi]
```

## Visual rules

Use the standard SmartPOS palette.

Do not create a purple or futuristic AI theme.

## Suggested prompt chips

Compact, optional:

- `Top sản phẩm tuần này`
- `Doanh thu 7 ngày`
- `Sản phẩm sắp hết`
- `So sánh doanh thu`

Do not use giant rounded pills.

## Assistant message

Prefer clean message blocks.

When answer contains numbers, make important figures readable.

If possible, indicate period/context used by the query.

## Loading

Show:

`Đang phân tích dữ liệu...`

Disable duplicate send while request is active when appropriate.

## Error

Example:

`Không thể lấy dữ liệu phân tích lúc này. Vui lòng thử lại.`

Do not fabricate an answer when database/API context is unavailable.

---

# 14. Common dialogs

## Delete confirmation

Title:

`Xác nhận xóa`

Message:

Explain exactly what will be affected.

Actions:

- Hủy
- Xóa

Danger action uses Danger style.

## Deactivate employee

Prefer wording around account/status rather than permanent delete where possible.

## Unsaved changes

Only show if the actual screen supports editing with unsaved state.

---

# 15. Search and filter behavior

Search fields should normally:

- have visible search icon
- use concise placeholder
- avoid immediate expensive DB calls on every keystroke unless debounced
- show clear empty results

Filter controls should be grouped near the data they affect.

Avoid filter panels that occupy too much permanent space.

---

# 16. Notifications / Feedback

Prefer lightweight inline or toast-like feedback for routine success events.

Examples:

- `Đã cập nhật sản phẩm.`
- `Nhập kho thành công.`
- `Thanh toán thành công.`

Use modal dialogs only when acknowledgement is necessary.

---

# 17. Empty states

## Products

`Chưa có sản phẩm nào.`

Action:

`Thêm sản phẩm`

## Reports

`Chưa có dữ liệu trong khoảng thời gian đã chọn.`

## AI chat

Provide suggested questions rather than a decorative illustration.

## Attendance history

`Không có bản ghi chấm công phù hợp với bộ lọc.`

---

# 18. Loading states

Any operation involving:

- database query
- camera initialization
- barcode camera scan
- AI API
- report generation

should provide visible feedback if it can take noticeable time.

Do not freeze the UI thread.

---

# 19. Error states

Errors should answer:

1. What failed?
2. Was the user's data saved?
3. What can they do next?

Bad:

`Error occurred.`

Better:

`Không thể lưu sản phẩm vì mã vạch đã tồn tại. Vui lòng kiểm tra lại mã vạch.`

---

# 20. Keyboard workflow

POS should prioritize keyboard/scanner operation.

Recommended behaviors when practical:

- barcode input receives focus on page entry
- Enter submits manual barcode
- focus returns after successful add
- checkout can have a deliberate shortcut later if documented
- Escape closes non-destructive dialogs where appropriate

Do not add shortcuts that conflict with text entry.

---

# 21. Screen consistency checklist

Before completing a new screen:

- Does it follow the application shell?
- Is the primary task obvious?
- Is the primary action obvious?
- Does it use existing styles?
- Does it avoid unnecessary cards?
- Does it avoid gradients/shadows/oversized radius?
- Does it support loading/error/empty states?
- Is data aligned correctly?
- Is the Vietnamese copy concise?
- Is it usable at 1366×768?
- Does it preserve MVVM?
