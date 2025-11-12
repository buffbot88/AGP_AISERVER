# Contributing to AGP Studios IDE

Thank you for your interest in contributing to AGP Studios IDE!

## Development Setup

1. **Prerequisites**
   - Windows 10 or later
   - Visual Studio 2022 (Community Edition or higher)
   - .NET 8.0 SDK
   - Git for Windows

2. **Clone the Repository**
   ```bash
   git clone https://github.com/buffbot88/AGP_Studios.git
   cd AGP_Studios
   ```

3. **Open in Visual Studio**
   - Open `AGP_Studios.sln`
   - Allow Visual Studio to restore NuGet packages
   - Build the solution (Ctrl+Shift+B)

## Project Architecture

### Models Layer
Contains data models and DTOs:
- `User.cs` - User authentication model
- `Game.cs` - Game and installation models
- `CodeDraft.cs` - Code draft model for admins
- `AppConfiguration.cs` - Application settings
- `AuthModels.cs` - Authentication request/response models

### Services Layer
Contains business logic and external integrations:
- `ApiClient.cs` - HTTP client for AI Server API
- `ConfigurationManager.cs` - Settings management
- `LocalRepository.cs` - Local file storage
- `GameServices.cs` - Game download and launch logic

### UI Layer
WPF windows and views:
- `LoginWindow` - Authentication screen
- `AdminConsoleWindow` - Code editor for admins
- `GameLibraryWindow` - Game browser for members

## Code Style Guidelines

1. **Naming Conventions**
   - PascalCase for classes, methods, properties
   - camelCase for local variables and parameters
   - Private fields prefixed with underscore: `_fieldName`

2. **Documentation**
   - Add XML documentation comments for public classes and methods
   - Use `<summary>` tags to describe purpose
   - Include `<param>` tags for method parameters

3. **Error Handling**
   - Use try-catch blocks for I/O operations
   - Log errors to Debug output
   - Show user-friendly error messages in UI

4. **Async/Await**
   - Use async/await for all I/O operations
   - Name async methods with `Async` suffix
   - Avoid blocking calls with `.Result` or `.Wait()`

## Testing AI Server Integration

To test without a live AI Server:

1. **Mock Server Option**
   - Create a simple mock server using ASP.NET Core
   - Implement the required endpoints (see README for API spec)
   - Return sample JSON data

2. **Local Data Testing**
   - Test draft management (works offline)
   - Test local storage functionality
   - Test UI navigation and controls

## Common Development Tasks

### Adding a New API Endpoint

1. Update `ApiClient.cs` with new method:
   ```csharp
   public async Task<ResponseType> MethodNameAsync(parameters)
   {
       var response = await _httpClient.GetAsync($"{GetBaseUrl()}/api/endpoint");
       // Handle response
   }
   ```

2. Add response model to `Models/` if needed

3. Call from UI window's code-behind

### Adding a New Window

1. Create XAML file in `UI/Windows/`
2. Create code-behind class
3. Update navigation logic in existing windows
4. Add any required services to constructor

### Modifying Local Storage

1. Update models in `Models/`
2. Modify `LocalRepository.cs` methods
3. Ensure backward compatibility with existing data
4. Test with existing user data

## Pull Request Process

1. **Create a Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Your Changes**
   - Follow code style guidelines
   - Add comments where necessary
   - Test thoroughly

3. **Commit Your Changes**
   ```bash
   git add .
   git commit -m "Description of changes"
   ```

4. **Push to GitHub**
   ```bash
   git push origin feature/your-feature-name
   ```

5. **Create Pull Request**
   - Provide clear description of changes
   - Reference any related issues
   - Wait for code review

## Security Considerations

- **Never commit credentials** or API keys
- **Use HTTPS** in production for AI Server
- **Validate user input** before processing
- **Sanitize file paths** to prevent directory traversal
- **Check file types** before extraction

## Reporting Issues

When reporting bugs, please include:
- Windows version
- .NET version
- Steps to reproduce
- Expected vs actual behavior
- Screenshots if applicable
- Relevant log output

## Feature Requests

Feature requests are welcome! Please:
- Check existing issues first
- Describe the use case
- Explain why it would be useful
- Consider providing a PR

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on the code, not the person
- Help others learn and grow

## License

By contributing to AGP Studios IDE, you agree that your contributions will be licensed under the same terms as the project.

## Questions?

- Open an issue for questions about the codebase
- Check the README for setup instructions
- Review existing PRs for examples

Thank you for contributing to AGP Studios IDE!
