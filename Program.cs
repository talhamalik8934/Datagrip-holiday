using HolidayMaker;

await using var testConn = await Database.GetConnectionAsync();
Console.WriteLine("Database Connected!");

HolidayMakerService service = new HolidayMakerService();
var customerService = new CustomerService();
int? selectedCustomerId = null;
bool running = true;

while (running)
{
    Console.WriteLine("\n=== HOLIDAY MAKER ===");
    Console.WriteLine("1. Search available rooms");
    Console.WriteLine("2. Create a booking");
    Console.WriteLine("3. View customer bookings");
    Console.WriteLine("4. Change number of guests in booking");
    Console.WriteLine("5. Cancel a booking");
    Console.WriteLine("6. Customer management");
    Console.WriteLine("0. Exit");
    Console.Write("\nSelect an option (0-6): ");

    string input = Console.ReadLine()!;

    switch (input)
    {
        case "1":
            // Query 1 Call
        Console.WriteLine("\n--- Search available rooms ---");
        Console.Write("Enter city: ");
        string city = Console.ReadLine()!;

        Console.Write("Ange antal personer: ");
        int guests = int.Parse(Console.ReadLine()!);

        Console.Write("Ange incheckning (YYYY-MM-DD): ");
        string checkIn = Console.ReadLine()!;

        Console.Write("Ange utcheckning (YYYY-MM-DD): ");
        string checkOut = Console.ReadLine()!;

        await service.SearchAvailableRooms(city, guests, checkIn, checkOut);
        break;

        case "2":
        Console.WriteLine("\n--- CREATE A BOOKING ---");

        Console.Write("Enter Customer ID: ");
        int cId = int.Parse(Console.ReadLine()!);

        Console.Write("Enter Room ID: ");
        int rId = int.Parse(Console.ReadLine()!);

        Console.Write("Enter Check-In (YYYY-MM-DD): ");
        string cIn = Console.ReadLine()!;

        Console.Write("Enter Check-Out (YYYY-MM-DD): ");
        string cOut = Console.ReadLine()!;

        Console.Write("Enter Number of Guests: ");
        int gCount = int.Parse(Console.ReadLine()!);

        await service.CreateBooking(cId, rId, cIn, cOut, gCount);
        break;

        case "3":

        Console.WriteLine("\n--- View Bookings ---");
        Console.WriteLine("Enter customer id: ");
        int customerid=int.Parse(Console.ReadLine()!);

        await service.ViewBooking(customerid);
        break;

        case "4":
        Console.WriteLine("\n--- UPDATE GUEST COUNT ---");
        Console.Write("Enter Booking ID: ");
        int updateBookingId = int.Parse(Console.ReadLine()!);

        Console.Write("Enter New Number of Guests: ");
        int newGuestCount = int.Parse(Console.ReadLine()!);

        await service.UpdateGuestCount(updateBookingId, newGuestCount);
        break;

        case "5":
        Console.WriteLine("\n--- Cancel Booking ---");
        Console.Write("Enter Booking ID: ");
        int deleteBookingId = int.Parse(Console.ReadLine()!);

        await service.CancelBooking(deleteBookingId);
        break;

        case "6":
        Console.WriteLine("\n--- CUSTOMER MANAGEMENT ---");
        Console.WriteLine("1. Register New Customer");
        Console.WriteLine("2. Select Existing Customer");
        Console.Write("Choose option: ");
        string subChoice = Console.ReadLine() ?? "";

        if (subChoice == "1")
        {
            Console.Write("Enter Name: ");
            string? name = Console.ReadLine();

            Console.Write("Enter Email: ");
            string? email = Console.ReadLine();

            Console.Write("Enter Phone Number: ");
            string? phone = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
            {
            Console.WriteLine("Name, email, and phone number cannot be empty.");
            }
            else
            {
            selectedCustomerId = await customerService.RegisterCustomer(name, email, phone);
            }

        }
        else if (subChoice == "2")
        {
            await customerService.ListCustomers();
            Console.Write("\nEnter Customer ID to select: ");
            if (int.TryParse(Console.ReadLine(), out int chosenId))
            {
            if (await customerService.CustomerExists(chosenId))
            {
                selectedCustomerId = chosenId;
                Console.WriteLine($"Active customer set to ID: {selectedCustomerId}");
            }
            else
            {
                Console.WriteLine($"Customer ID {chosenId} does not exist,\n");
            }
            }
        }
        break;

        
        case "0":
            running = false;
            Console.WriteLine("BYE");
            break;
        default:
            Console.WriteLine("Invalid choice");
            break;
    }
}
