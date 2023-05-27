namespace UteamUP.Client.Web.Repository.Interfaces;

public interface IPartWebRepository
{
    Task<Part> CreatePartAsync(PartDto? part, int tenantId = 0);
}