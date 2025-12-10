namespace ExpenseTracker.Api.Models;

public class Expense
{
    public int Id { get; set; }           // PK
    public string Title { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Category { get; set; }
}
