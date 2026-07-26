using Microsoft.EntityFrameworkCore;

public interface IStudentService
{
    Task<List<StudentDto>> GetAllAsync();
    Task<StudentDto?> GetByIdAsync(int id);
    Task<StudentDto> CreateAsync(CreateStudentDto input);
    Task<StudentDto?> UpdateAsync(int id, UpdateStudentDto input);
    Task<bool> DeleteAsync(int id);
}

public class StudentService : IStudentService
{
    private readonly AppDbContext _db;

    public StudentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<StudentDto>> GetAllAsync()
    {
        var students = await _db.Students.ToListAsync();
        return students.Select(s => new StudentDto(s.Id, s.Name, s.Age)).ToList();
    }

    public async Task<StudentDto?> GetByIdAsync(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student is null) return null;
        return new StudentDto(student.Id, student.Name, student.Age);
    }

    public async Task<StudentDto> CreateAsync(CreateStudentDto input)
    {
        var student = new Student { Name = input.Name, Age = input.Age };
        _db.Students.Add(student);
        await _db.SaveChangesAsync();
        return new StudentDto(student.Id, student.Name, student.Age);
    }

    // ДОДАНО: раніше відсутній метод — саме через це PUT /students/{id} у Program.cs
    // раніше не міг йти через сервіс і працював напряму з AppDbContext
    public async Task<StudentDto?> UpdateAsync(int id, UpdateStudentDto input)
    {
        var student = await _db.Students.FindAsync(id);
        if (student is null) return null;

        student.Name = input.Name;
        student.Age = input.Age;
        await _db.SaveChangesAsync();

        return new StudentDto(student.Id, student.Name, student.Age);
    }

    // ДОДАНО: аналогічно для DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student is null) return false;

        _db.Students.Remove(student);
        await _db.SaveChangesAsync();
        return true;
    }
}