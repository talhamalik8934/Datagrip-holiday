using MySqlConnector;

namespace HolidayMaker;

public static class Database
{
    private static string connectionString= "Server=localhost;Database=holiday_maker;User ID=root;Password=kurslosenord";

    // This method is used to open and return an async MySQL connection.
    public static async Task<MySqlConnection> GetConnectionAsync()
    {
        var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();
        return conn;
    }
} 