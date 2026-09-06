-- =============================================================================
-- CarAuction Database Initialization Script (init.sql)
-- Strictly generated according to backend.md specifications
-- =============================================================================

-- 1. Roles Table
CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL
);

-- 2. Permissions Table
CREATE TABLE IF NOT EXISTS permissions (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT
);

-- 3. Role-Permission Junction Table (Many-to-Many)
CREATE TABLE IF NOT EXISTS role_permissions (
    role_id INTEGER REFERENCES roles(id) ON DELETE CASCADE,
    permission_id INTEGER REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);

-- 4. Users Table
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,         -- password hash (HMACSHA512)
    salt VARCHAR(255) NOT NULL,                  -- cryptographic salt
    role_id INTEGER REFERENCES roles(id) ON DELETE SET NULL,
    refresh_token VARCHAR(500),
    refresh_token_expiry TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- 5. Listings Table
CREATE TABLE IF NOT EXISTS listings (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(18,2),                         -- Upgraded to DECIMAL(18,2) for luxury/hypercar valuation
    is_auction BOOLEAN DEFAULT FALSE,            -- is auction listing?
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    status VARCHAR(50) DEFAULT 'active'          -- active, sold, expired, cancelled
);

-- 6. Auctions Table (1-to-1 with listings)
CREATE TABLE IF NOT EXISTS auctions (
    id SERIAL PRIMARY KEY,
    listing_id INTEGER UNIQUE NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
    starting_price DECIMAL(18,2) NOT NULL,
    current_price DECIMAL(18,2) NOT NULL,
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    min_bid_increment DECIMAL(18,2) DEFAULT 1.00,
    winner_user_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    status VARCHAR(50) DEFAULT 'active'          -- active, completed, expired, cancelled
);

-- 7. Bids Table (For regular listings or auctions)
CREATE TABLE IF NOT EXISTS bids (
    id SERIAL PRIMARY KEY,
    listing_id INTEGER NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    amount DECIMAL(18,2) NOT NULL,
    idempotency_key VARCHAR(100),               -- Client deduplication key (X-Idempotency-Key)
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- 8. Listing Images
CREATE TABLE IF NOT EXISTS images (
    id SERIAL PRIMARY KEY,
    listing_id INTEGER NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
    image_data BYTEA NOT NULL,                   -- binary image data
    file_name VARCHAR(255),
    mime_type VARCHAR(100),
    uploaded_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- 9. Groups
CREATE TABLE IF NOT EXISTS groups (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_by INTEGER REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- 10. Group Members (Many-to-Many)
CREATE TABLE IF NOT EXISTS group_members (
    group_id INTEGER REFERENCES groups(id) ON DELETE CASCADE,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    joined_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (group_id, user_id)
);

-- 11. Direct Message Conversations (Between two users)
CREATE TABLE IF NOT EXISTS conversations (
    id SERIAL PRIMARY KEY,
    user1_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    user2_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    CONSTRAINT check_distinct_conversation_users CHECK (user1_id != user2_id),
    CONSTRAINT unique_conversation_user_pair UNIQUE (user1_id, user2_id)
);

-- 12. Direct Message Contents
CREATE TABLE IF NOT EXISTS messages (
    id SERIAL PRIMARY KEY,
    conversation_id INTEGER NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    sender_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    is_read BOOLEAN DEFAULT FALSE
);

-- 13. Group Messages
CREATE TABLE IF NOT EXISTS group_messages (
    id SERIAL PRIMARY KEY,
    group_id INTEGER NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    sender_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =============================================================================
-- Performance Indexes (Section 2.14)
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_listings_user_id ON listings(user_id);
CREATE INDEX IF NOT EXISTS idx_listings_status ON listings(status);
CREATE INDEX IF NOT EXISTS idx_auctions_listing_id ON auctions(listing_id);
CREATE INDEX IF NOT EXISTS idx_auctions_end_time_status ON auctions(end_time, status);
CREATE INDEX IF NOT EXISTS idx_auctions_winner_user_id ON auctions(winner_user_id);
CREATE INDEX IF NOT EXISTS idx_bids_listing_id ON bids(listing_id);
CREATE INDEX IF NOT EXISTS idx_bids_user_id ON bids(user_id);
CREATE INDEX IF NOT EXISTS idx_bids_created_at ON bids(created_at DESC);
CREATE UNIQUE INDEX IF NOT EXISTS idx_bids_idempotency ON bids(idempotency_key) WHERE idempotency_key IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_images_listing_id ON images(listing_id);
CREATE INDEX IF NOT EXISTS idx_conversations_user_pair ON conversations(user1_id, user2_id);
CREATE INDEX IF NOT EXISTS idx_messages_conversation_id ON messages(conversation_id);
CREATE INDEX IF NOT EXISTS idx_messages_sent_at ON messages(sent_at ASC);
CREATE INDEX IF NOT EXISTS idx_group_messages_group_id ON group_messages(group_id);
CREATE INDEX IF NOT EXISTS idx_group_messages_sent_at ON group_messages(sent_at ASC);

-- =============================================================================
-- Initial Seeds (Section 2.15)
-- =============================================================================
INSERT INTO roles (name) VALUES ('Admin'), ('User') ON CONFLICT (name) DO NOTHING;

INSERT INTO permissions (name, description) VALUES
  ('auction.bid', 'Ability to place bids on live auctions'),
  ('listing.create', 'Ability to create vehicle listings'),
  ('admin.manage', 'Access to administrative panel and user moderation')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id) VALUES
  (1, 1), (1, 2), (1, 3),
  (2, 1), (2, 2)
ON CONFLICT DO NOTHING;

-- Default Administrative User (Password: Admin123!)
INSERT INTO users (id, first_name, last_name, email, password_hash, salt, role_id, is_active)
VALUES
  (1, 'Admin', 'System', 'admin@carauction.com',
   'CnRQSDxWY6S+q8VlTv0ihy+zYx/wzuACgxJQc/XCfyX8OSmr4MJ2xIzNvCR+9Y1Le0eCgLgnj+T50EbeiqU0ZA==',
   'dog/2eVcuZtfW7xBdBEVnCXWAb0uWcz40l3J0c4VkmCFYQnIsIfDEs72nMGP7jkab8qtfa8snkWf6cXm/rb4Ew==',
   1, TRUE)
ON CONFLICT (id) DO NOTHING;

SELECT setval('users_id_seq', (SELECT GREATEST(MAX(id), 1) FROM users));

