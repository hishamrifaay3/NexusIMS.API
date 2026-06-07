# NexusIMS - Advanced Inventory & Sales Management (ERP) System

NexusIMS is an Enterprise-Grade Inventory, Procurement, and Sales Management Web API built with **.NET Core**. The system is specifically engineered to handle complex retail and warehouse logistics, mimicking real-world ERP environments. It places a heavy emphasis on data integrity, anti-fraud auditing, and strict security boundaries between multiple physical warehouses.

---

## 🚀 Key Architectural Concepts & Security

### 1. 🔐 Security & Role-Based Authorization
* **JWT Authentication:** Secure token-based access with fully configured user identities and token validation.
* **Granular Role Management:** Pre-seeded default administrative and operational roles: `Admin`, `Manager`, `GeneralAccountant`, `Storekeeper`, and `Cashier`.

### 2. 🛡️ Warehouse Data Isolation (Multi-Tenant Architecture)
* **Strict Physical Boundary Enforcement:** Implemented a continuous custom claims security check. Users with branch-level roles (`Cashier`, `Storekeeper`) are strictly isolated to their assigned warehouse data extracted securely from their JWT token. 
* **Global Oversight:** Centralized management roles (`Admin`, `Manager`, `GeneralAccountant`) retain cross-warehouse overriding visibility for company-wide reporting and stock allocation.

---

## ⚙️ Core Core Modules & Services

### 📦 1. Inventory & Stock Management
* **Dynamic SKU Generation:** Automated backend SKU generation based on category abbreviations and unique product counters (`CAT-PROD-0001`).
* **Real-time Stock Ledger:** Every product movement triggers an atomic ledger entry inside the `StockTransactions` table (`StockIn`, `StockOut`) ensuring a permanent, unalterable historical audit trail.
* **Low-Stock Triggers:** Live threshold-based evaluation alerting management when stock falls beneath safe levels (`MinStockQuantity`).

### 💰 2. Sales & Secure Returns Module (SalesInvoiceService)
* **Automated Financial Calculation:** Real-time cascading calculations of item pricing, tax rates (VAT), global discounts, and gross/net totals directly on the backend.
* **Inventory Interceptors:** Prevents sales transactions if the requested quantities exceed the live stock balance of that specific branch.
* **Anti-Fraud Returns (Credit Notes):** To prevent malicious internal data tampering, direct deletion or modification of validated invoices is strictly prohibited. Modifying transactions generates an official `SalesReturn` document with precise line-item quantity tracking, historical capping checks (preventing returning more than bought), and reverse stock adjustments.

### 🛒 3. Procurement & Suppliers Module (PurchaseInvoiceService)
* **Supplier Directory Management:** Full CRM sub-system to track vendors, business addresses, phone contacts, and official tax registration numbers (`TaxNumber`).
* **Automated Inventory Supply Chains:** Creating a purchase invoice instantly triggers an inventory update, adding the exact incoming product quantities directly into the target branch's `Stocks`.
* **Historical Cost Tracking:** Tracks dynamic cost prices per batch independently from the retail consumer price, feeding clean financial data into the profit margin engines.
* **Procurement Auditing:** Integrates with the global ledger by auto-generating a `StockIn` transaction logged under the executing user's tracking ID.

### 📈 4. Business Intelligence (BI) Dashboard
* Highly optimized LINQ aggregation pipelines that compute real-time analytical metrics over dynamic date ranges (`StartDate` / `EndDate` parameters).
* Computes overall Net Profitability ($\text{Sales} - \text{Cost Purchases} - \text{Returns}$).
* Features top-selling product tracks, supplier engagement volumes, and customer loyalty indicators broken down per warehouse.

---

## 🏗️ Tech Stack

* **Framework:** .NET 10 Web API
* **Database & ORM:** Entity Framework Core with SQL Server
* **Authentication:** ASP.NET Core Identity & JWT Bearer Tokens
* **Architectural Patterns:** Repository-like Service Layer, Data Transfer Objects (DTOs), Fluent API Configurations, Unit of Work (EF Database Transactions).

---

## 🛠️ Challenges Faced & Architectural Wins

### 🔹 Challenge 1: Internal Employee Fraud (Invoice Tampering)
* **The Vulnerability:** In basic architectures, updating or deleting an invoice directly allows bad actors (e.g., a rogue cashier) to pocket cash while artificially correcting stock.
* **The Solution:** Enforced an immutable database rule. Once an invoice status turns `Active`, it cannot be deleted or directly updated. It requires a formal `SalesReturn` receipt that locks the user’s ID, logs the time, and modifies inventory through verified database transactions (`BeginTransactionAsync`).

### 🔹 Challenge 2: EF Core Query Performance in Aggregations
* **The Vulnerability:** Generating dashboard metrics by loading heavy entity objects into memory (`.Include()`) causes memory thrashing and slow API responses when data grows.
* **The Solution:** Avoided unnecessary eagerly loaded `.Include()` paths by utilizing optimized projection queries (`.Select()`). This instructs EF Core to build direct, tailored SQL Joins (`SUM`, `COUNT`, `GROUP BY`) executing completely on the database server side.
