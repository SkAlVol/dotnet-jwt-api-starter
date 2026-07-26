# Student Management API

REST API для управління записами студентів з JWT-авторизацією.
Побудовано на ASP.NET Core 9, Entity Framework Core, SQLite.

## Технології
- ASP.NET Core Minimal API
- Entity Framework Core (SQLite)
- JWT Bearer Authentication
- BCrypt для хешування паролів
- xUnit для юніт-тестування

## Можливості
- Реєстрація / логін з видачею JWT-токена
- CRUD-операції над студентами (захищені авторизацією)
- Валідація вхідних даних
- Глобальна обробка помилок

## Як запустити
\`\`\`bash
git clone https://github.com/SkAlVol/student-management-api
cd student-management-api
dotnet ef database update
dotnet run
\`\`\`

## Ендпоінти
| Метод | Шлях | Опис | Авторизація |
|-------|------|------|-------------|
| POST | /register | Реєстрація | Ні |
| POST | /login | Логін, отримати токен | Ні |
| GET | /students | Список студентів | Так |
| POST | /students | Створити студента | Так |
| GET | /students/{id} | Отримати студента | Так |
| PUT | /students/{id} | Оновити студента | Так |
| DELETE | /students/{id} | Видалити студента | Так |