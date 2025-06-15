namespace Mcp.Functions.Tools.TransactionTools;

public class Invoice
{
    public string Id { get; set; }
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public double Amount { get; set; }
    public System.DateTime DueDate { get; set; }
}
