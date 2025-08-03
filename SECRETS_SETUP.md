# 🔐 Secrets and Environment Variables Setup

## Overview

This RBAC API has been configured to use environment variables for sensitive configuration data like database passwords and JWT secrets, instead of hardcoding them in configuration files.

## 🚀 Quick Setup

### 1. Local Development

1. **Copy the example environment file:**
   ```bash
   cp .env.example .env
   ```

2. **Edit the `.env` file with your actual values:**
   ```bash
   # Database Configuration
   DB_HOST=localhost
   DB_NAME=RBACApiDb
   DB_USERNAME=postgres
   DB_PASSWORD=your-actual-password
   DB_PORT=5432

   # JWT Configuration
   JWT_SECRET_KEY=YourActualSecretKeyThatIsAtLeast32CharactersLong!
   ```

3. **Install dotnet-env (if not already installed):**
   ```bash
   dotnet add package DotNetEnv
   ```

4. **The application will automatically load environment variables from the `.env` file.**

### 2. Production/CI/CD Setup

#### GitHub Secrets Configuration

1. **Go to your GitHub repository**
2. **Navigate to Settings → Secrets and variables → Actions**
3. **Add the following secrets:**

**For Staging Environment:**
- `STAGING_DB_HOST`
- `STAGING_DB_NAME` 
- `STAGING_DB_USERNAME`
- `STAGING_DB_PASSWORD`
- `STAGING_DB_PORT`
- `STAGING_JWT_SECRET_KEY`

**For Production Environment:**
- `PROD_DB_HOST`
- `PROD_DB_NAME`
- `PROD_DB_USERNAME` 
- `PROD_DB_PASSWORD`
- `PROD_DB_PORT`
- `PROD_JWT_SECRET_KEY`

## 📋 Environment Variables Reference

| Variable | Description | Example |
|----------|-------------|---------|
| `DB_HOST` | Database server hostname | `localhost` or `pg-xxx.aivencloud.com` |
| `DB_NAME` | Database name | `RBACApiDb` |
| `DB_USERNAME` | Database username | `postgres` |
| `DB_PASSWORD` | Database password | `your-secure-password` |
| `DB_PORT` | Database port | `5432` |
| `JWT_SECRET_KEY` | JWT signing secret (min 32 chars) | `YourSecretKey...` |

## 🛠️ Configuration Files

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USERNAME};Password=${DB_PASSWORD};Port=${DB_PORT}"
  },
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "RBACApi",
    "Audience": "RBACApiUsers",
    "ExpiryInHours": "24"
  }
}
```

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USERNAME};Password=${DB_PASSWORD};Port=${DB_PORT}"
  }
}
```

## 🔄 GitHub Actions Workflow

The included workflow (`.github/workflows/deploy.yml`) automatically:

1. **Builds and tests** the application
2. **Deploys to staging** when code is pushed to `develop` branch
3. **Deploys to production** when code is pushed to `main` branch
4. **Uses environment-specific secrets** for each deployment

## 🔒 Security Best Practices

### ✅ What We've Implemented

- **Environment Variables**: Secrets stored as environment variables
- **GitHub Secrets**: Sensitive data stored in GitHub encrypted secrets
- **Separate Environments**: Different secrets for staging/production
- **No Hardcoded Secrets**: No passwords in source code or config files

### 🚨 Important Security Notes

1. **Never commit `.env` files** - They're in `.gitignore`
2. **Use strong JWT secrets** - Minimum 32 characters, random
3. **Rotate secrets regularly** - Update passwords periodically
4. **Use separate databases** - Different DBs for staging/production
5. **Monitor access logs** - Keep track of who accesses secrets

## 🐳 Docker Setup (Optional)

If using Docker, create a `docker-compose.yml`:

```yaml
version: '3.8'
services:
  rbac-api:
    build: .
    ports:
      - "5000:80"
    environment:
      - DB_HOST=${DB_HOST}
      - DB_NAME=${DB_NAME}
      - DB_USERNAME=${DB_USERNAME}
      - DB_PASSWORD=${DB_PASSWORD}
      - DB_PORT=${DB_PORT}
      - JWT_SECRET_KEY=${JWT_SECRET_KEY}
    env_file:
      - .env
```

## 🧪 Testing the Setup

1. **Verify environment variables are loaded:**
   ```bash
   dotnet run
   ```

2. **Check the connection string in logs** (should show environment variables are replaced)

3. **Test API endpoints** to ensure database connectivity

## 🚨 Troubleshooting

### Common Issues

1. **Environment variables not found:**
   - Ensure `.env` file exists and has correct format
   - Check for typos in variable names
   - Restart the application after changing `.env`

2. **Database connection fails:**
   - Verify database credentials are correct
   - Check network connectivity to database host
   - Ensure database server is running

3. **JWT token errors:**
   - Verify JWT secret is at least 32 characters
   - Check for special characters that might need escaping

### Debug Commands

```bash
# Check if environment variables are set
echo $DB_HOST
echo $JWT_SECRET_KEY

# Test database connection
psql -h $DB_HOST -p $DB_PORT -U $DB_USERNAME -d $DB_NAME
```

## 📚 Additional Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [GitHub Encrypted Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [Environment Variables Best Practices](https://12factor.net/config)

---

**✅ Your database passwords and JWT secrets are now properly secured with environment variables and GitHub secrets!**