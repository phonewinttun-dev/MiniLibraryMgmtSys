# 📚 Mini Library Management System – Module Listing

This document outlines the **feature modules** for the Mini Library Management System, enhanced with a **Dashboard Summary module** and **AI-generated insights** to support better decision-making and system observability.

---

## Core Modules

### 1. User Management

* Register new users (Admin, Librarian, Member)
* Login and Authentication
* Role-based Access Control (RBAC)
* Manage user profile information
* Soft delete users (`DeleteFlag`)
* Audit tracking (`CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`)

---

### 2. Book Management

* Add new books
* Edit book details
* Soft delete books
* Manage book availability status
* Categorize books by genre
* Search and filter books by:

  * Title
  * Author
  * Genre
* Track book creation and updates (audit fields)

---

### 3. Borrow Management

* Borrow a book (only if available)
* Return a borrowed book
* Automatically update book availability on borrow/return
* Track borrowing history per user
* Track borrowing status:
  * Borrowed
  * Returned
  * Overdue (computed, not stored)
* Prevent invalid state transitions (double borrow / double return)

---

## 📊 Dashboard & Insights Modules

### 4. Dashboard Summary

Provides **system-level visibility** using aggregated, read-only data.

#### Features

* Total number of books
* Available books count
* Borrowed books count
* Total registered users
* Active borrow count
* Overdue borrow count (rule-based, time-derived)

#### Purpose

* System monitoring
* Administrative overview
* Entry point for analytics and insights

---

### 5. AI-Generated Dashboard Insights

An **AI-assisted interpretation layer** that transforms dashboard metrics into human-readable insights.

#### Features

* AI-generated textual summary of dashboard statistics
* Trend explanation (e.g. borrowing increases/decreases)
* Overdue risk explanations

#### Characteristics

* Read-only
* No direct database access by AI
* AI consumes pre-aggregated, backend-computed metrics only
* Designed as an optional, replaceable service

#### Example Output

> "Borrowing activity increased this week, with Fiction being the most active genre. 5 books are approaching overdue status and may require attention."

---

## ⚙️ Supporting Modules

### 6. Authentication & Authorization

* Secure login and token-based authentication
* Role-based permissions enforcement
* Endpoint protection based on user roles

---

## 🗄️ Database Overview

| Table               | Description                                   |
| ------------------- | --------------------------------------------- |
| `tbl_users`         | Stores user accounts, roles, and credentials  |
| `tbl_books`         | Contains book details and availability status |
| `tbl_borrowedBooks` | Tracks borrowing records and status           |

---

**Author:** Phone Wint Tun\
**Database:** MSSQL\
**Tech Stack:** ASP.NET Core 8 · EF Core 8 · MSSQL\
**Project Goal:** Master backend development with clean architecture, scalability, and responsible AI integration

