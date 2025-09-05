# Concert Ticket Management System – Design Document

## 1. Overview
The Concert Ticket Management System is a RESTful .NET Web API designed to manage concert events, ticket reservations, and venue capacity efficiently. 
The system provides core functionality to manage events, tickets, and reservations while ensuring data consistency and preventing overbooking.

## 2. Core Requirements

### 1.Event Management

Create, update, and delete concert events.

Set ticket types and pricing.

Manage available capacity per event.

Store basic event details: date, venue, description.

### 2.Ticket Reservations and Sales

Reserve tickets for a limited time window.

Purchase tickets leveraging an existing payment system.

Cancel reservations.

View ticket availability in real-time.

### 3.Venue Capacity Management

Track venue capacity and ticket allocation.

Prevent overbooking by enforcing capacity constraints.

Update availability dynamically upon reservations or cancellations.

## 3. Domain Model
### 3.1 Entities

#### Event

EventId (Guid)

Name (string)

Description (string)

Venue (string)

EventDate (DateTime)

TotalCapacity (int)

TicketTypes (List<TicketType>)

#### TicketType

TicketTypeId (Guid)

Name (string, e.g., VIP, General)

Price (decimal)

Capacity (int)

EventId (Guid)

#### Ticket

TicketId (Guid)

EventId (Guid)

TicketTypeId (Guid)

Status (enum: Reserved, Purchased, Cancelled)

ReservationExpiry (DateTime, optional)

PurchaseDate (DateTime, optional)

CustomerEmail (string)

## 4. API Design

### 4.1 Event Management APIs

| Endpoint             | Method | Description                  |
|---------------------|--------|------------------------------|
| /api/events          | GET    | Get all events               |
| /api/events/{id}     | GET    | Get event details by ID      |
| /api/events          | POST   | Create a new event           |
| /api/events/{id}     | PUT    | Update an existing event     |
| /api/events/{id}     | DELETE | Delete an event              |

### 4.2 Ticket Management APIs

| Endpoint                          | Method | Description                          |
|----------------------------------|--------|--------------------------------------|
| /api/tickets/reserve              | POST   | Reserve tickets for an event         |
| /api/tickets/purchase/{ticketId}             | POST   | Purchase tickets using payment system|
| /api/tickets/cancel/{ticketId}   | POST   | Cancel a reserved ticket             |
| /api/tickets/availability/{eventId} | GET    | Check ticket availability for an event|


## 5. Reservation Logic

**Reservation Window:** Tickets are held for a limited time (e.g., 15 minutes).

**Concurrency Control:** Use optimistic locking or row-level locking to prevent overbooking.

**Availability Check:** Before reserving/purchasing, check remaining capacity for the event and ticket type.

**Expiration Handling:** A background service or scheduled job will release expired reservations automatically. (Currently not implemented but we could extend it. Right now its in reserveTicket that we check and release it.)

## 6. Database Design (SQL Server / SQLite)

### 6.1 Tables

#### Events

| Column       | Type           | Notes           |
|--------------|----------------|----------------|
| EventId      | UNIQUEIDENTIFIER | Primary Key   |
| Name         | NVARCHAR       | Not null       |
| Description  | NVARCHAR       | Nullable       |
| Venue        | NVARCHAR       | Not null       |
| EventDate    | DATETIME       | Not null       |

#### TicketTypes

| Column        | Type           | Notes                  |
|---------------|----------------|-----------------------|
| TicketTypeId  | UNIQUEIDENTIFIER | Primary Key          |
| EventId       | UNIQUEIDENTIFIER | Foreign Key to Events|
| Name          | NVARCHAR       | Not null              |
| Price         | DECIMAL        | Not null              |
| Capacity      | INT            | Not null              |
| BookedCount   | INT            | Not null              |

#### Tickets

| Column            | Type              | Notes                             |
|------------------|------------------|----------------------------------|
| TicketId          | UNIQUEIDENTIFIER | Primary Key                      |
| EventId           | UNIQUEIDENTIFIER | Foreign Key to Events            |
| TicketTypeId      | UNIQUEIDENTIFIER | Foreign Key to TicketTypes       |
| Status            | INT / ENUM       | Reserved, Purchased, Cancelled   |
| ReservationExpiry | DATETIME         | Nullable                         |
| PurchaseDate      | DATETIME         | Nullable                         |
| CustomerEmail     | NVARCHAR         | Nullable                         |

## 7. System Architecture

Backend: .NET 8 Web API

Database: SQL Server or SQLite (lightweight option for local development)

Authentication/Authorization: JWT / API key (Not implemented but can extend)

Payment Integration: Assume existing payment system (stub service for API testing)

Concurrency & Reservation Expiry: Background service or in-memory cache with scheduled cleanup

## 8. Future Improvements

### Security

Validate inputs to prevent SQL Injection

Optional role-based authorization for admin endpoints (not implemented yet).

AuthN for ticketing endpoints

### Scalability

1. API Throttling to prevent abuse
2. For large-scale deployments, caching ticket availability
3. AzureSQL or SQL Server for production use.
4. Maintaining a single count per ticket type might cause issues in high concurrency scenarios. We could possibly add sections or zones in the venue to better manage capacity and reservations.
5. A background thread could clean up and archive past events and tickets to keep the database size manageable.
6. The current implementation for capacity management uses 'ConcurrencyCheck'. Under high TPS scenarios, this might lead to frequent exceptions. Using guarded transactions will mitigate this issue.

### Code maintainability
1. API model and DB model classes should be ideally separate as per Single Responsibility Principal

### Features
1. Multiple reservations in same request.
2. Dedicated seat numbers for tickets. 

### Operations
1. Logging and monitoring for API usage and errors.

## 9. Example Use Case Flows
### 9.1 Reserving a Ticket

User selects event and ticket type.

API checks availability.

Tickets are marked as Reserved with expiry time.

User completes purchase or reservation expires automatically.

### 9.2 Purchasing a Ticket

User selects reserved tickets.

API calls payment service.

Upon successful payment, ticket status is updated to Purchased.

Capacity is decremented for the event and ticket type.

## 10. Tech Stack

Backend: .NET 8 Web API

Database: SQLite (for simplicity) but better to use SQL Server for production

ORM: Entity Framework Core

Unit Testing: xUnit

Version Control: GitHub

API Testing: Postman 

## 11. Key design decisions

### Database type (Relational vs NoSQL)

In a reservation system, relational databases are preferred due to the structured nature of the data and the need for complex queries and relationships.