using MySqlConnector;

namespace HolidayMaker;

public class HolidayMakerService
{
    // Method for the first Query = Search Rooms.
    // It is used to search available rooms with optional filters and sorting.
    public async Task SearchAvailableRooms(string city, int guests, string checkIn, string checkOut,bool? requirePool = null,
    bool? requireRestaurant = null,
    int? maxBeachDistance = null,
    int? maxCenterDistance = null,
    string sortBy = "price")
    {
        await using var conn= await Database.GetConnectionAsync();
        
        // Base query using the view and excluding rooms with overlapping bookings.
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
        )";

        await using var prm= new MySqlCommand(sql,conn);
        prm.Parameters.AddWithValue("@city",city);
        prm.Parameters.AddWithValue("@guests",guests);
        prm.Parameters.AddWithValue("@checkOut",checkOut);
        prm.Parameters.AddWithValue("@checkIn",checkIn);

        if (requirePool == true)
        {
            sql += " AND pool = TRUE";
        }

        if (requireRestaurant == true)
        {
            sql += " AND restaurant = TRUE";
        }

        if (maxBeachDistance.HasValue)
        {
            sql += " AND distance_beach <= @maxBeach";
            prm.Parameters.AddWithValue("@maxBeach", maxBeachDistance.Value);
        }

        if (maxCenterDistance.HasValue)
        {
            sql += " AND distance_center <= @maxCenter";
            prm.Parameters.AddWithValue("@maxCenter", maxCenterDistance.Value);
        }

        if (sortBy == "rating")
        {
            sql += " ORDER BY rating DESC, price_per_night ASC;";
        }
        else
        {
            sql += " ORDER BY price_per_night ASC;";
        }

        prm.CommandText = sql;

        
        await using var read= await prm.ExecuteReaderAsync();
        Console.WriteLine("\n--- FOUND ROOMS ---");
        bool foundAny = false;

        while (await read.ReadAsync())
        {
            foundAny = true;
            int roomId = read.GetInt32("room_id");
            string hotelName = read.GetString("hotel_name");
            string roomNum = read.GetString("room_number");
            int capacity = read.GetInt32("capacity");
            decimal price = read.GetDecimal("price_per_night");
            decimal rating = read.GetDecimal("rating");

            Console.WriteLine($"ID: {roomId} | Hotel: {hotelName} | Room: {roomNum} | Capacity: {capacity} | Price: {price} SEK/night | Rating: {rating}/5");
        }

        if (!foundAny)
    {
        Console.WriteLine("Filtered rooms not found");
    }

    }

    
    // This method is used to show all bookings for a specific customer.
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


}