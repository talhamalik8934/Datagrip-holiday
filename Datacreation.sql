use holiday_maker;

--
SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS bookings;
DROP TABLE IF EXISTS customers;
DROP TABLE IF EXISTS rooms;
DROP TABLE IF EXISTS hotels;
SET FOREIGN_KEY_CHECKS = 1;

-- Table for hotels
CREATE table hotels(
    id INT AUTO_INCREMENT primary key,
    name VARCHAR(100) not null,
    city VARCHAR(100) not null,
    pool boolean not null default false,
    restaurant boolean not null default false,
    distance_beach int not null,
    distance_center int not null,
    rating decimal(2,1) check (rating between 1.0 and 5.0)
);

-- Table for rooms
CREATE table rooms(
    id int auto_increment primary key,
    hotel_id int not null,
    room_number varchar(10) not null,
    capacity int not null check ( capacity>0 ),
    price_per_night decimal(10,2) not null check ( price_per_night>0 ),
    foreign key (hotel_id) references hotels(id) on delete cascade
);

-- Table for customers
CREATE table customers(
    id int auto_increment primary key,
    name varchar(100) not null,
    email varchar(100) not null unique,
    phone_number varchar(100) not null
);

-- Table for bookings
CREATE table bookings(
    id int auto_increment primary key,
    customer_id int not null,
    room_id int not null,
    check_in date not null,
    check_out date not null,
    amount_guests int not null check ( amount_guests>0 ),
    total_price decimal(10,2) not null,
    foreign key (customer_id) references customers(id) on delete cascade ,
    foreign key  (room_id) references rooms(id) on delete cascade,
    CONSTRAINT chk_date check ( check_out>check_in )
);