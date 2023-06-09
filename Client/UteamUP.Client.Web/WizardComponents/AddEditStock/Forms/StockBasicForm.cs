namespace UteamUP.Client.Web.WizardComponents.AddEditStock.Forms;

public class StockBasicForm
{
    public string Name { get; set; }

    public string? Guid { get; set; }
    public string? RackBarNumber { get; set; }
    public string? ShelveNumber { get; set; }
    public string? ShelveName { get; set; }

    public int? TenantId { get; set; }
    public string Location { get; set; }
    public int? LocationId { get; set; }
    public string Category { get; set; }
    public int? CategoryId { get; set; }
}