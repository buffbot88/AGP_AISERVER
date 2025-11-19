# Contributing to ASHATAIServer

Thank you for your interest in contributing to ASHATAIServer! We welcome contributions from the community.

## How to Contribute

### Reporting Issues

- Use the GitHub issue tracker to report bugs or suggest features
- Check if the issue already exists before creating a new one
- Provide clear, detailed information including steps to reproduce bugs
- Include environment details (OS, .NET version, etc.)

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the phased implementation plan** in PHASES.md when possible
3. **Write clear, concise commit messages**
4. **Ensure your code builds** without errors or warnings
5. **Add or update tests** for your changes
6. **Follow the existing code style** (see .editorconfig)
7. **Update documentation** if you change functionality
8. **Submit your pull request** with a clear description of changes

### Development Setup

1. Install [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
2. Clone the repository
3. Restore dependencies:
   ```bash
   dotnet restore ASHATAIServer/ASHATAIServer.csproj
   ```
4. Build the project:
   ```bash
   dotnet build ASHATAIServer/ASHATAIServer.csproj
   ```
5. Run tests:
   ```bash
   dotnet test
   ```

### Code Style

- Follow the .editorconfig settings
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Use async/await for I/O operations

### Testing

- Write unit tests for new functionality
- Ensure all tests pass before submitting PR
- Aim for good test coverage of critical paths
- Use TestServer for integration tests

### Commit Messages

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit first line to 72 characters
- Reference issues and pull requests when relevant

### Review Process

1. Maintainers will review your PR
2. Address any requested changes
3. Once approved, your PR will be merged

### Phased Development

We follow a phased implementation approach outlined in PHASES.md:
- **Phase 0**: CI/CD and project infrastructure
- **Phase 1**: Pluggable inference runtime
- **Phase 2**: Streaming and project generation
- **Phase 3**: Security and authentication
- **Phase 4**: Packaging and deployment
- **Phase 5**: Client tooling
- **Phase 6**: Tests and monitoring
- **Phase 7**: Advanced features (long-term)

Consider which phase your contribution fits into and coordinate with ongoing work.

## Code of Conduct

Please note that this project follows a [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## Questions?

Feel free to open an issue for questions or reach out to the maintainers.

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (see LICENSE file).
