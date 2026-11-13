using MySqlConnector;

namespace HolidayMaker;

public class CustomerService
{
    // Register new customer and return generated ID.
    public async Task<int> RegisterCustomer(string name, string email, string phoneNumber)
    {
        using var conn = await Database.GetConnectionAsync();

        string sql = @"INSERT INTO customers (name, email, phone_number)
                       VALUES (@name, @email, @phoneNumber);
                       SELECT LAST_INSERT_ID();";

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);

        object? result = await cmd.ExecuteScalarAsync();
        int newCustomerId = Convert.ToInt32(result);

        Console.WriteLine($"\n Customer registered! ID: {newCustomerId} | Name: {name}");
        return newCustomerId;
    }

    // This method is used to list all existing customers in the database.
    public async Task ListCustomers()
    {
        using var conn = await Database.GetConnectionAsync();

        string sql = @"SELECT id, name, email, phone_number FROM customers ORDER BY id ASC;";
        await using var cmd = new MySqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        Console.WriteLine("\n--- EXISTING CUSTOMERS ---");
        bool found = false;

        while (await reader.ReadAsync())
        {
            found = true;
            Console.WriteLine($"[ID: {reader.GetInt32("id")}] {reader.GetString("name")} | Email: {reader.GetString("email")} | Phone: {reader.GetString("phone_number")}");
        }

        if (!found)
        {
            Console.WriteLine("No customers found in the database.");
        }

    }

    // Verify if an entered Customer ID actually exists before selecting.
    public async Task<bool> CustomerExists(int customerId)
    {
        using var conn = await Database.GetConnectionAsync();

        string sql = @"SELECT COUNT(*) FROM customers WHERE id = @id;";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", customerId);

        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return count > 0;
    }


}