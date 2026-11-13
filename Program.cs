using HolidayMaker;

// Test database connection at startup.
await using var testConn = await Database.GetConnectionAsync();
Console.WriteLine("Database Connected!");

HolidayMakerService service = new HolidayMakerService();
var createBooking_= new CreateBooking();
var customerService = new CustomerService();
var ud_booking= new updateDelete();
int? selectedCustomerId = null;
bool running = true;

// Main menu loop for the Holiday Maker menu.
while (running)
{
    try
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
        
        Console.WriteLine("\n--- Search available rooms ---");
        Console.Write("Enter city: ");
        string city = Console.ReadLine()!;

        Console.Write("How many customers: ");
        int guests = int.Parse(Console.ReadLine()!);

        Console.Write("Enter chekin date (YYYY-MM-DD): ");
        string checkIn = Console.ReadLine()!;

        Console.Write("Enter checkout date (YYYY-MM-DD): ");
        string checkOut = Console.ReadLine()!;

        // Check that dates are valid and check-out is after check-in.
        if (!DateTime.TryParse(checkIn, out DateTime d1) || !DateTime.TryParse(checkOut, out DateTime d2) || d2 <= d1)
        {
            Console.WriteLine("Invalid dates! Check-out must be after check-in (YYYY-MM-DD).");
            break;
        }

        // Optional search filters (press Enter to skip).
        Console.Write("Require Pool? (y/n, Enter to skip): ");
        bool? requirePool = Console.ReadLine()?.ToLower() == "y" ? true : null;

        Console.Write("Require Restaurant? (y/n, Enter to skip): ");
        bool? requireRestaurant = Console.ReadLine()?.ToLower() == "y" ? true : null;

        Console.Write("Max distance to beach in meters (Enter to skip): ");
        int? maxBeach = int.TryParse(Console.ReadLine(), out int bDist) ? bDist : null;

        Console.Write("Max distance to center in meters (Enter to skip): ");
        int? maxCenter = int.TryParse(Console.ReadLine(), out int cDist) ? cDist : null;

        Console.Write("Sort by (1 = Price, 2 = Rating): ");
        string sortBy = Console.ReadLine() == "2" ? "rating" : "price";

        await service.SearchAvailableRooms(city, guests, checkIn, checkOut, requirePool, requireRestaurant, maxBeach, maxCenter, sortBy);
        
        break;

        case "2":

        Console.WriteLine("\n--- CREATE A BOOKING ---");

        // Use the active customer if already selected, otherwise ask for ID.
        int cId = selectedCustomerId ?? 0;
        if (cId == 0)
        {
            Console.Write("Enter Customer ID: ");
            cId = int.Parse(Console.ReadLine()!);
        }
        else
        {
        Console.WriteLine($"Using selected Customer ID: {cId}");
        }

        Console.Write("Enter Room ID: ");
        int rId = int.Parse(Console.ReadLine()!);

        Console.Write("Enter Check-In (YYYY-MM-DD): ");
        string cIn = Console.ReadLine()!;

        Console.Write("Enter Check-Out (YYYY-MM-DD): ");
        string cOut = Console.ReadLine()!;

        if (!DateTime.TryParse(cIn, out DateTime dd1) || !DateTime.TryParse(cOut, out DateTime dd2) || dd2 <= dd1)
        {
            Console.WriteLine("Invalid dates! Check-out must be after check-in (YYYY-MM-DD).");
            break;
        }

        Console.Write("Enter Number of Guests: ");
        int gCount = int.Parse(Console.ReadLine()!);

        await createBooking_.CBooking(cId, rId, cIn, cOut, gCount);
        break;

        case "3":

        Console.WriteLine("\n--- View Bookings ---");
        int customerid = selectedCustomerId ?? 0;
        if (customerid == 0)
        {
            Console.Write("Enter Customer ID: ");
            customerid = int.Parse(Console.ReadLine()!);
        }
        else
        {
            Console.WriteLine($"Using selected Customer ID: {customerid}");
        }
        await service.ViewBooking(customerid);
        break;

        case "4":
        Console.WriteLine("\n--- UPDATE GUEST COUNT ---");
        Console.Write("Enter Booking ID: ");
        int updateBookingId = int.Parse(Console.ReadLine()!);

        Console.Write("Enter New Number of Guests: ");
        int newGuestCount = int.Parse(Console.ReadLine()!);

        await ud_booking.UpdateGuestCount(updateBookingId, newGuestCount);
        break;

        case "5":
        Console.WriteLine("\n--- Cancel Booking ---");
        Console.Write("Enter Booking ID: ");
        int deleteBookingId = int.Parse(Console.ReadLine()!);

        await ud_booking.CancelBooking(deleteBookingId);
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
    // Prevent crash if user enters letters instead of numbers.
    catch (FormatException)
    {
        Console.WriteLine("Invalid input format! Please enter valid numbers and dates (YYYY-MM-DD).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Something went wrong: {ex.Message}");
    }

}
