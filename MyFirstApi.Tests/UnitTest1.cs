using Microsoft.EntityFrameworkCore;

public class StudentServiceTests
{
    private AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStudents()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.Students.AddRange(
            new Student { Name = "Богдан", Age = 20 },
            new Student { Name = "Іра", Age = 22 }
        );
        await db.SaveChangesAsync();

        var service = new StudentService(db);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Name == "Богдан");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var service = new StudentService(db);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsStudentToDb()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var service = new StudentService(db);
        var input = new CreateStudentDto("Тестовий", 25);

        // Act
        var result = await service.CreateAsync(input);

        // Assert
        Assert.Equal("Тестовий", result.Name);
        Assert.Equal(25, result.Age);
        Assert.Equal(1, db.Students.Count());
    }
}