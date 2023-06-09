namespace UteamUP.Shared.ModelDto;

public class TenantDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ContactEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;
    public int OwnerId { get; set; }
    public int SubscriptionId { get; set; }
}