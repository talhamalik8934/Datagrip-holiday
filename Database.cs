using MySqlConnector;

namespace HolidayMaker;

public static class Database
{
    private static string connectionString= "Server=localhost;Database=holiday_maker;User ID=root;Password=kurslosenord";

    public static async Task<MySqlConnection> GetConnectionAsync()
    {
        var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();
        return conn;
    }
} 