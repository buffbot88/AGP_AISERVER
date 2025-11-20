# Release Guide

This document describes the release process for ASHATAIServer.

## Release Workflow

The release workflow is automated using GitHub Actions. It:

1. Builds and tests the project
2. Creates platform-specific binaries
3. Builds Docker images
4. Creates a GitHub Release with artifacts

## Creating a Release

### Automated Release (Recommended)

#### Via Git Tag

```bash
# Create and push a version tag
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

This triggers the release workflow automatically.

#### Via GitHub Actions

1. Go to **Actions** → **Release** workflow
2. Click **Run workflow**
3. Enter the version number (e.g., `1.0.0`)
4. Click **Run workflow**

### Manual Release

If you need to create a release manually:

```bash
# Build for all platforms
./scripts/publish.sh

# Or on Windows
.\scripts\publish.ps1

# Create release archives
cd publish
tar -czf ASHATAIServer-linux-x64.tar.gz linux-x64/
zip -r ASHATAIServer-win-x64.zip win-x64/
```

## Release Checklist

Before creating a release:

- [ ] All tests pass (`dotnet test`)
- [ ] Integration tests pass
- [ ] Security scan passes (CodeQL)
- [ ] Documentation is up to date
- [ ] PHASES.md reflects current status
- [ ] Version numbers updated in:
  - [ ] README.md
  - [ ] AGPClientWinForms/AGPClientWinForms.csproj
  - [ ] agp-cli/agp-cli.csproj
- [ ] CHANGELOG.md updated (if exists)

## Release Artifacts

Each release includes:

### Binaries

- `ASHATAIServer-win-x64.zip` - Windows 64-bit
- `ASHATAIServer-win-arm64.zip` - Windows ARM64
- `ASHATAIServer-linux-x64.tar.gz` - Linux 64-bit
- `ASHATAIServer-linux-arm64.tar.gz` - Linux ARM64
- `ASHATAIServer-linux-arm.tar.gz` - Linux ARM

### Docker Images

- `ashataiserver:latest` - Latest stable release
- `ashataiserver:X.Y.Z` - Specific version

## Version Numbering

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR** version for incompatible API changes
- **MINOR** version for new functionality (backward-compatible)
- **PATCH** version for bug fixes (backward-compatible)

Examples:
- `1.0.0` - First major release
- `1.1.0` - Added new features
- `1.1.1` - Bug fixes

## Docker Release Process

### Building Docker Images

```bash
# Build multi-architecture image
docker buildx build --platform linux/amd64,linux/arm64 \
  -t ashataiserver:1.0.0 \
  -t ashataiserver:latest \
  --push .
```

### Tagging Conventions

- `latest` - Most recent stable release
- `X.Y.Z` - Specific version
- `X.Y` - Latest patch for minor version
- `X` - Latest minor for major version

## Testing Releases

### Test Binaries

1. **Download the binary** for your platform
2. **Extract** the archive
3. **Run the server:**
   ```bash
   ./ASHATAIServer  # Linux
   ASHATAIServer.exe  # Windows
   ```
4. **Test health endpoint:**
   ```bash
   curl http://localhost:7077/api/ai/health
   ```

### Test Docker Image

```bash
# Pull the image
docker pull ashataiserver:1.0.0

# Run container
docker run -d -p 7077:7077 ashataiserver:1.0.0

# Test health
curl http://localhost:7077/api/ai/health

# View logs
docker logs <container-id>
```

## Release Notes Template

```markdown
# ASHATAIServer X.Y.Z

## 🚀 Features

- List new features
- One per line

## 🐛 Bug Fixes

- List bug fixes
- One per line

## 📦 Downloads

### Binaries

- Windows x64: ASHATAIServer-win-x64.zip
- Linux x64: ASHATAIServer-linux-x64.tar.gz

### Docker

```bash
docker pull ashataiserver:X.Y.Z
```

## 📖 Documentation

- [README.md](README.md)
- [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md)
- [docs/SECURITY.md](docs/SECURITY.md)
- [docs/CLIENTS.md](docs/CLIENTS.md)

## ⚠️ Breaking Changes

List any breaking changes (for major versions)

## 🔐 Security

- Argon2id password hashing
- API key authentication
- Rate limiting enabled
- See [docs/SECURITY.md](docs/SECURITY.md)

## 🙏 Contributors

Thanks to all contributors!
```

## Hotfix Releases

For critical bug fixes:

1. Create a hotfix branch from the release tag:
   ```bash
   git checkout -b hotfix/1.0.1 v1.0.0
   ```

2. Make the fix and commit

3. Create a new patch version tag:
   ```bash
   git tag -a v1.0.1 -m "Hotfix: Critical bug fix"
   git push origin v1.0.1
   ```

4. Merge back to main:
   ```bash
   git checkout main
   git merge hotfix/1.0.1
   git push origin main
   ```

## Rollback Process

If a release has critical issues:

### Docker Rollback

```bash
# Re-tag previous version as latest
docker tag ashataiserver:1.0.0 ashataiserver:latest
docker push ashataiserver:latest
```

### Binary Rollback

Download and use previous version from GitHub Releases.

## Release Verification

After release:

1. ✅ GitHub Release created with all artifacts
2. ✅ Docker images pushed to Docker Hub
3. ✅ Release notes published
4. ✅ Documentation updated
5. ✅ Health check passes on live release
6. ✅ Download and test sample binary

## Monitoring Post-Release

After releasing:

- Monitor GitHub Issues for bug reports
- Check Docker Hub download stats
- Monitor server logs (if deployed)
- Collect user feedback

## Continuous Delivery

For automatic releases on every commit to main:

1. Update `.github/workflows/release.yml`
2. Add trigger for pushes to main
3. Use commit hash or date as version

```yaml
on:
  push:
    branches:
      - main
    tags:
      - 'v*'
```

## Metrics to Track

- Download count (GitHub Releases)
- Docker pulls
- Issue reports post-release
- Build success rate
- Test pass rate

## Support Windows

For release support:

- GitHub Releases: Create issues for bugs
- Documentation: Check docs/ directory
- Community: Discussions tab

## License

All releases are subject to the license terms in [LICENSE](../LICENSE).

---

## Quick Commands

### Create Release
```bash
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0
```

### Build Locally
```bash
./scripts/publish.sh
```

### Test Release
```bash
docker run -p 7077:7077 ashataiserver:latest
curl http://localhost:7077/api/ai/health
```

### Rollback
```bash
docker tag ashataiserver:1.0.0 ashataiserver:latest
docker push ashataiserver:latest
```

---

**Maintained by:** AGP Studios  
**Last Updated:** 2025-11-20
