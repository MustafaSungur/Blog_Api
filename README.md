# BlogAPI Project
## Introduction
Welcome to the BlogAPI project! This project is a comprehensive blog management system designed to help authors and administrators manage blog posts, comments, and user interactions effectively. The project includes features such as post management, comment management, user authentication, and more. It is built with a focus on scalability, security, and ease of use.

## Project Overview
The BlogAPI project is a RESTful API built using ASP.NET Core. It provides endpoints for managing blog resources such as posts, comments, tags, authors, and categories. The project also includes features for handling likes, bookmarks, and user roles and authentication. The API is designed to be used by different roles such as authors and administrators, each with specific permissions and capabilities.

## Technologies Used
This project utilizes a variety of technologies to ensure a robust and efficient system. Below is a detailed explanation of the key technologies used:

### ASP.NET Core
ASP.NET Core is a cross-platform, high-performance framework for building modern, cloud-based, internet-connected applications. It is used to build the RESTful API that powers the BlogAPI project.

### Entity Framework Core
Entity Framework Core (EF Core) is an open-source ORM (Object-Relational Mapper) for .NET. It allows developers to work with a database using .NET objects, eliminating the need for most data-access code.

### Identity Framework
The ASP.NET Core Identity framework is used to manage users, passwords, roles, and claims. It provides a complete, customizable authentication and authorization system. It integrates seamlessly with EF Core to handle the storage and retrieval of user-related data.

### SQL Server
SQL Server is a relational database management system developed by Microsoft. It is used as the database for the BlogAPI project to store all the blog data.

### Swagger
Swagger is an open-source tool for documenting APIs. It provides a user-friendly interface to explore and test API endpoints. The BlogAPI project includes Swagger for API documentation and testing.

### Project Structure
The project is organized into several folders and files to maintain a clean and manageable structure. Here is an overview of the project structure:

    BlogAPI/ 
      ├── Controllers/ 
      ├── Data/ 
      ├── DTOs 
      ├── Models 
      ├── Migrations/ 
      ├── BlogAPI/ 

#### Key Folders and Files
* **Controllers/:** Contains the API controllers that handle HTTP requests.
* **DTOs/:** Contains Data Transfer Objects used for data encapsulation.
* **Data/:** Contains the database context and migration files.
* **models/:** Contains the entity models representing the database schema.

### Model Classes
The model classes represent the entities in the database. Here are some key model classes used in the project:

#### ApplicationUser
Represents a user in the system with properties like Id, Name, Email, and more.

### Endpoints
The BlogAPI provides various endpoints for managing blog resources. Here is a table of the key endpoints and their purposes:
| HTTP Method	  | Endpoint	  |  Endpoint	 |
| ------------ | ------------ | ------------ |
|   |   |   |
|   |   |   |
|   |   |   |
|   |   |   |
