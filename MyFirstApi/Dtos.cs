using System.ComponentModel.DataAnnotations;

public record StudentDto(int Id, string Name, int Age);

public record CreateStudentDto(
    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [MaxLength(100)] string Name,
    [Range(1, 120)] int Age
);

public record UpdateStudentDto(
    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [MaxLength(100)] string Name,
    [Range(1, 120)] int Age
);

public record LoginDto(string Email, string Password);
public record TokenDto(string Token);