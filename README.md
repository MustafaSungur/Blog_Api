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
#### Auth Endpoints
| HTTP Method	  | Endpoint	  |  Endpoint	 |
| ------------ | ------------ | ------------ |
| POST  | /Login	  |  Authenticates a user by verifying the username and password. |
| GET  | /Logout	  | Logs out the authenticated user.  |


### CommentLikes  Endpoints
| HTTP Method	  | Endpoint	  |  Endpoint	 |
| ------------ | ------------ | ------------ |
| POST  | /api/CommentLikes	  |  Allows an authenticated user to like a comment. |
| DELETE  |  /api/CommentLikes/{commentId}	 |  Allows an authenticated user to remove their like from a specific comment. |
|  GET |  /api/CommentLikes/{commentId}	 |  Retrieves the like details of an authenticated user for a specific comment. |
| GET  | /api/CommentLikes/user/{userId}	  | Retrieves all comment likes by a specific user. Accessible only to the user or an admin. |


### Comments Endpoints

| HTTP Method	  | Endpoint	  |  Endpoint	 |
| ------------ | ------------ | ------------ |
| GET  | /api/Comments	  |  Retrieves all top-level comments (those without a parent comment) along with their nested replies and likes. |
| GET  |  /api/Comments/{id}	 |  Retrieves a specific comment by its ID, including replies and likes. |
| POST  | /api/Comments	  | Allows an authenticated user to create a new comment or reply to an existing comment.  |
|  DELETE | /api/Comments/{id}	  | Allows an authenticated user or an admin to delete a comment and its associated replies and likes.  |
| GET  |  /api/Comments/user/{userId}	 |  Retrieves all comments made by a specific user. Accessible only to the user or an admin. |


### PostLikes Endpoints
| HTTP Method	  | Endpoint	  |  Endpoint	 |
| ------------ | ------------ | ------------ |
|  GET | /api/PostLikes	  | Retrieves all post likes, including the associated post and user information.  |
|  GET | /api/PostLikes/{postId}	  | Retrieves the like details for a specific post by the currently authenticated user.  |
| POST  | /api/PostLikes	  |  Allows an authenticated user to like a post. |
| DELETE  |  /api/PostLikes	 | Allows an authenticated user to remove their like from a specific post.  |
|GET | /api/PostLikes/user/{userId}	  |  Retrieves all post likes by a specific user. |

### Posts Endpoints
| HTTP Method	  | Endpoint	  |  Endpoint	 |
| ------------ | ------------ | ------------ |
| GET  |  /api/Posts	 |  Retrieves all posts, including their comments, replies, and likes. |
|  GET |  /api/Posts/{id}	 | Retrieves a specific post by its ID, including its comments, replies, and likes.  |
| PUT  |  /api/Posts/{id}	 |  Updates a specific post. Only the post owner can perform this action. |
| POST  | /api/Posts	  | Creates a new post.  |
|  DELETE | /api/Posts/{id}	  |  Deletes a specific post, including its comments and likes. Only the post owner or an admin can perform this action. |
|  GET | /api/Posts/User/{id}	  | Retrieves all posts by a specific user, including their comments, replies, and likes.  |

### User Endpoints
| HTTP Method	  | Endpoint	  |  Endpoint	 |
| ------------ | ------------ | ------------ |
| GET  | /api/User	  |  Retrieves all users. This action is restricted to admin users. |
|POST   | /api/User	  | Creates a new user.  |
|  GET | /api/User	  |Retrieves details of a specific user by ID. Accessible to the user themselves or an admin.   |
|  DELETE | /api/User	  | Deactivates a user by setting their status to false. Accessible to the user themselves or an admin.|
|  POST |  /api/User/ForgetPassword	 | Generates a password reset token for the specified username, except for the "Admin" user.  |
|  POST |  /api/User/ForgetPassword	 |  Resets the password using a token, except for the "Admin" user. |
|  PUT | /api/User/{id}	 | Updates a user. Accessible by admin users or users who are updating their own profile. |

## Running
To run the project, follow these steps:

1. Clone the project:
    
    	git clone https://github.com/MustafaSungur/Blog_Api.git
    
1. Open the solution in Visual Studio.
1. Update the connection string in appsettings.json to point to your local SQL Server instance.

### Developer Guide for Testing the Project
To test the BlogAPI project, follow these steps:

### **Step 1: Register User**

- Endpoint: POST /api/User
- JSON Body:

```
	{
	  "firstName": "John",
	  "lastName": "Doe",
	  "birthDate": "1990-05-15T00:00:00Z",
	  "email": "johndoe@example.com",
	  "userName": "johndoe123",
	  "photoUrl": "",
	  "password": "Test123!",
	  "confirmPassword": "Test123!"
	}
```
### **Step 2: Login as Author:**
- Endpoint: POST /Login
- JSON Body:
  
		{
			"userName": "johndoe123",
			"password": "Test123!"
		}

### Step 3: Create and Interact with Posts
#### **1.  Create Post**
- Endpoint: POST /api/Posts
- JSON Body:
  
		{
		  "title": "My First Post",
		  "content": "This is the content of my first post."
		}

#### **2. Comment on a Post:**
- Endpoint: POST /api/Comments
- JSON Body:

			{
				"content": "Great post!",
				"postId": 1
			}

#### **3. Like a Post:**
Endpoint: POST /api/PostLikes/{postId}
URL Parameter: postId = 1

#### **4. Like a Comment:**
Endpoint: POST /api/CommentLikes/{commentId}
URL Parameter: commentId = 1

## Conclusion
The BlogAPI project provides a robust and scalable solution for managing blog resources. With its comprehensive set of features, modern technology stack, and detailed documentation, it is well-suited for real-world blog environments.

## Acknowledgements
This project was developed as part of the backend program at [Softito Yazılım - Bilişim Akademisi](https://softito.com.tr/ "Softito Yazılım - Bilişim Akademisi"). Special thanks to the instructors and peers who provided valuable feedback and support throughout the development process.
