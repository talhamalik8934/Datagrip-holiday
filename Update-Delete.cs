using MySqlConnector;
namespace HolidayMaker;

public class updateDelete
{   
        // This method is used to update the guest count if the room capacity allows it.
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

    // This method is used to cancel and remove a booking from the database.
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