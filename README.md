# Minimal API Task Manager

# Task Manager API

This project is a Task Manager API built using ASP.NET Minimal API. It follows RESTful principles and provides CRUD operations for managing tasks. The API is optimized for low latency and includes features like pagination and caching using Microsoft's inbuilt caching services.

## Features

- **Entity Framework ORM**: Use's EF to manage database queries.
- **RESTful API**: Adheres to REST principles for resource management.
- **CRUD Operations**: Create, Read, Update, and Delete tasks.
- **JSON Handling**: Efficient serialization and deserialization of JSON data.
- **Role-Based Access Control**: Uses JWT tokens to manage user roles (Admin and User).
- **Pagination**: Supports pagination for efficient retrieval of larger data sets to minimize load.
- **Caching**: Utilizes Microsoft's inbuilt caching for performance optimization.
- **MySQL DB**: Utilizse's MySQL with EntityFramework using Pomelo

## Installation
# Clone
```bash
git clone https://github.com/02scanks/Task-Managment-Minimal-API.git
```

# Setup MYSQL Database
Link up the project with your newly created database in whatever way you prefer and link it up with the "AppDbContext" dependency injection in program.cs.


# Run Inital Migration and Update
Run an inital migration with the following
```bash
add-migration InitialMigration
```
And then update the database with your inital migration to create the table with the sets inside of the AppDbContext.cs file with the following command
```bash
update-database
```

## Complete
Then from here on you should be good to run the program and contact all the end points with Postman and link it up to a front end if you so desire!
