-- =====================================================
-- DeveloperStore Database Schema Creation Script
-- PostgreSQL Version: 15+
-- Created: 2025-08-24
-- =====================================================

-- Create schema if not exists
CREATE SCHEMA IF NOT EXISTS developerstore;

-- Set default schema
SET search_path TO developerstore, public;

-- =====================================================
-- SALES TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS sales (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sale_number VARCHAR(50) NOT NULL UNIQUE,
    sale_date TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- Customer Information (Value Object)
    customer_id UUID NOT NULL,
    customer_name VARCHAR(200) NOT NULL,
    customer_email VARCHAR(320) NOT NULL,
    
    -- Branch Information (Value Object)
    branch_id UUID NOT NULL,
    branch_name VARCHAR(200) NOT NULL,
    branch_address TEXT NOT NULL,
    
    -- Money Value Objects
    subtotal_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    subtotal_currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    
    total_discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    total_discount_currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    
    total_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    total_amount_currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    
    -- Aggregate fields
    total_quantity INTEGER NOT NULL DEFAULT 0,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    
    -- Audit fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    version INTEGER NOT NULL DEFAULT 1
);

-- =====================================================
-- SALE ITEMS TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS sale_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sale_id UUID NOT NULL,
    
    -- Product Information (Value Object)
    product_id UUID NOT NULL,
    product_name VARCHAR(200) NOT NULL,
    product_category VARCHAR(100) NOT NULL,
    
    -- Unit Price (Money Value Object)
    unit_price_amount DECIMAL(18,2) NOT NULL,
    unit_price_currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    
    -- Quantity and totals
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    line_total_amount DECIMAL(18,2) NOT NULL,
    line_total_currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    
    -- Audit fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Foreign Key
    CONSTRAINT fk_sale_items_sale_id FOREIGN KEY (sale_id) REFERENCES sales(id) ON DELETE CASCADE
);

-- =====================================================
-- INDEXES FOR PERFORMANCE
-- =====================================================

-- Sales table indexes
CREATE INDEX IF NOT EXISTS idx_sales_sale_number ON sales(sale_number);
CREATE INDEX IF NOT EXISTS idx_sales_sale_date ON sales(sale_date);
CREATE INDEX IF NOT EXISTS idx_sales_customer_id ON sales(customer_id);
CREATE INDEX IF NOT EXISTS idx_sales_branch_id ON sales(branch_id);
CREATE INDEX IF NOT EXISTS idx_sales_status ON sales(status);
CREATE INDEX IF NOT EXISTS idx_sales_created_at ON sales(created_at);

-- Sale items table indexes
CREATE INDEX IF NOT EXISTS idx_sale_items_sale_id ON sale_items(sale_id);
CREATE INDEX IF NOT EXISTS idx_sale_items_product_id ON sale_items(product_id);
CREATE INDEX IF NOT EXISTS idx_sale_items_product_category ON sale_items(product_category);

-- =====================================================
-- CONSTRAINTS
-- =====================================================

-- Business rule constraints
ALTER TABLE sales 
ADD CONSTRAINT chk_sales_amounts_positive 
CHECK (
    subtotal_amount >= 0 AND 
    total_discount_amount >= 0 AND 
    total_amount >= 0
);

ALTER TABLE sales 
ADD CONSTRAINT chk_sales_total_quantity_positive 
CHECK (total_quantity >= 0);

ALTER TABLE sales 
ADD CONSTRAINT chk_sales_status_valid 
CHECK (status IN ('Active', 'Cancelled', 'Completed'));

ALTER TABLE sale_items 
ADD CONSTRAINT chk_sale_items_amounts_positive 
CHECK (
    unit_price_amount > 0 AND 
    line_total_amount > 0
);

ALTER TABLE sale_items 
ADD CONSTRAINT chk_sale_items_quantity_limits 
CHECK (quantity BETWEEN 1 AND 20); -- Business rule: max 20 items per product

-- =====================================================
-- TRIGGERS FOR UPDATED_AT
-- =====================================================

-- Function to update the updated_at column
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    NEW.version = OLD.version + 1;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Trigger for sales table
DROP TRIGGER IF EXISTS update_sales_updated_at ON sales;
CREATE TRIGGER update_sales_updated_at
    BEFORE UPDATE ON sales
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =====================================================
-- SAMPLE DATA (Optional)
-- =====================================================

-- Insert sample customers and branches (this would normally come from separate tables)
INSERT INTO sales (
    sale_number, sale_date, 
    customer_id, customer_name, customer_email,
    branch_id, branch_name, branch_address,
    subtotal_amount, total_discount_amount, total_amount, total_quantity,
    status
) VALUES 
(
    'SALE-001', 
    CURRENT_TIMESTAMP,
    'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'::UUID,
    'John Doe',
    'john.doe@email.com',
    'b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'::UUID,
    'Downtown Store',
    '123 Main Street, City, State 12345',
    100.00, 10.00, 90.00, 3,
    'Active'
) ON CONFLICT (sale_number) DO NOTHING;

-- Insert sample sale items
INSERT INTO sale_items (
    sale_id, 
    product_id, product_name, product_category,
    unit_price_amount, quantity, line_total_amount
) SELECT 
    s.id,
    'c0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'::UUID,
    'Programming Book',
    'Books',
    29.99, 2, 59.98
FROM sales s 
WHERE s.sale_number = 'SALE-001'
ON CONFLICT DO NOTHING;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Verify schema creation
SELECT 
    table_schema, 
    table_name, 
    table_type 
FROM information_schema.tables 
WHERE table_schema = 'developerstore' 
ORDER BY table_name;

-- Verify constraints
SELECT 
    conname as constraint_name,
    contype as constraint_type,
    pg_get_constraintdef(oid) as definition
FROM pg_constraint 
WHERE connamespace = (
    SELECT oid FROM pg_namespace WHERE nspname = 'developerstore'
)
ORDER BY conname;

PRINT 'Database schema created successfully!';
