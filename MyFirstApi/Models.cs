public class Student
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}