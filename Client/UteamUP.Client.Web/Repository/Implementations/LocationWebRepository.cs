using System.Net.Http.Json;
using UteamUP.Client.Web.Repository.Interfaces;

namespace UteamUP.Client.Web.Repository.Implementations;

public class LocationWebRepository : ILocationWebRepository
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IHeaderRepository _headerRepository;
    private readonly ILogger<LocationWebRepository> _logger;
    private protected string Url = "api/location";
    
    public LocationWebRepository(
        HttpClient httpClient, 
        AuthenticationStateProvider authenticationStateProvider, 
        IHeaderRepository headerRepository, 
        ILogger<LocationWebRepository> logger
    )
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _headerRepository = headerRepository;
        _logger = logger;
    }

    // a private function that updates the header with the current user's token
    private async Task GetHttpClientHeaderToken()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        _httpClient.DefaultRequestHeaders.Authorization = await _headerRepository.GetHeaderAsync();
    }
    
    public async Task<List<Location>> GetAllLocationsByTenantIdAsync(int tenantId)
    {
        await GetHttpClientHeaderToken();
        var result = await _httpClient.GetAsync($"{Url}/{tenantId}");
        
        if (result.IsSuccessStatusCode)
        {
            _logger.Log(LogLevel.Information, $"{nameof(GetAllLocationsByTenantIdAsync)}: Locations retrieved successfully");
            return await result.Content.ReadFromJsonAsync<List<Location>>();
        }
        else
        {
            _logger.Log(LogLevel.Error,
                $"{nameof(GetAllLocationsByTenantIdAsync)}: Locations retrieval failed, because of : " + result.StatusCode);
            return new List<Location>();
        }
    }

    public async Task<Location?> CreateLocationAsync(LocationDto location, int tenantId)
    {
        await GetHttpClientHeaderToken();
        var result = await _httpClient.PostAsJsonAsync($"{Url}/{tenantId}", location);
        
        if (result.IsSuccessStatusCode)
        {
            _logger.Log(LogLevel.Information, $"{nameof(CreateLocationAsync)}: Location created successfully");
            return await result.Content.ReadFromJsonAsync<Location>();
        }
        else
        {
            _logger.Log(LogLevel.Error,
                $"{nameof(CreateLocationAsync)}: Location creation failed, because of : " + result.StatusCode);
            return new Location();
        }
    }
}