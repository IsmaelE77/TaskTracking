# Docker Setup for TaskTracking Application

This document explains how to run the TaskTracking application using Docker and Docker Compose.

## Prerequisites

- Docker Desktop or Docker Engine
- Docker Compose
- At least 4GB of available RAM

## Available Services

The application consists of several containerized services:

1. **MySQL Database** (`mysql`) - Database server
2. **Database Migrator** (`migrator`) - Runs database migrations
3. **API Host** (`api`) - REST API backend service
4. **Blazor Server** (`blazor`) - Main web application
5. **Traditional Web** (`web`) - Alternative web interface (optional)

## Quick Start

### 1. Build and Run All Services

```bash
# Build and start all services
docker-compose up --build

# Or run in detached mode
docker-compose up --build -d
```

### 2. Access the Application

- **Blazor Application**: http://localhost:8080
- **API Documentation**: http://localhost:8081/swagger
- **MySQL Database**: localhost:3306

### 3. Stop the Services

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: This will delete all data)
docker-compose down -v
```

## Individual Service Management

### Build Specific Services

```bash
# Build only the Blazor application
docker build -f Dockerfile.blazor -t tasktracking-blazor .

# Build only the API
docker build -f Dockerfile.api -t tasktracking-api .

# Build only the Web application
docker build -f Dockerfile.web -t tasktracking-web .

# Build only the Database Migrator
docker build -f Dockerfile.migrator -t tasktracking-migrator .
```

### Run Individual Services

```bash
# Run only specific services
docker-compose up mysql migrator api

# Run only the Blazor app (requires API to be running)
docker-compose up blazor
```

## Environment Configuration

### Development Environment

The `docker-compose.override.yml` file provides development-specific settings:
- Simplified passwords
- Debug logging enabled
- Source code mounting for hot reload (optional)

### Production Environment

For production deployment:

1. Update passwords in `docker-compose.yml`
2. Set proper CORS origins
3. Configure HTTPS certificates
4. Use environment-specific configuration files

### Environment Variables

Key environment variables you can customize:

```bash
# Database
MYSQL_ROOT_PASSWORD=your_secure_password
MYSQL_PASSWORD=your_secure_password
ConnectionStrings__Default=Server=mysql;Port=3306;Database=TaskTracking;Uid=tasktracking;Pwd=your_password;

# Application URLs
App__SelfUrl=http://localhost:8081
App__CorsOrigins=http://localhost:8080
AuthServer__Authority=http://localhost:8081

# Logging
Logging__LogLevel__Default=Information
```

## Troubleshooting

### Common Issues

1. **Port Conflicts**
   ```bash
   # Check what's using the ports
   netstat -tulpn | grep :8080
   netstat -tulpn | grep :3306
   ```

2. **Database Connection Issues**
   ```bash
   # Check if MySQL is healthy
   docker-compose ps
   docker-compose logs mysql
   ```

3. **Build Failures**
   ```bash
   # Clean build
   docker-compose down
   docker system prune -f
   docker-compose build --no-cache
   ```

### Logs

```bash
# View logs for all services
docker-compose logs

# View logs for specific service
docker-compose logs blazor
docker-compose logs api
docker-compose logs mysql

# Follow logs in real-time
docker-compose logs -f blazor
```

### Database Management

```bash
# Access MySQL shell
docker-compose exec mysql mysql -u root -p

# Backup database
docker-compose exec mysql mysqldump -u root -p TaskTracking > backup.sql

# Restore database
docker-compose exec -T mysql mysql -u root -p TaskTracking < backup.sql
```

## File Structure

```
├── Dockerfile                 # Default Dockerfile (Blazor app)
├── Dockerfile.api            # API Host service
├── Dockerfile.blazor         # Blazor Server application
├── Dockerfile.web            # Traditional Web application
├── Dockerfile.migrator       # Database migrator
├── docker-compose.yml        # Main compose configuration
├── docker-compose.override.yml # Development overrides
├── .dockerignore            # Files to exclude from Docker context
└── README.Docker.md         # This file
```

## Performance Tips

1. **Use multi-stage builds** - Already implemented in Dockerfiles
2. **Optimize layer caching** - Copy project files before source code
3. **Use .dockerignore** - Exclude unnecessary files
4. **Allocate sufficient memory** - At least 4GB recommended

## Security Considerations

1. Change default passwords in production
2. Use secrets management for sensitive data
3. Configure proper CORS origins
4. Enable HTTPS in production
5. Regularly update base images
