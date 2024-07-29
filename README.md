# Microservices Architecture

This repository contains a set of microservices that communicate with each other via RabbitMQ and interact with a PostgreSQL database. The microservices are built using .NET Core Web API.

## Overview

- **OrderService**: Handles orders and communicates with the `UserService` via RabbitMQ.
- **UserService**: Manages user data and listens to messages from RabbitMQ.
- **NotificationService**: Sends notifications based on events received from RabbitMQ.

## Prerequisites

- **Docker**: Ensure Docker is installed and functional. The application relies on Docker to run all services and pull images from online repositories.

## Setup and Running the Application

From the root directory containing `docker-compose.yml`, run:

```bash
docker-compose up --build -d
```

To completely remove all the resources created, run: (-v will remove the Volume, clearing out persistent storage for PostgreSQL)
```bash
docker-compose down -v
```

## Wait for Services to be Ready

The application needs approximately 30 to 40 seconds to be fully operational after the containers are started. This is because services such as RabbitMQ and PostgreSQL need some time to initialize and become ready.

## Exposed Ports

All services are accessible on localhost (ports forwarded by docker-compose networking)
- ``UserService``: Port 8080
- ``OrderService``: Port 8081

## Output and Monitoring

All the services use Loggers to log details out to the console. This means you can view all the activities, errors and mock outputs in the container logs. No logs are persistet to files.

**NotificationService** outputs mock notifications to the console when events are published that would send notifications to end users.

## Postman Collection

A Postman Collection is available (`Linkup.postman_collection.json`) with all CRUD operations included. It provides Body JSON examples where appropriate. You can use it to test and interact with the microservices.

## Microservices Architecture

- **Microservices**: All microservices are .NET Core Web APIs.
- **Communication**: Microservices communicate via RabbitMQ.
- **Persistent Storage**: Communication with the PostgreSQL database is done via Entity Framework Core.
- **Message Handling**:
  - **UserService** and **NotificationService** have permanent RabbitMQ queues that they continually listen on.
  - **OrderService** publishes messages to `UserService` to get user details, defining a temporary reply channel with a unique GUID. This channel has exclusive access and auto-closure to enable asynchronous execution while waiting for `UserService` to respond with the necessary data. This is a very simple request-reply pattern implementation

## Database and Entity Management

- **PostgreSQL**: The `Order` table references the `User` table with a foreign key to enforce referential integrity.
- **Entity Definitions**: Each microservice has unique entity definitions to ensure decoupling.
- **Entity Framework Core**: Used for database interactions in each microservice.
- **Automapper**: Simplifies mapping between entities and DTOs.

## Future Improvements

Consider creating a separate service to handle all database migrations and repository interactions to centralize database management.

## Troubleshooting

- Ensure Docker is running properly and has access to the internet for pulling images.
- Verify that RabbitMQ and PostgreSQL are correctly initialized and ready before making API requests.

## Experimentation from my part
Some of the code is done in different approaches. For example, in OrderService I declared the RabbitMQ service as a scoped service, and in the permanent listeners (User and Order Services), I declared it as a Singleton instances. This falls in line with my understanding of their intended use cases, but might be undesireable due to inconsistency between microservice sources.