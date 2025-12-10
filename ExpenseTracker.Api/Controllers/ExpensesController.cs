using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ExpensesController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/expenses
    [HttpGet]
    public async Task<ActionResult<List<Expense>>> GetAll()
    {
        var expenses = await _db.Expenses
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        return Ok(expenses);
    }

    // GET: api/expenses/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Expense>> GetById(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense == null)
            return NotFound();

        return Ok(expense);
    }

    // POST: api/expenses
    [HttpPost]
    public async Task<ActionResult<Expense>> Create([FromBody] Expense expense)
    {
        expense.Date = expense.Date == default ? DateTime.UtcNow : expense.Date;

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
    }

    // PUT: api/expenses/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Expense updated)
    {
        Console.WriteLine("id: " + id);
        Console.WriteLine("updated: " + updated);

        var expense = await _db.Expenses.FindAsync(id);
        if (expense == null)
            return NotFound();

        expense.Title = updated.Title;
        expense.Amount = updated.Amount;
        expense.Date = updated.Date;
        expense.Category = updated.Category;

        await _db.SaveChangesAsync();
            await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Updated successfully",
            data = expense
        });
    }

    // DELETE: api/expenses/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense == null)
            return NotFound();

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
        return Ok($"Delete expense no. {id} successfully");
    }
}
