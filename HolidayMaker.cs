using MySqlConnector;

namespace HolidayMaker;

public class HolidayMakerService
{
    // Method for the first Query: Search Rooms.
    public async Task SearchAvailableRooms(string city, int guests, string checkIn, string checkOut)
    {
        await using var conn= await Database.GetConnectionAsync();
        

        string sql= @"
        SELECT hotel_name, city, room_id, room_number, capacity, price_per_night, rating
        FROM v_available_room
        WHERE city = @city
        AND capacity >= @guests
        AND room_id NOT IN (
        SELECT room_id
        FROM bookings
        WHERE check_in < @checkOut
        AND check_out > @checkIn
        )
        ORDER BY price_per_night ASC;";

        await using var prm= new MySqlCommand(sql,conn);
        prm.Parameters.AddWithValue("@city",city);
        prm.Parameters.AddWithValue("@guests",guests);
        prm.Parameters.AddWithValue("@checkOut",checkOut);
        prm.Parameters.AddWithValue("@checkIn",checkIn);

        
        await using var read= await prm.ExecuteReaderAsync();
        Console.WriteLine("\n--- FOUND ROOMS ---");
        bool foundAny = false;

        while (await read.ReadAsync())
        {
            foundAny = true;
            string hotelName = read.GetString("hotel_name");
            string roomNum = read.GetString("room_number");
            int capacity = read.GetInt32("capacity");
            decimal price = read.GetDecimal("price_per_night");
            decimal rating = read.GetDecimal("rating");

            Console.WriteLine($"Hotel: {hotelName} | Room: {roomNum} | Capacity: {capacity} | Price: {price} SEK/night | Rating: {rating}/5");
        }

        if (!foundAny)
    {
        Console.WriteLine("Filtered rooms not found");
    }

    }

    public async Task CreateBooking(int customer_id, int room_id, string checkIn_d, string checkOut_d, int _guests)
    {
        using var conn= await Database.GetConnectionAsync();

        string priceSql=@"SELECT price_per_night FROM rooms WHERE id=@room_id;"; 
        await using var priceCmd= new MySqlCommand(priceSql,conn);
        priceCmd.Parameters.AddWithValue("@room_id",room_id);
        object? result= await priceCmd.ExecuteScalarAsync();

        if(result==null)
        {
            Console.WriteLine("Invalid room id!!");
            return;
        }

        decimal pricePerNight= Convert.ToDecimal(result);
        int nights= ((DateTime.Parse(checkOut_d))-(DateTime.Parse(checkIn_d))).Days;
        Decimal totalPrice= nights*pricePerNight;

        string insertSql=@"
        INSERT INTO bookings (customer_id, room_id, check_in, check_out, amount_guests, total_price)
        VALUES (@customer_id, @room_id, @checkIn_d, @checkOut_d, @_guests, @total_price);";
        
        await using var insertCmd = new MySqlCommand(insertSql,conn);
        insertCmd.Parameters.AddWithValue("@customer_id", customer_id);
        insertCmd.Parameters.AddWithValue("@room_id", room_id);
        insertCmd.Parameters.AddWithValue("@checkIn_d", checkIn_d);
        insertCmd.Parameters.AddWithValue("@checkOut_d", checkOut_d);
        insertCmd.Parameters.AddWithValue("@_guests", _guests);
        insertCmd.Parameters.AddWithValue("@total_price", totalPrice);

        await insertCmd.ExecuteNonQueryAsync();
        Console.WriteLine($"Booking created!! Nights: {nights} || Total price: {totalPrice} SEK");

    }

    public async Task ViewBooking(int customer_id_b)
    {
        await using var conn= await Database.GetConnectionAsync();

        string vBSql=@"
        SELECT * FROM v_customer_booking
        WHERE customer_id= @customer_id
        ORDER BY check_in ASC; ";

        await using var cmd = new MySqlCommand(vBSql, conn);
        cmd.Parameters.AddWithValue("@customer_id", customer_id_b);

        await using var reader = await cmd.ExecuteReaderAsync();

        Console.WriteLine("\n----Booking List----");
        bool foundBooking=false;

        while (await reader.ReadAsync())
        {
            foundBooking=true;
            int bookingId = reader.GetInt32("booking_id");
            string customerName= reader.GetString("customer_name");
            string hotelName = reader.GetString("hotel_name");
            string roomNum = reader.GetString("room_number");
            string checkIn = reader.GetDateTime("check_in").ToString("yyyy-MM-dd");
            string checkOut = reader.GetDateTime("check_out").ToString("yyyy-MM-dd");
            int guests = reader.GetInt32("amount_guests");
            decimal totalPrice = reader.GetDecimal("total_price");
            
            Console.WriteLine($"Booking id:{bookingId} | Customer: {customerName} | Hotel: {hotelName} | Room: {roomNum} | Dates: {checkIn} to {checkOut} | Guests: {guests} | Total: {totalPrice} SEK");

        }

        if(!foundBooking)
        {
            Console.WriteLine("No booking found for this customer");
        }


    }

    public async Task UpdateGuestCount(int bookingId, int newGuests)
    {
        await using var conn= await Database.GetConnectionAsync();
        
        string updateSql=@"
        UPDATE bookings
        SET amount_guests = @newGuests
        WHERE id = @bookingId
        AND @newGuests <= (SELECT capacity FROM rooms WHERE id = room_id);";

        await using var cmd = new MySqlCommand(updateSql, conn);
        cmd.Parameters.AddWithValue("@newGuests", newGuests);
        cmd.Parameters.AddWithValue("@bookingId", bookingId);

        int affectedrow= await cmd.ExecuteNonQueryAsync();

        if (affectedrow > 0)
    {
        Console.WriteLine($"Booking id: {bookingId} updated successfully! New guest count: {newGuests}");
    }
    else
    {
        Console.WriteLine($"Update failed. Booking id: {bookingId} doesn't exist, or room capacity is too small");
    }
    }

    public async Task CancelBooking(int bookingId)
    {
        using var conn = await Database.GetConnectionAsync();

        string deleteSql = @"DELETE FROM bookings WHERE id = @bookingId;";

        await using var cmd = new MySqlCommand(deleteSql, conn);
        cmd.Parameters.AddWithValue("@bookingId", bookingId);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();

        if (rowsAffected > 0)
        {
        Console.WriteLine($"\n Booking id: {bookingId} has been successfully canceled.");
        }
        else
        {
        Console.WriteLine($"\n Booking id: {bookingId} not found.");
        }
    }

}