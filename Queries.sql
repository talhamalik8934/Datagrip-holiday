-- Query 1: Search Rooms

SELECT
    h.name AS hotel_name,
    h.city,
    r.id AS room_id,
    r.room_number,
    r.capacity,
    r.price_per_night,
    h.rating
FROM rooms r
JOIN hotels h on r.hotel_id = h.id
WHERE h.city = 'Malmö'
  AND r.capacity >= 2
  -- filter to not show
  AND r.id NOT IN (
      SELECT room_id
      FROM bookings
      WHERE check_in < '2026-12-15'
        AND check_out > '2026-12-10'
  )
ORDER BY r.price_per_night ASC;

CREATE VIEW v_available_room as SELECT
    h.name AS hotel_name,
    h.city,
    r.id AS room_id,
    r.room_number,
    r.capacity,
    r.price_per_night,
    h.rating
FROM rooms r
JOIN hotels h on r.hotel_id = h.id;

-- Query 2: Create booking

INSERT INTO bookings (customer_id, room_id, check_in, check_out, amount_guests, total_price)
VALUES (1, 3, '2026-12-25', '2026-12-28', 2, 8100.00);

-- Test
select * from bookings where customer_id=1
and check_in='2026-12-25';


-- Query 3: View booking

SELECT
    b.id AS booking_id,
    h.name AS hotel_name,
    c.name as customer_name,
    h.city,
    r.room_number,
    b.check_in,
    b.check_out,
    b.amount_guests,
    b.total_price
FROM bookings b
JOIN rooms r ON b.room_id = r.id
JOIN hotels h ON r.hotel_id = h.id
JOIN customers c on b.customer_id=c.id
WHERE b.customer_id = 1;

CREATE or replace VIEW v_customer_booking as
    SELECT
    b.id AS booking_id,
    h.name AS hotel_name,
    b.customer_id,
    c.name as customer_name,
    h.city,
    r.room_number,
    b.check_in,
    b.check_out,
    b.amount_guests,
    b.total_price
FROM bookings b
JOIN rooms r ON b.room_id = r.id
JOIN hotels h ON r.hotel_id = h.id
JOIN customers c on b.customer_id=c.id;


-- Query 4: Update booking

UPDATE bookings
SET amount_guests = 3
WHERE id = 1
  AND 3 <= (SELECT capacity FROM rooms WHERE id = room_id);



-- Query 5: Delete booking

DELETE FROM bookings
       where id=1;

SELECT * FROM bookings WHERE id = 1;

INSERT INTO bookings (id, customer_id, room_id, check_in, check_out, amount_guests, total_price)
VALUES (1, 1, 1, '2026-12-10', '2026-12-15', 2, 5000);

-- Query 6: Register customer

INSERT INTO customers (name, email, phone_number)
VALUES ('Thorfinn Karlsefni', 'thorfinn@email.se', '0705556677');