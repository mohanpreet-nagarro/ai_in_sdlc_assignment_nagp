# Instructions for this repository

- Everything under `ops/` describes a PLANNED production deployment (Azure SQL, Service Bus, PayEx).
  It is not the current code. Never describe ops/ content as what the system does today.
- When describing the architecture, cite a file and line for every claim,
  and list anything you inferred separately.
- Business rules belong in `InvoiceApp.Domain`; it must not reference HTTP, SQL, ASP.NET or Azure code.
- Run tests with: `dotnet test backend/InvoiceApp.sln`
- If you are unsure of a fact (e.g. what the PayEx API supports), say so instead of assuming.