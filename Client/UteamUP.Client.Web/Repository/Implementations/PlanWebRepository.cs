using System.Net.Http.Json;
using UteamUP.Client.Web.Repository.Interfaces;

namespace UteamUP.Client.Web.Repository.Implementations;

public class PlanWebRepository : IPlanWebRepository
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IHeaderRepository _headerRepository;
    private readonly ILogger<PlanWebRepository> _logger;
    private protected string ServerUrl = "https://localhost:5001";
    private protected string Url = "api/plan";

    public PlanWebRepository(
        HttpClient httpClient, 
        AuthenticationStateProvider authenticationStateProvider, 
        IHeaderRepository headerRepository, ILogger<PlanWebRepository> logger)
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
    
    // Create the plan
    public async Task<bool> CreatePlanAsync(PlanDto? plan)
    {
        await GetHttpClientHeaderToken();
        var result = await _httpClient.PostAsJsonAsync<PlanDto>($"{Url}", plan);
        if (result.IsSuccessStatusCode)
        {
            await result.Content.ReadFromJsonAsync<PlanDto>();
            _logger.Log(LogLevel.Information, $"{nameof(CreatePlanAsync)}: Plan created successfully");
            return true;
        }
        else
        {
            _logger.Log(LogLevel.Error,
                $"{nameof(CreatePlanAsync)}: Plan creation failed, because of : " + result.StatusCode);
            return false;
        }
    }

    public async Task<List<Plan?>?> GetAllPlansAsync()
    {
        var result = await _httpClient.GetAsync($"{Url}");
        if (result.IsSuccessStatusCode)
        {
            _logger.Log(LogLevel.Information, $"{nameof(GetAllPlansAsync)}: Got all plans successfully");
            return await result.Content.ReadFromJsonAsync<List<Plan?>>();
        }
        else
        {
            _logger.Log(LogLevel.Error,
                $"{nameof(GetAllPlansAsync)}: Failed to get all plans, because of : " + result.StatusCode);
            return new List<Plan?>();
        }
    }
}