using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities; 
using LNUBookShare.Infrastructure;
using LNUBookShare.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connectionString = "Host=ep-rapid-term-adecek0i-pooler.c-2.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_bwNlrKEdO71B;SSL Mode=Require;Trust Server Certificate=true;"; 
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql(connectionString);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/console-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

using var context = new AppDbContext(optionsBuilder.Options);

var bookRepository = new BookRepository(context);
var categoryRepository = new CategoryRepository(context);
var userRepository = new UserRepository(context);
var facultyRepository = new FacultyRepository(context);
var roleRepository = new RoleRepository(context);

try
{
    await bookRepository.ClearAllAsync();   
    await categoryRepository.ClearAllAsync(); 
    await userRepository.ClearAllAsync();
    await facultyRepository.ClearAllAsync();
    await roleRepository.ClearAllAsync();

    var testCategory = new Category
    {
        
        CategoryName = "Програмування",
 
    };
    await categoryRepository.AddAsync(testCategory);

    var testFaculty = new Faculty
    {
        FacultyName = "Applied Mathemetics"
    };
    await facultyRepository.AddAsync(testFaculty);

    var testRole = new Role
    {
        RoleName = "Гість"
    };

    var testUser = new User
    {
        FirstName = "Sofiia",
        LastName = "Bohdanovych",
        Email = "sofiia.bohdanovych@lnu.edu.ua",
        PasswordHash = "abrakadabra",
        Faculty = testFaculty,
        Role = testRole
    };
    await userRepository.AddAsync(testUser);

    

    Console.WriteLine($"Created category (ID: {testCategory.CategoryId}) || user (ID: {testUser.UserId}) || faculty (ID: {testFaculty.FacultyId})  || role (ID: {testRole.RoleId})");
    Log.Information($"Created category (ID: {testCategory.CategoryId}) || user (ID: {testUser.UserId}) || faculty (ID: {testFaculty.FacultyId})  || role (ID: {testRole.RoleId})");

    Console.WriteLine("Creating book...");
    Log.Information("Creating book...");

    var newBook = new Book
    {
        Title = "Clean Architecture",
        Author = "Geek",
        Year = 2024,
        Status = "available",
        CreatedAt = DateTime.UtcNow,

        CategoryId = testCategory.CategoryId,
        OwnerId = testUser.UserId
    };
    await bookRepository.AddAsync(newBook);
    Console.WriteLine($"New book ID: {newBook.BookId}");
    Log.Information($"New book ID: {newBook.BookId}");

    Console.WriteLine("Reading created book...");
    Log.Information("Reading created book...");

    var fetchedBook = await bookRepository.GetByIdAsync(newBook.BookId);
    Console.WriteLine($"Found: {fetchedBook?.Title} (Author: {fetchedBook?.Author})");
    Log.Information($"Found: {fetchedBook?.Title} (Author: {fetchedBook?.Author})");

    Console.WriteLine("Updating a book...");
    Log.Information("Updating a book...");

    fetchedBook.Title = "Something UPD";
    await bookRepository.UpdateAsync(fetchedBook);
    Console.WriteLine("Name Updated");
    Log.Information("Name Updated");

    Console.WriteLine("Reading updated book...");
    Log.Information("Reading updated book...");
    var updatedBook = await bookRepository.GetByIdAsync(newBook.BookId);
    Console.WriteLine($"Found: {updatedBook?.Title} (Author: {updatedBook?.Author})");
    Log.Information($"Found: {updatedBook?.Title} (Author: {updatedBook?.Author})");

    Console.WriteLine("Deleting");
    Log.Information("Deleting");
    await bookRepository.DeleteAsync(updatedBook);
    Console.WriteLine("Book deleted");
    Log.Information("Book deleted");

    Console.WriteLine("Searching deleted...");
    Log.Information("Searching deleted...");
    var deletedBook = await bookRepository.GetByIdAsync(updatedBook.BookId);
    
    if(deletedBook == null)
    {
        Console.WriteLine("Not found");
        Log.Information("Not found");
    }
    else
    {
        Console.WriteLine("Hmmm..was found");
        Log.Information("Hmmm..was found");
    }


}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Log.Information($"Error: {ex.Message}");

    if (ex.InnerException != null)
    {
        Console.WriteLine($"(Inner Exception): {ex.InnerException.Message}");
        Log.Information($"(Inner Exception): {ex.InnerException.Message}");
    }
}

Console.ReadLine();

//public class UglyTestClass
//{
//    // 1. Властивість з маленької літери (Аналізатори іменування це ненавидять)
//    public string badNamingProperty { get; set; }

//    // 2. Метод без вказаного public/private 
//    void do_something_weird()
//    {
//        // 3. Змінна, яку створили, але ніде не використали
//        int uselessVariable = 42;

//        // 4. Порожній блок if
//        if (true)
//        {
//        }
//    }
//}

