namespace UteamUP.Shared.Models;

public class Stock : Base
{
    public Stock()
    {
        Locations = new List<Location>();
        Tags = new List<Tag>();
    }

    [Key] public int Id { get; set; }

    [MaxLength(512)]
    [MinLength(2)]
    [Required(ErrorMessage = "You must specify the name before you can save.")]
    public string Name { get; set; } = string.Empty;

    public string Guid { get; set; } = string.Empty;
    public string RackBarNumber { get; set; } = string.Empty;
    public string ShelveNumber { get; set; } = string.Empty;
    public string ShelveName { get; set; } = string.Empty;

    public virtual List<Location> Locations { get; set; }
    public virtual List<Tag>? Tags { get; set; } = new();

    [ForeignKey("Tenant")] public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    [ForeignKey("Category")] public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}