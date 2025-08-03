# 🐳 Docker Setup Guide

## Overview

The RBAC API has been fully containerized with Docker, providing consistent deployment across all environments with proper secrets management.

## 📁 Docker Files Structure

```
RBACApi/
├── Dockerfile                    # Multi-stage build for production
├── .dockerignore                # Excludes unnecessary files from build context
├── docker-compose.yml           # Local development with PostgreSQL
├── docker-compose.prod.yml      # Production deployment
└── scripts/
    └── init-db.sql              # Database initialization script
```

## 🚀 Quick Start

### 1. Local Development

**Prerequisites:**
- Docker and Docker Compose installed
- Copy `.env.example` to `.env` and configure

```bash
# Clone and navigate to project
cd /path/to/RBACApi

# Create environment file
cp .env.example .env

# Edit .env with your values (or use defaults for local dev)
# DB_HOST=postgres
# DB_NAME=rbac_db
# DB_USERNAME=postgres
# DB_PASSWORD=postgres
# DB_PORT=5432
# JWT_SECRET_KEY=YourSecretKey...

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f rbac-api

# Stop services
docker-compose down
```

**Services Started:**
- **RBAC API**: http://localhost:5000
- **PostgreSQL**: localhost:5432
- **Adminer** (DB Admin): http://localhost:8080

### 2. Production Deployment

```bash
# Set environment variables
export DB_HOST=your-prod-db-host
export DB_NAME=your-prod-db
export DB_USERNAME=your-username
export DB_PASSWORD=your-password
export DB_PORT=5432
export JWT_SECRET_KEY=your-jwt-secret

# Deploy to production
docker-compose -f docker-compose.prod.yml up -d

# Check status
docker-compose -f docker-compose.prod.yml ps
```

## 🏗️ Dockerfile Features

### Multi-Stage Build
```dockerfile
# Build stage - Uses .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Runtime stage - Uses lighter ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
```

### Security Features
- **Non-root user**: Application runs as `appuser`
- **Health checks**: Built-in container health monitoring
- **Minimal surface**: Only includes necessary runtime components

### Production Optimizations
- **Layer caching**: Optimal Docker layer structure
- **Multi-architecture**: Supports AMD64 and ARM64
- **Health monitoring**: Automatic container health checks

## 🔧 Environment Configuration

### Development Environment Variables
```bash
# Database
DB_HOST=postgres              # Docker service name
DB_NAME=rbac_db
DB_USERNAME=postgres
DB_PASSWORD=postgres
DB_PORT=5432

# JWT
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%^&*()
```

### Production Environment Variables
```bash
# Database (use your actual production values)
DB_HOST=your-production-db-host
DB_NAME=your-production-db
DB_USERNAME=your-production-username
DB_PASSWORD=your-secure-production-password
DB_PORT=5432

# JWT (generate a secure key)
JWT_SECRET_KEY=your-production-jwt-secret-key
```

## 🐳 Docker Commands Reference

### Building
```bash
# Build the image locally
docker build -t rbac-api:latest .

# Build with specific tag
docker build -t rbac-api:v1.0.0 .

# Build for multiple architectures
docker buildx build --platform linux/amd64,linux/arm64 -t rbac-api:latest .
```

### Running
```bash
# Run container directly
docker run -d \
  --name rbac-api \
  -p 5000:8080 \
  -e DB_HOST=your-db-host \
  -e DB_PASSWORD=your-password \
  rbac-api:latest

# Run with environment file
docker run -d \
  --name rbac-api \
  -p 5000:8080 \
  --env-file .env \
  rbac-api:latest
```

### Management
```bash
# View logs
docker logs rbac-api
docker logs -f rbac-api  # Follow logs

# Execute commands in container
docker exec -it rbac-api bash

# Check container health
docker inspect rbac-api | grep Health

# Restart container
docker restart rbac-api

# Stop and remove
docker stop rbac-api
docker rm rbac-api
```

## 🏥 Health Checks

### Container Health Check
The Dockerfile includes automatic health monitoring:
```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
```

### Health Check Endpoint
The API exposes a health check endpoint:
- **URL**: `/health`
- **Method**: GET
- **Response**: 200 OK when healthy

### Monitoring Health
```bash
# Check health status
docker ps  # Shows health status in STATUS column

# View detailed health info
docker inspect rbac-api --format='{{.State.Health.Status}}'

# View health check logs
docker inspect rbac-api --format='{{range .State.Health.Log}}{{.Output}}{{end}}'
```

## 🚀 CI/CD Integration

### GitHub Actions Workflow

The project includes automated Docker builds and deployments:

1. **Build & Push**: Builds multi-architecture images and pushes to Docker Hub
2. **Deploy Staging**: Deploys to staging environment on `develop` branch
3. **Deploy Production**: Deploys to production on `main` branch

### Required GitHub Secrets
```bash
# Docker Hub
DOCKER_USERNAME=your-dockerhub-username
DOCKER_PASSWORD=your-dockerhub-password

# Staging Environment
STAGING_DB_HOST=staging-db-host
STAGING_DB_NAME=staging-db-name
STAGING_DB_USERNAME=staging-username
STAGING_DB_PASSWORD=staging-password
STAGING_DB_PORT=5432
STAGING_JWT_SECRET_KEY=staging-jwt-secret

# Production Environment
PROD_DB_HOST=production-db-host
PROD_DB_NAME=production-db-name
PROD_DB_USERNAME=production-username
PROD_DB_PASSWORD=production-password
PROD_DB_PORT=5432
PROD_JWT_SECRET_KEY=production-jwt-secret
```

## 📊 Monitoring & Logging

### Container Logs
```bash
# Docker Compose logs
docker-compose logs rbac-api
docker-compose logs postgres
docker-compose logs adminer

# Follow logs in real-time
docker-compose logs -f rbac-api

# View last N lines
docker-compose logs --tail=100 rbac-api
```

### Production Logging
The production configuration includes log rotation:
```yaml
logging:
  driver: "json-file"
  options:
    max-size: "10m"
    max-file: "3"
```

## 🛠️ Troubleshooting

### Common Issues

#### 1. Port Already in Use
**Error**: `Port 5000 is already allocated`
**Solution**:
```bash
# Check what's using the port
lsof -i :5000

# Use different port
docker-compose up -d --scale rbac-api=0
docker-compose up -d
```

#### 2. Database Connection Issues
**Error**: `Connection refused` or timeout errors
**Solution**:
```bash
# Check if PostgreSQL is running
docker-compose ps postgres

# Check PostgreSQL logs
docker-compose logs postgres

# Verify environment variables
docker-compose exec rbac-api env | grep DB_
```

#### 3. Health Check Failures
**Error**: Container marked as unhealthy
**Solution**:
```bash
# Check health check logs
docker inspect rbac-api --format='{{range .State.Health.Log}}{{.Output}}{{end}}'

# Test health endpoint manually
curl http://localhost:5000/health

# Check if application is running
docker-compose logs rbac-api
```

#### 4. Build Failures
**Error**: Docker build fails
**Solution**:
```bash
# Clean Docker cache
docker system prune -a

# Rebuild without cache
docker-compose build --no-cache

# Check .dockerignore file
cat .dockerignore
```

### Debug Commands
```bash
# Enter container for debugging
docker-compose exec rbac-api bash

# Check container processes
docker-compose exec rbac-api ps aux

# Test database connectivity
docker-compose exec rbac-api ping postgres

# View container resource usage
docker stats rbac-api
```

## 🔐 Security Considerations

### Production Security
- **Secrets**: Never hardcode secrets in Dockerfiles
- **User**: Application runs as non-root user
- **Network**: Use Docker networks for service isolation
- **Images**: Regularly update base images for security patches

### Best Practices
1. **Regular Updates**: Keep base images updated
2. **Minimal Images**: Use alpine or distroless images when possible
3. **Health Checks**: Always include health checks
4. **Resource Limits**: Set memory and CPU limits in production
5. **Monitoring**: Implement comprehensive logging and monitoring

## 📚 Additional Resources

- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)

---

**✅ Your RBAC API is now fully containerized and ready for deployment across all environments!**