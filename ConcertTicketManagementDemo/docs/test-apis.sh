#!/bin/bash

# Base URL of your API
BASE_URL="https://localhost:7170/api"

# Function to print JSON nicely if jq exists
function print_json() {
    if command -v jq &> /dev/null; then
        echo "$1" | jq .
    else
        echo "$1"
    fi
}

# ---- CREATE EVENT ----
echo -e "\n=== Creating Event ==="
CREATE_EVENT_RESPONSE=$(curl -s -X POST "$BASE_URL/events" \
-H "Content-Type: application/json" \
-d '{
    "Name": "Rock Concert",
    "Description": "Live rock concert",
    "Venue": "Madison Square Garden",
    "EventDate": "2025-10-15T19:00:00Z",
    "TotalCapacity": 100,
    "TicketTypes": [
        {"Name":"VIP","Price":200,"Capacity":10},
        {"Name":"Regular","Price":100,"Capacity":90}
    ]
}')
print_json "$CREATE_EVENT_RESPONSE"

EVENT_ID=$(echo "$CREATE_EVENT_RESPONSE" | jq -r '.eventId')
VIP_TICKET_TYPE_ID=$(echo "$CREATE_EVENT_RESPONSE" | jq -r '.ticketTypes[0].ticketTypeId')

# ---- UPDATE EVENT ----
echo -e "\n=== Updating Event ==="
UPDATE_RESPONSE=$(curl -s -X PUT "$BASE_URL/events/$EVENT_ID" \
-H "Content-Type: application/json" \
-d "{
    \"Name\": \"Rock Concert Updated\",
    \"Description\": \"Updated description\",
    \"Venue\": \"Madison Square Garden\",
    \"EventDate\": \"2025-10-15T19:00:00Z\",
    \"TotalCapacity\": 120,
    \"TicketTypes\": [
        {\"TicketTypeId\": \"$VIP_TICKET_TYPE_ID\", \"Price\": 220, \"Capacity\": 15}
    ]
}")
echo "Event updated."

# ---- GET ALL EVENTS ----
echo -e "\n=== Fetching All Events ==="
ALL_EVENTS=$(curl -s "$BASE_URL/events")
print_json "$ALL_EVENTS"

# ---- GET SINGLE EVENT ----
echo -e "\n=== Fetching Single Event ==="
SINGLE_EVENT=$(curl -s "$BASE_URL/events/$EVENT_ID")
print_json "$SINGLE_EVENT"

# ---- RESERVE TICKET ----
echo -e "\n=== Reserving Ticket ==="
RESERVE_RESPONSE=$(curl -s -X POST "$BASE_URL/tickets/reserve" \
-H "Content-Type: application/json" \
-d "{
    \"TicketTypeId\": \"$VIP_TICKET_TYPE_ID\",
    \"CustomerEmail\": \"test@example.com\"
}")
print_json "$RESERVE_RESPONSE"
TICKET_ID=$(echo "$RESERVE_RESPONSE" | jq -r '.ticketId')

# ---- PURCHASE TICKET ----
echo -e "\n=== Purchasing Ticket ==="
PURCHASE_RESPONSE=$(curl -s -X POST "$BASE_URL/tickets/purchase" \
-H "Content-Type: application/json" \
-d "{
    \"TicketId\": \"$TICKET_ID\",
    \"CustomerEmail\": \"test@example.com\",
    \"PaymentTransactionId\": \"txn_123456\"
}")
print_json "$PURCHASE_RESPONSE"

# ---- CANCEL TICKET ----
echo -e "\n=== Cancelling Ticket ==="
CANCEL_RESPONSE=$(curl -s -X POST "$BASE_URL/tickets/cancel" \
-H "Content-Type: application/json" \
-d "{
    \"TicketId\": \"$TICKET_ID\",
    \"CustomerEmail\": \"test@example.com\"
}")
print_json "$CANCEL_RESPONSE"

# ---- CHECK AVAILABILITY ----
echo -e "\n=== Checking Ticket Availability ==="
AVAILABILITY=$(curl -s "$BASE_URL/tickets/availability/$EVENT_ID")
print_json "$AVAILABILITY"

# ---- DELETE EVENT ----
echo -e "\n=== Deleting Event ==="
DELETE_RESPONSE=$(curl -s -X DELETE "$BASE_URL/events/$EVENT_ID")
echo "Event deleted."
