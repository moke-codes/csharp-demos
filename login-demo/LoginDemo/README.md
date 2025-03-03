# Login Demo

A secure authentication system built with ASP.NET Core, featuring user registration, login, password management, and JWT-based authentication.

## Features

- User registration with email and password
- Secure login with JWT authentication
- Password reset functionality via email
- User profile management
- Rate limiting for API endpoints
- Secure password storage with hashing
- Remember me functionality
- Email notifications for password reset

## Project Structure

```
LoginDemo/
├── Controllers/           # API endpoints
│   └── AuthController.cs  # Authentication endpoints
├── Data/                 # Data access layer
│   └── ApplicationDbContext.cs
├── Models/               # Domain models
│   └── ApplicationUser.cs
├── Services/             # Business logic
│   ├── AuthService.cs    # Authentication logic
│   └── EmailService.cs   # Email notifications
├── wwwroot/             # Static files
│   ├── css/            # Stylesheets
│   ├── js/             # JavaScript files
│   ├── icons/          # SVG icons
│   └── *.html          # HTML pages
├── Migrations/          # Database migrations
└── Program.cs          # Application entry point
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login with email and password
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token
- `POST /api/auth/logout` - Logout user

### User Profile
- `GET /api/auth/profile` - Get user profile
- `PUT /api/auth/profile` - Update user profile
- `POST /api/auth/change-password` - Change password

## Setup

1. Clone the repository
2. Copy `appsettings.template.json` to `appsettings.Development.json`:
   ```bash
   cp appsettings.template.json appsettings.Development.json
   ```

3. Update the following settings in `appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=app.db"
     },
     "JwtSettings": {
       "SecretKey": "your-secure-secret-key-here",
       "Issuer": "your-issuer",
       "Audience": "your-audience",
       "ExpirationMinutes": 10080
     },
     "EmailSettings": {
       "SmtpServer": "smtp.example.com",
       "SmtpPort": 587,
       "SmtpUsername": "your-email@example.com",
       "SmtpPassword": "your-email-password",
       "FromEmail": "noreply@example.com",
       "FromName": "Login Demo"
     }
   }
   ```

   Important notes:
   - Generate a strong secret key for JWT (at least 32 characters)
   - Use app-specific passwords for email accounts
   - Never commit `appsettings.Development.json` to version control

4. Run the application:
   ```bash
   dotnet run
   ```

5. The application will be available at:
   - API: https://localhost:7081
   - Web Interface: https://localhost:7081/index.html

## Configuration

### Development Settings
The application uses different configuration files for different environments:
- `appsettings.json` - Default settings
- `appsettings.Development.json` - Development-specific settings (not in git)
- `appsettings.template.json` - Template for development settings

### Security Features
- JWT-based authentication
- Password hashing with ASP.NET Core Identity
- Rate limiting on API endpoints
- HTTPS enabled
- Secure password requirements:
  - Minimum 8 characters
  - Requires uppercase
  - Requires lowercase
  - Requires numbers
  - Requires special characters

## Testing the API

Use the `LoginDemo.http` file in the project root to test the API endpoints. The file includes:
- Example requests for all endpoints
- Required headers and request bodies
- Authentication token handling

## Development Notes

- The application uses SQLite for development
- Email notifications are configured for development
- Rate limiting is enabled to prevent abuse
- JWT tokens expire after 7 days (configurable)

## Security Considerations

1. Never commit sensitive configuration files
2. Use strong passwords in production
3. Configure proper CORS settings for production
4. Use HTTPS in production
5. Regularly rotate JWT secrets
6. Monitor rate limiting logs

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License.