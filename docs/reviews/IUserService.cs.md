# Exhaustive Review of `Services/Interfaces/IUserService.cs`

The `IUserService.cs` file, located in the `Domain/Services/Interfaces` directory, defines the `IUserService` interface. This is the final and one of the most critical service interfaces in the domain layer. It establishes the contract for the business logic that directly manages the `User` entity. While `IAuthService` handles the process of authentication (logging in, creating tokens), `IUserService` is responsible for the underlying CRUD-like (Create, Read, Update, Delete) operations and other management tasks related to user accounts. This separation of concerns between authentication process and user management is a clean and effective design choice.

The declaration `public interface IUserService` defines a public contract that will be implemented by the `UserService` class. Various parts of the application, including the `AuthService` and the API endpoints in `UserEndpoints.cs`, will depend on this interface.

Let's perform a detailed analysis of each method defined in the interface:

`Task<User?> GetByIdAsync(long id);`
This is a standard method for retrieving a single `User` by their primary key. It's a fundamental use case for any system that needs to look up user details. The `Task<User?>` return type correctly indicates that the operation is asynchronous and that a user with the given ID might not be found.

`Task<User?> GetByCredentialsAsync(string email, string password);`
This method defines the contract for a critical piece of the login logic. It takes a user's `email` and plain-text `password` and is responsible for retrieving the corresponding `User` entity *if and only if* the credentials are valid. The implementation of this method will first find the user by email using the repository, and then it will use the `IPasswordSecurity` service to verify that the provided password matches the stored hash. It returns the `User` object on success or `null` on failure. The `AuthService` will call this method as the first step in its `LoginAsync` workflow.

`Task<User?> CreateAsync(UserCreateData data);`
This method defines the contract for creating a new user entity. It takes a `UserCreateData` DTO, which contains the necessary email and password. This method's responsibility is to map the DTO to a `User` entity (using the `UserMapper` and `IPasswordSecurity` service) and then add it to the repository. It returns the newly created `User` object or `null` if the creation failed (e.g., because a user with that email already exists). The `AuthService`'s `RegisterAsync` method will call this as its first step.

`Task<bool> DeleteAllInactiveUsersAsync();`
This method defines a contract for a background maintenance or administrative task. It provides a way to trigger the permanent deletion of all users who are currently in an "inactive" state (as defined by the `User.IsActive` property). The service implementation would use the `IUserRepository.GetAllInactiveAsync` method to fetch these users and then call the `Remove` method for each one, finally committing the changes with the `IUnitOfWork`. The `bool` return type provides a simple success/failure indicator.

`Task<OperationResult> UpdateRoleByIdAsync(long id, UserRoleUpdateData data);`
This method defines the contract for the privileged operation of updating a user's role. It takes the `id` of the user to modify and a `UserRoleUpdateData` DTO containing the new role. The use of a specific DTO is an excellent security practice. The `OperationResult` return type allows the service to communicate the specific outcome (`Success`, `NotFound`, etc.) to the caller.

`Task<OperationResult> UpdateCredentialsByIdAsync(long id, UserCredentialsUpdateData data);`
*Note: The actual file is named `UserUpdateData.cs` but for clarity in this review, `UserCredentialsUpdateData` is a more descriptive name for its purpose.*
This method defines the contract for updating a user's credentials (email and password). It takes the user's `id` and a `UserCredentialsUpdateData` DTO. This maintains the clean separation from the role update operation. The service implementation would handle the logic for hashing the new password and updating the entity. The `OperationResult` return type is again used for a detailed outcome.

`Task<OperationResult> DeleteByIdAsync(long id);`
This method defines the contract for deleting a single user by their ID. The implementation of this would likely perform a "soft delete" by setting the `Deleted` timestamp on the `User` entity, rather than a physical deletion. This is a more robust approach. The `OperationResult` return type provides clear feedback on the outcome of the operation.

In conclusion, the `IUserService.cs` interface provides a comprehensive and well-structured contract for all user management business logic. It cleanly separates these responsibilities from the authentication flow logic in `IAuthService`. The methods are well-named, asynchronous, and use specific DTOs and expressive result types, all of which are hallmarks of a high-quality, maintainable, and secure design. This file is excellently designed and requires no recommendations for improvement. It successfully completes the definition of the core service interfaces for the domain.