# Concert Ticket Management System

Design Document is located in /docs/DesignDoc.md

## (.NET 8 + SQLite Web API)

This is a simple Web API built with **.NET 8** and **Entity Framework Core** using **SQLite** as the database.  
It demonstrates a minimal CRUD setup with EF Core, SQLite, and ASP.NET Core Web API.

---

## Prerequisites

Before running this project locally, make sure you have:

- [Install .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with ASP.NET workload) **or** [Visual Studio Code](https://code.visualstudio.com/)
- (Optional) [DB Browser for SQLite](https://sqlitebrowser.org/) if you want to explore the SQLite database file manually.
- Install jq (for testing script: test-apis.sh) - https://stedolan.github.io/jq/download/

---

## Getting Started

### 1. Clone the Repository

git clone https://github.com/jahnavi24/ConcertTicketManagement.git <br>
cd ConcertTicketManagement

### 2. Restore Dependencies

dotnet restore

### 3. Build the Project

dotnet build

### 4. Setup the Database

dotnet tool install --global dotnet-ef   # only required once<br>
dotnet ef database update

## Run the project locally:

dotnet run

The API will be available at:

- http://localhost:5159/api/events
- https://localhost:7170/api/events

## Testing the API (Using provided script)

```
chmod +x test-apis.sh
./test-apis.sh
```

## Testing the API (Manual)

### 1. Event Management APIs

| Endpoint             | Method | Description                  |
|---------------------|--------|------------------------------|
| /api/events          | GET    | Get all events               |
| /api/events/{id}     | GET    | Get event details by ID      |
| /api/events          | POST   | Create a new event           |
| /api/events/{id}     | PUT    | Update an existing event     |
| /api/events/{id}     | DELETE | Delete an event              |

### 2. Ticket Management APIs

| Endpoint                          | Method | Description                          |
|----------------------------------|--------|--------------------------------------|
| /api/tickets/reserve              | POST   | Reserve tickets for an event         |
| /api/tickets/purchase             | POST   | Purchase tickets using payment system|
| /api/tickets/cancel   | POST   | Cancel a reserved ticket             |
| /api/tickets/availability/{eventId} | GET    | Check ticket availability for an event|


### 2. Using Swagger

http://localhost:5159/swagger/index.html
