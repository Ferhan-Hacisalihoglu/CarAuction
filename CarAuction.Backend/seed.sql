-- =============================================================================
-- CarAuction Comprehensive Seed Data (seed.sql)
-- Populates all tables with realistic cars, auctions, users, bids, clubs & chat
-- =============================================================================

BEGIN;

-- 1. Roles & Permissions Junction
INSERT INTO roles (name) VALUES ('Admin'), ('User') ON CONFLICT (name) DO NOTHING;

INSERT INTO permissions (name, description) VALUES
  ('auction.bid', 'Ability to place bids on live auctions'),
  ('listing.create', 'Ability to create vehicle listings'),
  ('admin.manage', 'Access to administrative panel and user moderation')
ON CONFLICT (name) DO NOTHING;

-- Map Admin to all permissions (1, 2, 3) and User to (1, 2)
INSERT INTO role_permissions (role_id, permission_id) VALUES
  (1, 1), (1, 2), (1, 3),
  (2, 1), (2, 2)
ON CONFLICT DO NOTHING;

-- 2. Users
-- Passwords:
-- Admin: admin@carauction.com -> Admin123!
-- Users: Password123!
INSERT INTO users (id, first_name, last_name, email, password_hash, salt, role_id, is_active, created_at)
VALUES
  (1, 'Admin', 'System', 'admin@carauction.com',
   'CnRQSDxWY6S+q8VlTv0ihy+zYx/wzuACgxJQc/XCfyX8OSmr4MJ2xIzNvCR+9Y1Le0eCgLgnj+T50EbeiqU0ZA==',
   'dog/2eVcuZtfW7xBdBEVnCXWAb0uWcz40l3J0c4VkmCFYQnIsIfDEs72nMGP7jkab8qtfa8snkWf6cXm/rb4Ew==',
   1, TRUE, NOW() - INTERVAL '30 days'),

  (2, 'Michael', 'Schumacher', 'seller@carauction.com',
   'zNgR7QY9Z6xoxBdvWCWRE7qGkkux8I75hQNNNKk1LkCGj64CJxubOo5SpVd5LZb0nGzc2Agkh2s5KANMYkaHGg==',
   'dog/2eVcuZtfW7xBdBEVnCXWAb0uWcz40l3J0c4VkmCFYQnIsIfDEs72nMGP7jkab8qtfa8snkWf6cXm/rb4Ew==',
   2, TRUE, NOW() - INTERVAL '25 days'),

  (3, 'Sarah', 'Connor', 'bidder@carauction.com',
   'zNgR7QY9Z6xoxBdvWCWRE7qGkkux8I75hQNNNKk1LkCGj64CJxubOo5SpVd5LZb0nGzc2Agkh2s5KANMYkaHGg==',
   'dog/2eVcuZtfW7xBdBEVnCXWAb0uWcz40l3J0c4VkmCFYQnIsIfDEs72nMGP7jkab8qtfa8snkWf6cXm/rb4Ew==',
   2, TRUE, NOW() - INTERVAL '20 days'),

  (4, 'David', 'Beckham', 'collector@carauction.com',
   'zNgR7QY9Z6xoxBdvWCWRE7qGkkux8I75hQNNNKk1LkCGj64CJxubOo5SpVd5LZb0nGzc2Agkh2s5KANMYkaHGg==',
   'dog/2eVcuZtfW7xBdBEVnCXWAb0uWcz40l3J0c4VkmCFYQnIsIfDEs72nMGP7jkab8qtfa8snkWf6cXm/rb4Ew==',
   2, TRUE, NOW() - INTERVAL '15 days')
ON CONFLICT (id) DO UPDATE SET
  password_hash = EXCLUDED.password_hash,
  salt = EXCLUDED.salt,
  role_id = EXCLUDED.role_id;

SELECT setval('users_id_seq', (SELECT GREATEST(MAX(id), 1) FROM users));

-- 3. Listings (10 Cars corresponding to SeedData 01.png - 10.png)
INSERT INTO listings (id, user_id, title, description, price, is_auction, status, created_at)
VALUES
  (1, 2, '2023 Porsche 911 GT3 RS (Weissach Package)',
   'Finished in GT Silver Metallic with exposed carbon Weissach package. 4.0L Naturally Aspirated Flat-6 producing 518 hp screaming to a 9,000 RPM redline. Front axle lift system, Porsche Ceramic Composite Brakes (PCCB), magnesium lightweight wheels, full carbon bucket seats trimmed in black leather and Race-Tex with Guards Red contrast accents.',
   345000.00, TRUE, 'active', NOW() - INTERVAL '4 days'),

  (2, 4, '2022 Ferrari F8 Tributo',
   'Finished in timeless Rosso Corsa over Nero full-grain leather with Giallo contrast stitching. Powered by a 3.9L Twin-Turbocharged V8 producing 710 hp. Factory options include Carbon Fiber Driver Zone with LED steering wheel, Scuderia Ferrari fender shields, titanium sport exhaust, and passenger telemetry display.',
   380000.00, TRUE, 'active', NOW() - INTERVAL '3 days'),

  (3, 2, '2024 Lamborghini Revuelto V12 Hybrid',
   'The revolutionary flagship High Performance Electrified Vehicle (HPEV) from Sant''Agata Bolognese. Mid-mounted 6.5L naturally aspirated V12 engine coupled with three electric motors generating a mind-bending 1,001 hp. Giallo Auge pearl exterior, full carbon aerodynamic kit, bespoke Ad Personam two-tone cockpit.',
   620000.00, TRUE, 'active', NOW() - INTERVAL '5 days'),

  (4, 4, '2023 McLaren 765LT Spider',
   'Numbered example (1 of only 765 worldwide). Finished in striking Papaya Spark with bespoke MSO Carbon Fiber Exterior Packs 1 & 2. 4.0L Twin-Turbo V8 pushing 755 hp and 800 Nm of torque. Quad titanium active exhaust system, ultra-lightweight forged alloy wheels, and carbon racing buckets.',
   495000.00, TRUE, 'active', NOW() - INTERVAL '2 days'),

  (5, 2, '2021 Aston Martin DBS Superleggera',
   'Onyx Black metallic over Chancellor Red hand-stitched Bridge of Weir leather. 5.2L Twin-Turbocharged V12 delivering 715 hp and 900 Nm. Equipped with Bang & Olufsen BeoSound premium audio, carbon ceramic braking system, and 21-inch forged diamond-turned alloy wheels.',
   285000.00, FALSE, 'active', NOW() - INTERVAL '6 days'),

  (6, 4, '2023 BMW M4 CSL (Competition Sport Lightweight)',
   'Frozen Brooklyn Grey Metallic with exposed carbon fiber hood and yellow motorsport laser headlights. 3.0L M TwinPower Turbo inline-6 producing 543 hp. Strict limited production of 1,000 units worldwide. M Carbon full bucket seats and titanium rear silencer.',
   155000.00, FALSE, 'active', NOW() - INTERVAL '7 days'),

  (7, 2, '2022 Mercedes-AMG GT Black Series',
   'Finished in exclusive Magmabeam with AMG Track Package. 4.0L Flat-Plane Crank Biturbo V8 pumping out 720 hp. Active carbon fiber aerodynamics including dual rear wing and adjustable front splitter. Nürburgring Nordschleife record-holding heritage with carbon ceramic brakes.',
   420000.00, TRUE, 'active', NOW() - INTERVAL '8 days'),

  (8, 4, '2024 Audi RS6 Avant GT Limited Edition',
   'Ultra-exclusive 1 of 660 units worldwide honoring the legendary Audi 90 quattro IMSA GTO. Arkona White with historical heritage racing decals, carbon fiber front fenders and hood, pass-through roof spoiler, and 621 hp twin-turbo 4.0L V8.',
   235000.00, FALSE, 'active', NOW() - INTERVAL '10 days'),

  (9, 2, '1969 Ford Mustang Boss 429 Concours',
   'Legendary NASCAR homologation muscle car. Numbers-matching 429ci semi-hemi V8 engine with factory close-ratio 4-speed manual transmission. Finished in Wimbledon White over Black Deluxe interior. Documented by Kar Kraft and Deluxe Marti Report, Concours gold restoration.',
   310000.00, TRUE, 'active', NOW() - INTERVAL '12 hours'),

  (10, 4, '2023 Chevrolet Corvette Z06 3LZ Z07',
   'Torch Red with carbon flash accents. 5.5L Naturally Aspirated Flat-Plane Crank LT6 V8 delivering 670 hp with an 8,600 RPM redline. Equipped with the full Z07 Performance Package, carbon fiber aero package, carbon ground effects, and Brembo carbon ceramic brakes.',
   165000.00, FALSE, 'active', NOW() - INTERVAL '12 days')
ON CONFLICT (id) DO UPDATE SET
  title = EXCLUDED.title,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  is_auction = EXCLUDED.is_auction,
  status = EXCLUDED.status;

SELECT setval('listings_id_seq', (SELECT GREATEST(MAX(id), 1) FROM listings));

-- 4. Images (Load binary image bytes from /tmp/SeedData/*.png)
DELETE FROM images WHERE listing_id BETWEEN 1 AND 10;

INSERT INTO images (id, listing_id, image_data, file_name, mime_type, uploaded_at)
VALUES
  (1, 1, pg_read_binary_file('/tmp/SeedData/01.png'), 'porsche-gt3rs.png', 'image/png', NOW() - INTERVAL '4 days'),
  (2, 2, pg_read_binary_file('/tmp/SeedData/02.png'), 'ferrari-f8.png', 'image/png', NOW() - INTERVAL '3 days'),
  (3, 3, pg_read_binary_file('/tmp/SeedData/03.png'), 'lamborghini-revuelto.png', 'image/png', NOW() - INTERVAL '5 days'),
  (4, 4, pg_read_binary_file('/tmp/SeedData/04.png'), 'mclaren-765lt.png', 'image/png', NOW() - INTERVAL '2 days'),
  (5, 5, pg_read_binary_file('/tmp/SeedData/05.png'), 'aston-martin-dbs.png', 'image/png', NOW() - INTERVAL '6 days'),
  (6, 6, pg_read_binary_file('/tmp/SeedData/06.png'), 'bmw-m4-csl.png', 'image/png', NOW() - INTERVAL '7 days'),
  (7, 7, pg_read_binary_file('/tmp/SeedData/07.png'), 'amg-gt-black-series.png', 'image/png', NOW() - INTERVAL '8 days'),
  (8, 8, pg_read_binary_file('/tmp/SeedData/08.png'), 'audi-rs6-gt.png', 'image/png', NOW() - INTERVAL '10 days'),
  (9, 9, pg_read_binary_file('/tmp/SeedData/09.png'), 'ford-mustang-boss429.png', 'image/png', NOW() - INTERVAL '12 hours'),
  (10, 10, pg_read_binary_file('/tmp/SeedData/10.png'), 'corvette-z06.png', 'image/png', NOW() - INTERVAL '12 days')
ON CONFLICT (id) DO UPDATE SET
  image_data = EXCLUDED.image_data,
  file_name = EXCLUDED.file_name,
  mime_type = EXCLUDED.mime_type;

SELECT setval('images_id_seq', (SELECT GREATEST(MAX(id), 1) FROM images));

-- 5. Auctions (For listings 1, 2, 3, 4, 7, 9)
INSERT INTO auctions (id, listing_id, starting_price, current_price, start_time, end_time, min_bid_increment, winner_user_id, status)
VALUES
  (1, 1, 300000.00, 345000.00, NOW() - INTERVAL '2 days', NOW() + INTERVAL '3 days', 2500.00, 3, 'active'),
  (2, 2, 330000.00, 380000.00, NOW() - INTERVAL '1 day', NOW() + INTERVAL '4 days', 5000.00, 3, 'active'),
  (3, 3, 550000.00, 620000.00, NOW() - INTERVAL '3 days', NOW() + INTERVAL '5 days', 5000.00, 4, 'active'),
  (4, 4, 440000.00, 495000.00, NOW() - INTERVAL '1 day', NOW() + INTERVAL '2 days', 5000.00, 3, 'active'),
  (7, 7, 380000.00, 420000.00, NOW() - INTERVAL '2 days', NOW() + INTERVAL '6 days', 5000.00, 4, 'active'),
  (9, 9, 270000.00, 310000.00, NOW() - INTERVAL '1 day', NOW() + INTERVAL '4 hours', 2500.00, 3, 'active')
ON CONFLICT (listing_id) DO UPDATE SET
  starting_price = EXCLUDED.starting_price,
  current_price = EXCLUDED.current_price,
  start_time = EXCLUDED.start_time,
  end_time = EXCLUDED.end_time,
  min_bid_increment = EXCLUDED.min_bid_increment,
  winner_user_id = EXCLUDED.winner_user_id,
  status = EXCLUDED.status;

SELECT setval('auctions_id_seq', (SELECT GREATEST(MAX(id), 1) FROM auctions));

-- 6. Bids & Offers
DELETE FROM bids WHERE listing_id IN (1, 2, 3, 4, 5, 7, 9);

INSERT INTO bids (listing_id, user_id, amount, idempotency_key, created_at)
VALUES
  -- Bids on Porsche GT3 RS (Listing 1)
  (1, 4, 315000.00, 'seed-bid-1-1', NOW() - INTERVAL '36 hours'),
  (1, 3, 330000.00, 'seed-bid-1-2', NOW() - INTERVAL '24 hours'),
  (1, 4, 340000.00, 'seed-bid-1-3', NOW() - INTERVAL '12 hours'),
  (1, 3, 345000.00, 'seed-bid-1-4', NOW() - INTERVAL '2 hours'),

  -- Bids on Ferrari F8 (Listing 2)
  (2, 3, 350000.00, 'seed-bid-2-1', NOW() - INTERVAL '20 hours'),
  (2, 2, 365000.00, 'seed-bid-2-2', NOW() - INTERVAL '14 hours'),
  (2, 3, 380000.00, 'seed-bid-2-3', NOW() - INTERVAL '3 hours'),

  -- Bids on Revuelto (Listing 3)
  (3, 3, 580000.00, 'seed-bid-3-1', NOW() - INTERVAL '40 hours'),
  (3, 4, 620000.00, 'seed-bid-3-2', NOW() - INTERVAL '18 hours'),

  -- Bids on McLaren 765LT (Listing 4)
  (4, 3, 460000.00, 'seed-bid-4-1', NOW() - INTERVAL '16 hours'),
  (4, 2, 480000.00, 'seed-bid-4-2', NOW() - INTERVAL '8 hours'),
  (4, 3, 495000.00, 'seed-bid-4-3', NOW() - INTERVAL '1 hour'),

  -- Private Offer on Listing 5 (Aston Martin DBS - Direct Sale)
  (5, 3, 275000.00, 'seed-offer-5-1', NOW() - INTERVAL '5 hours'),

  -- Bids on AMG GT Black Series (Listing 7)
  (7, 3, 400000.00, 'seed-bid-7-1', NOW() - INTERVAL '30 hours'),
  (7, 4, 420000.00, 'seed-bid-7-2', NOW() - INTERVAL '6 hours'),

  -- Bids on Mustang Boss 429 (Listing 9 - Ending Soon!)
  (9, 4, 285000.00, 'seed-bid-9-1', NOW() - INTERVAL '18 hours'),
  (9, 3, 300000.00, 'seed-bid-9-2', NOW() - INTERVAL '5 hours'),
  (9, 4, 305000.00, 'seed-bid-9-3', NOW() - INTERVAL '2 hours'),
  (9, 3, 310000.00, 'seed-bid-9-4', NOW() - INTERVAL '30 minutes');

SELECT setval('bids_id_seq', (SELECT GREATEST(MAX(id), 1) FROM bids));

-- 7. Groups (Enthusiast Clubs)
INSERT INTO groups (id, name, description, created_by, created_at)
VALUES
  (1, 'Porsche Motorsport Syndicate',
   'Dedicated to GT-series owners, GT3 RS allocations, Nürburgring track setup specs, and Weissach aero telemetry.',
   2, NOW() - INTERVAL '20 days'),

  (2, 'V12 & Hypercar Collectors Lounge',
   'Private collector circle discussing bespoke Ferrari, Lamborghini, McLaren, and Koenigsegg allocations and deliveries.',
   4, NOW() - INTERVAL '18 days'),

  (3, 'Classic Muscle & Historic Legends',
   'Provenance verification, numbers-matching authentic restorations, Kar Kraft registry, and vintage auction analysis.',
   2, NOW() - INTERVAL '15 days')
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description;

SELECT setval('groups_id_seq', (SELECT GREATEST(MAX(id), 1) FROM groups));

-- 8. Group Members
INSERT INTO group_members (group_id, user_id, joined_at)
VALUES
  (1, 1, NOW() - INTERVAL '20 days'),
  (1, 2, NOW() - INTERVAL '20 days'),
  (1, 3, NOW() - INTERVAL '18 days'),
  (1, 4, NOW() - INTERVAL '15 days'),

  (2, 1, NOW() - INTERVAL '18 days'),
  (2, 2, NOW() - INTERVAL '17 days'),
  (2, 3, NOW() - INTERVAL '16 days'),
  (2, 4, NOW() - INTERVAL '18 days'),

  (3, 1, NOW() - INTERVAL '15 days'),
  (3, 2, NOW() - INTERVAL '15 days'),
  (3, 3, NOW() - INTERVAL '12 days')
ON CONFLICT (group_id, user_id) DO NOTHING;

-- 9. Group Messages
DELETE FROM group_messages WHERE group_id IN (1, 2, 3);

INSERT INTO group_messages (group_id, sender_id, content, sent_at)
VALUES
  -- Porsche Group
  (1, 2, 'Welcome everyone to the Porsche Motorsport channel! Just listed the 2023 GT3 RS Weissach on the auction floor.', NOW() - INTERVAL '2 days'),
  (1, 3, 'That GT Silver spec with magnesium wheels looks stunning in the gallery.', NOW() - INTERVAL '1 day'),
  (1, 4, 'Does it have the front axle lift system installed?', NOW() - INTERVAL '18 hours'),
  (1, 2, 'Yes David, factory front axle lift plus full PCCB ceramic brakes with zero track wear.', NOW() - INTERVAL '12 hours'),
  (1, 3, 'Just placed a bid on it. The aero package is unreal in person.', NOW() - INTERVAL '2 hours'),

  -- Hypercar Group
  (2, 4, 'The Lamborghini Revuelto V12 hybrid is on the live floor now. 1,001 hp is remarkable engineering.', NOW() - INTERVAL '3 days'),
  (2, 2, 'The Giallo Auge pearl paintwork under showroom lighting is pure art.', NOW() - INTERVAL '2 days'),
  (2, 3, 'Bidding is moving fast, already at $620k.', NOW() - INTERVAL '16 hours'),

  -- Classic Muscle Group
  (3, 2, 'The 1969 Boss 429 auction is in its final hours! Documented Kar Kraft numbers-matching.', NOW() - INTERVAL '5 hours'),
  (3, 3, 'I have inspected the Marti Report, provenance is 100% verified. Bidding now.', NOW() - INTERVAL '1 hour');

SELECT setval('group_messages_id_seq', (SELECT GREATEST(MAX(id), 1) FROM group_messages));

-- 10. Direct Conversations & Messages
INSERT INTO conversations (id, user1_id, user2_id, created_at)
VALUES (1, 2, 3, NOW() - INTERVAL '5 days')
ON CONFLICT (id) DO NOTHING;

SELECT setval('conversations_id_seq', (SELECT GREATEST(MAX(id), 1) FROM conversations));

DELETE FROM messages WHERE conversation_id = 1;

INSERT INTO messages (conversation_id, sender_id, content, sent_at, is_read)
VALUES
  (1, 3, 'Hello Michael, I submitted a private offer on the Aston Martin DBS Superleggera.', NOW() - INTERVAL '5 hours', TRUE),
  (1, 2, 'Hi Sarah! Thank you for the offer. The car is kept in a climate-controlled facility in pristine condition.', NOW() - INTERVAL '4 hours', TRUE),
  (1, 3, 'Are service records from the authorized dealer available for inspection?', NOW() - INTERVAL '3 hours', TRUE),
  (1, 2, 'Yes, full Aston Martin franchise records and original window sticker are ready. I will review your offer shortly!', NOW() - INTERVAL '2 hours', FALSE);

SELECT setval('messages_id_seq', (SELECT GREATEST(MAX(id), 1) FROM messages));

COMMIT;
