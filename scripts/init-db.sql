-- Initialize RBAC database

-- Create extensions if they don't exist
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create database user if not exists (for development)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'rbac_user') THEN
        CREATE USER rbac_user WITH PASSWORD 'rbac_password';
    END IF;
END
$$;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE rbac_db TO rbac_user;

-- Create initial tables will be handled by Entity Framework migrations
-- This script is just for database initialization

SELECT 'Database initialized successfully' as status;