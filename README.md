# E-CommerceSystem

## Overview
E-CommerceSystem is a comprehensive e-commerce platform designed to facilitate online shopping experiences. It provides features for product management, user authentication, order processing, and payment integration.


## Models With Data annotaions and value validation 
*note*
Data annotations and validations are included in the model definitions to ensure data integrity and enforce business rules.
such as:
- [key] => for primary keys,
- [required] => for mandatory fields,
- [maxlength] => for string length constraints,
- [range] => for numerical limits, etc.
- [RegularExpression] => for pattern matching (e.g., email format).

- [JsonIgnore] => to prevent circular references during JSON serialization.
- [ForeignKey] => to define foreign key relationships.

1. **Users Table**
   - `UID` (Primary Key): Unique identifier for each user .
   - `UName`: User's name .
   - `password`: Hashed password for security. 
   - `email`: User's email address.
   - `created_at`: Timestamp of account creation.
	- `Phone` : User's phone number.
	- `Role`: Role of the user (e.g., customer, admin , Manager).
2. **Products Table**
   - `PID` (Primary Key): Unique identifier for each product.
   - `ProductName`: Name of the product.
   - `Description`: Detailed description of the product.
   - `price`: Price of the product.
   - `stock`(quantity): Available stock for the product.
   - `OverallRating`: Average rating of the product.
3. **Reviews Table**
   - `ReviewID` (Primary Key): Unique identifier for each review.
   - `UID` (Foreign Key): References the user who wrote the review.
   - `PID` (Foreign Key): References the product being reviewed.
   - `rating`: Rating given by the user.
   - `comment`: Review comment.
   - `ReviewDate`: Timestamp of when the review was created.
4. **OrderProducts Table**
   - `OID` (Primary Key): Unique identifier for each order.
   - `PID` (Foreign Key): References the product being ordered.)
   - `Quantity`: Quantity of the product ordered.
5. **Orders Table**
   - `OID` **int**  => (Primary Key) Unique identifier for each order.
   - `UID` **int** => (Foreign Key) References the user who placed the order.
   - `OrderDate` **DateTime** =>  Timestamp of when the order was placed.
   - `TotalAmount` **Decimal** => Total amount for the order.

## Relationships description 
- A user can place multiple orders (One-to-Many relationship between Users and Orders).
- An order can contain multiple products (Many-to-Many relationship between Orders and Products, resolved through the OrderProducts table).
- A user can write multiple reviews, but each review is for a single product (One-to-Many relationship between Users and Reviews, and One-to-Many relationship between Products and Reviews).
- Each product can have multiple reviews (One-to-Many relationship between Products and Reviews).
- Each order is associated with a single user (Many-to-One relationship between Orders and Users).

## Navigation 

**Orders Table**

Each Order record is associated with a single User record.
Each Order record is associated with one or more OrderProduct records.

**OrderProducts Table**

Each OrderProduct record is associated with a single Order record.
Each OrderProduct record is associated with a single Product record.

**Products Table**

Each Product record can have multiple Review records.
Each Product record can have multiple OrderProduct records.

**Reviews Table**

Each Review record is associated with a single User record.
Each Review record is associated with a single Product record.

**Users Table**
Each User record can have multiple Order records.
Each User record can have multiple Review records.

## DB Context
**ECommerceContext Class**

  - This class inherits from DbContext and represents the session with the database.
  - It includes DbSet properties for each of the tables: Users, Products, Reviews, Orders, and OrderProducts.
  - The OnModelCreating method is overridden to configure relationships and constraints using Fluent API.

## Databse Schema 

![](img/ecommerce_schema.JPG)



---

## Repositories
- Each repository class implements CRUD operations for its respective entity, which mean Repositories include methods for adding, retrieving, updating, and deleting records.
- They interact with the ECommerceContext to perform database operations.
- Example: OrderProductsRepo , injecting the DbContext through the constructor to enable database operations.
    ```sql
        private readonly ApplicationDbContext _context;

        public OrderProductsRepo(ApplicationDbContext context)
        {
            _context = context;
        }

    ```
## Services with DTO
- Services contain business logic and interact with repositories to perform operations.
- Example: OrderProductsService, which uses the OrderProductsRepo to manage order products.
    ```sql
        private readonly OrderProductsRepo _orderProductsRepo;
        public OrderProductsService(OrderProductsRepo orderProductsRepo)
        {
            _orderProductsRepo = orderProductsRepo;
        }
    ```
- They handle tasks such as validating data, processing orders, and managing user accounts.
- Dto (Data Transfer Objects) are used to transfer data between layers, ensuring that only necessary information is exposed.
- DTOs help in shaping the data according to the requirements of the client or API consumers.
- DTO Example : OrderProductsDto
    ```sql
        public class OrderProductsDto
        {
            public int OID { get; set; }
            public int PID { get; set; }
            public int Quantity { get; set; }
        }
    ```
In above example, every property in the DTO corresponds to a field in the OrderProducts entity, facilitating data transfer without exposing the entire entity.

## Controllers
- Controllers handle HTTP requests and responses, acting as the entry point for API calls.
- They use services to perform operations and return appropriate responses.
``` sql
private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

```

- They define routes and actions for creating, retrieving, updating, and deleting resources.
- Controllers ensure that the application adheres to RESTful principles, providing a structured way to interact with the e-commerce system.
- HTTP Method Mapping
    - GET => Retrieve data (e.g., get all products, get user details).
    - POST => Create new resources (e.g., create a new order, add a product).
    - PUT => Update existing resources (e.g., update user information, modify product details).
    - DELETE => Remove resources (e.g., delete a review, remove a product from the catalog).
`Example`
```sql 

[HttpPost("PlaceOrder")]

```
The above example shows a POST method in the OrderController for placing a new order.
- Each controller method corresponds to a specific action, ensuring clear and organized handling of requests.
- Method return types
    - IActionResult => Provides flexibility in returning different HTTP status codes and responses.
    - ActionResult<T> => Combines the benefits of IActionResult with strong typing, allowing for more specific return types.

`Example`
```sql
public IActionResult PlaceOrder([FromBody] List<OrderItemDTO> items)
```
The above example shows a method in the OrderController that returns an IActionResult, allowing for various HTTP responses based on the outcome of the operation.
[From Body] => indicates that the items parameter should be bound from the body of the HTTP request.
- Retrieve the Authorization header from the request
`Example`
```sql
  var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

```
Above example shows how to extract the JWT token from the Authorization header in an HTTP request.
note : This is commonly used in scenarios where the API requires authentication, and the token is needed to verify the user's identity and permissions. 

---
#### **Authentication and Authorization**
- **Authentication** is the process of verifying the identity of a user or system. This is commonly done through a login process where a user provides credentials (such as a username and password) that are then checked against a trusted database. 
`Example`
1. => User Login by email and password
```sql
[AllowAnonymous]
        [HttpGet("Login")]
        public IActionResult Login(string email, string password)
        {
            try
            {
                var user = _userService.GetUSer(email, password);
                string token = GenerateJwtToken(user.UID.ToString(), user.UName, user.Role);
                return Ok(token);

            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while login. {(ex.Message)}");
            }

        }
```

2. => Generate Token
```sql
[NonAction]
        public string GenerateJwtToken(string userId, string username, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(JwtRegisteredClaimNames.UniqueName, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
```
3. => from token value can get user id to be able to use it in any controller

```sql
 private string? GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Extract the 'sub' claim
                var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");


                return (subClaim?.Value); // Return both values as a tuple
            }

            throw new UnauthorizedAccessException("Invalid or unreadable token.");
        }
```

`Explanation above code `

JwtSecurityTokenHandler: This class, from the System.IdentityModel.Tokens.Jwt library, provides the tools to read and validate JWTs.

handler.CanReadToken(token): This initial check verifies that the token's format is valid before attempting a full decode.

jwtToken.Claims: A JWT is essentially a collection of claims, which are pieces of information about the entity (e.g., the user).

Extracting the sub claim: The code specifically looks for the claim with the type "sub", which stands for "subject." This claim is a standard way to store a unique identifier for the user within the token.

Return Value: The method returns the value of the "sub" claim. The ? (null-conditional operator) ensures that null is returned if the claim is not found, preventing errors.

Security: If the token is invalid or unreadable, the method throws an UnauthorizedAccessException, which is a robust way to handle failed token validation.

4. => Through token user has access to Controllers methods that are authorized only

- **Authorization** is the process of determining what actions or resources a user or system is allowed to access.
`Example`
```sql
  var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
````
- The system uses JWT (JSON Web Tokens) for authentication and authorization.
- Users must log in to receive a token, which is then used to access protected resources.
- The token is included in the Authorization header of HTTP requests.

## API Services 
**Order Controller**

- PlaceOrder => Allows users to place a new order by providing a list of order items.
- GetOrdersByID => Retrieves all orders placed by a specific Order ID.
- GetAllOrders => Retrieves all orders in the system (admin access).

**Product Controller**

- AddProduct => Allows admins to add a new product to the catalog.
- GetAllProducts => Retrieves a list of all products.
- GetProductById => Retrieves details of a specific product by its ID.
- UpdateProduct => Allows admins to update product details.

**User Controller**
- Register => Allows new users to register by providing their details.
- Login => Authenticates users and returns a JWT token.
- GetUserById => Retrieves user details by their ID.

**Review Controller**
- AddReview => Allows users to add a review for a product.
- GetAllReviews => Retrieves all reviews for a specific product.
- DeleteReview => Allows users to delete their review by its ID.
- UpdateReview => Allows users to update their review by its ID.


