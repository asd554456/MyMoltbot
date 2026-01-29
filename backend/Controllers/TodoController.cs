using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodoController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoResponse>>> GetAll()
    {
        var userId = GetUserId();
        var todos = await _context.TodoItems
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new TodoResponse(
                t.Id, t.Title, t.Description, t.IsCompleted, 
                t.Priority, t.CreatedAt, t.CompletedAt, t.DueDate))
            .ToListAsync();

        return Ok(todos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoResponse>> GetById(int id)
    {
        var userId = GetUserId();
        var todo = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null) return NotFound();

        return Ok(new TodoResponse(
            todo.Id, todo.Title, todo.Description, todo.IsCompleted,
            todo.Priority, todo.CreatedAt, todo.CompletedAt, todo.DueDate));
    }

    [HttpPost]
    public async Task<ActionResult<TodoResponse>> Create(CreateTodoRequest request)
    {
        var userId = GetUserId();
        var todo = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            UserId = userId
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = todo.Id },
            new TodoResponse(todo.Id, todo.Title, todo.Description, todo.IsCompleted,
                todo.Priority, todo.CreatedAt, todo.CompletedAt, todo.DueDate));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TodoResponse>> Update(int id, UpdateTodoRequest request)
    {
        var userId = GetUserId();
        var todo = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null) return NotFound();

        if (request.Title != null) todo.Title = request.Title;
        if (request.Description != null) todo.Description = request.Description;
        if (request.Priority.HasValue) todo.Priority = request.Priority.Value;
        if (request.DueDate.HasValue) todo.DueDate = request.DueDate.Value;
        
        if (request.IsCompleted.HasValue)
        {
            todo.IsCompleted = request.IsCompleted.Value;
            todo.CompletedAt = request.IsCompleted.Value ? DateTime.UtcNow : null;
        }

        await _context.SaveChangesAsync();

        return Ok(new TodoResponse(todo.Id, todo.Title, todo.Description, todo.IsCompleted,
            todo.Priority, todo.CreatedAt, todo.CompletedAt, todo.DueDate));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var todo = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null) return NotFound();

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
