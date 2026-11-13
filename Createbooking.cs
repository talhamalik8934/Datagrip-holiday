using MySqlConnector;

namespace HolidayMaker;

public class CreateBooking
{
    // This method is used to check availability and create a new booking.
    public async Task CBooking(int customer_id, int room_id, string checkIn_d, string checkOut_d, int _guests)
    {
        using var conn= await Database.GetConnectionAsync();

        // Check if the room exists and get its price and capacity.
        string roomSql = @"SELECT price_per_night, capacity FROM rooms WHERE id = @room_id;";
        await using var roomCmd = new MySqlCommand(roomSql, conn);
        roomCmd.Parameters.AddWithValue("@room_id", room_id);
        await using var roomReader = await roomCmd.ExecuteReaderAsync();

        if (!await roomReader.ReadAsync())
        {
            Console.WriteLine("Invalid room id!!");
            return;
        }


        decimal pricePerNight = roomReader.GetDecimal("price_per_night");
        int maxCapacity = roomReader.GetInt32("capacity");
        await roomReader.CloseAsync();

        if (_guests > maxCapacity)
        {
            Console.WriteLine($"Too many guests! Max capacity for this room is {maxCapacity}.");
            return;
        }

        // Re check room availability to prevent double bookings.
        string overlapSql = @"
            SELECT COUNT(*) FROM bookings
            WHERE room_id = @room_id
              AND check_in < @checkOut_d
              AND check_out > @checkIn_d;";
        await using var overlapCmd = new MySqlCommand(overlapSql, conn);
        overlapCmd.Parameters.AddWithValue("@room_id", room_id);
        overlapCmd.Parameters.AddWithValue("@checkIn_d", checkIn_d);
        overlapCmd.Parameters.AddWithValue("@checkOut_d", checkOut_d);

        int conflicts = Convert.ToInt32(await overlapCmd.ExecuteScalarAsync());
        if (conflicts > 0)
        {
            Console.WriteLine("Booking failed! This room is already booked for those dates.");
            return;
        }

        // Calculate total price based on number of nights.
        int nights = (DateTime.Parse(checkOut_d) - DateTime.Parse(checkIn_d)).Days;
        decimal totalPrice = nights * pricePerNight;

        string insertSql=@"
        INSERT INTO bookings (customer_id, room_id, check_in, check_out, amount_guests, total_price)
        VALUES (@customer_id, @room_id, @checkIn_d, @checkOut_d, @_guests, @total_price);
        SELECT LAST_INSERT_ID();";
        
        await using var insertCmd = new MySqlCommand(insertSql,conn);
        insertCmd.Parameters.AddWithValue("@customer_id", customer_id);
        insertCmd.Parameters.AddWithValue("@room_id", room_id);
        insertCmd.Parameters.AddWithValue("@checkIn_d", checkIn_d);
        insertCmd.Parameters.AddWithValue("@checkOut_d", checkOut_d);
        insertCmd.Parameters.AddWithValue("@_guests", _guests);
        insertCmd.Parameters.AddWithValue("@total_price", totalPrice);

        int bookingId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
        Console.WriteLine($"Booking created!! Booking ID: {bookingId} | Nights: {nights} | Total price: {totalPrice} SEK");

    }
}