# 📚 Mini Library Management System - Module Listing

This document outlines the key modules and features for the **Mini
Library Management System**, designed to manage users, books, and
borrowing activities efficiently.

------------------------------------------------------------------------

## Core Modules

### 1. User Management

-   Register new users (Admin, Librarian, Member)
-   Login and Authentication
-   Role-based Access Control (RBAC)
-   Manage profile information

### 2. Book Management

-   Add, edit, and delete books
-   Manage book availability status
-   Categorize books by genre
-   Search and filter books by name, author, or genre
-   Track book creation and updates (createdBy, updatedBy)

### 3. Borrow Management

-   Borrow a book (only if available)
-   Return a borrowed book
-   Update book status automatically when borrowed/returned
-   Track borrowing history of users
-   Borrowing status types:
    -   `borrowed`
    -   `returned`
    -   `overdue`

------------------------------------------------------------------------

## ⚙️ Supporting Modules

### 4. Authentication & Authorization

-   Role-based permissions (Admin, Librarian, Member)


## 🗄️ Database Schema Overview

  -----------------------------------------------------------------------
  Table                     Description
  ------------------------- ---------------------------------------------
  **users**                 Stores user accounts, roles, and credentials

  **books**                 Contains book details and availability status

  **borrowed_books**                 Links users with borrowed books and tracks borrowing status

------------------------------------------------------------------------

**Author:** Phone Wint Tun\
**Database:** MSSQL\
**Tech Stack:** .NET 8 /  MSSQL (Database)
