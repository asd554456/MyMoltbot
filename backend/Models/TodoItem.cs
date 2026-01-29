namespace Backend.Models;

public class TodoItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public int Priority { get; set; } = 1; // 1-5, 5 = 最高
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }
    
    // 關聯用戶
    public int UserId { get; set; }
    public User? User { get; set; }
}
