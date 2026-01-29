namespace Backend.DTOs;

public record CreateTodoRequest(string Title, string? Description, int Priority = 1, DateOnly? DueDate = null);
public record UpdateTodoRequest(string? Title, string? Description, int? Priority, bool? IsCompleted, DateOnly? DueDate);
public record TodoResponse(int Id, string Title, string? Description, bool IsCompleted, int Priority, DateTime CreatedAt, DateTime? CompletedAt, DateOnly? DueDate);
