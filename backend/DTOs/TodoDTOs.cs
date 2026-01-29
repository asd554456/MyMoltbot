namespace Backend.DTOs;

public record CreateTodoRequest(string Title, string? Description, int Priority = 1, DateTime? DueDate = null);
public record UpdateTodoRequest(string? Title, string? Description, int? Priority, bool? IsCompleted, DateTime? DueDate);
public record TodoResponse(int Id, string Title, string? Description, bool IsCompleted, int Priority, DateTime CreatedAt, DateTime? CompletedAt, DateTime? DueDate);
