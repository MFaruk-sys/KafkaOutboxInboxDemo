# KafkaOutboxDemo - Full automated

This demo uses:
- OrderService -> PostgreSQL (Outbox table)
- Debezium (Postgres connector) -> Kafka
- PaymentService -> SQL Server (consumes Kafka topic) with Inbox pattern
- Auto-registers Debezium connector at startup
- Services auto-create databases/tables with EnsureCreated

Run:
```bash
docker compose up --build
```

Postman collection included: `postman/KafkaOutboxDemo.postman_collection.json`

How it works:
1. POST /orders to OrderService (http://localhost:5001/orders) → writes order + outbox row into Postgres.
2. Debezium picks up outbox row and publishes to Kafka `dbserver1.public.outbox_messages`.
3. PaymentService subscribes to that topic and processes messages idempotently (stores processed ids into SQL Server `inbox_messages` table).

The Debezium connector is auto-registered by the `connector-registrar` service.
