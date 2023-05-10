using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UteamUP.Client.Web.Repository.Interfaces;
using UteamUP.Shared.States;
using Blazored.LocalStorage;

namespace UteamUP.Client.Web.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IUserWebRepository _userWebRepository;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly UserState _userState;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;

        public CustomAuthenticationStateProvider(
            ILocalStorageService localStorageService,
            IUserWebRepository userWebRepository,
            IAccessTokenProvider accessTokenProvider, 
            UserState userState, 
            ILogger<CustomAuthenticationStateProvider> logger
            )
        {
            _localStorageService = localStorageService;
            _userWebRepository = userWebRepository;
            _accessTokenProvider = accessTokenProvider;
            _userState = userState;
            _logger = logger;
        }

        // Retrieves the authentication state asynchronously
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _logger.Log(LogLevel.Information, $"{nameof(GetAuthenticationStateAsync)}: Getting authentication claims state");
            var claims = await GetClaimsFromLocalStorageAsync() ?? await GetClaimsFromTokenAsync();
            var identity = new ClaimsIdentity(claims, "Authentication");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        // Retrieves claims from local storage asynchronously
        private async Task<IEnumerable<Claim>> GetClaimsFromLocalStorageAsync()
        {
            _logger.Log(LogLevel.Information, $"{nameof(GetClaimsFromLocalStorageAsync)}: Getting claims from local storage");
            var savedStateJson = await _localStorageService.GetItemAsStringAsync("globalState");
            OnGlobalStateChanged?.Invoke();

            if (string.IsNullOrEmpty(savedStateJson)) return null;
            
            _logger.Log(LogLevel.Information, $"{nameof(GetClaimsFromLocalStorageAsync)}: Found saved state");
            var globalState = await _localStorageService.GetItemAsync<GlobalState>("globalState");

            _userState.SetUser(CreateMUserFromGlobalState(globalState));

            return new[]
            {
                new Claim("name", globalState.Name),
                new Claim("email", globalState.Email),
                new Claim("oid", globalState.Oid)
            };
        }

        // Retrieves claims from the token asynchronously
        private async Task<IEnumerable<Claim>> GetClaimsFromTokenAsync()
        {
            var tokenResult = await _accessTokenProvider.RequestAccessToken();

            if (!tokenResult.TryGetToken(out var accessToken)) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken.Value);

            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "signInNames.emailAddress")?.Value;
            var oid = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;

            return string.IsNullOrEmpty(oid) ? null : new[]
            {
                new Claim("name", name),
                new Claim("email", email),
                new Claim("oid", oid)
            };
        }

        // Creates an MUser from the given global state
        private static MUser CreateMUserFromGlobalState(GlobalState globalState)
        {
            return new MUser
            {
                Name = globalState.Name,
                Email = globalState.Email,
                Oid = globalState.Oid,
                Tenants = globalState.Tenants,
                DefaultTenantId = globalState.DefaultTenantId,
                HasBeenActivated = globalState.IsActivated,
                IsFirstLogin = globalState.FirstLogin
            };
        }

        // Updates the app state with the user information
        public async Task UpdateAppStateWithUserAsync(ClaimsPrincipal user)
        {
            _logger.Log(LogLevel.Information, $"{nameof(UpdateAppStateWithUserAsync)}: Updating app state with user information");
            var oidClaim = user.Claims.FirstOrDefault(c => c.Type == "oid");
            var nameClaim = user.Claims.FirstOrDefault(c => c.Type == "name");
            var emailClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email) ??
                             user.Claims.FirstOrDefault(c => c.Type == "signInNames.emailAddress") ??
                             user.Claims.FirstOrDefault(c => c.Type == "email");

            if (oidClaim != null)
            {
                _logger.Log(LogLevel.Information, $"{nameof(UpdateAppStateWithUserAsync)}: Getting user information from database");
                MUser muser = await GetUserInformationFromDB(oidClaim.Value);

                // Check if the global state exists
                GlobalState globalState = await _localStorageService.GetItemAsync<GlobalState>("globalState");

                // if the global state does not exist, create it and update it with the user information
                if (globalState == null)
                {
                    MUserDto newMUserDto = new MUserDto
                    {
                        Name = nameClaim?.Value,
                        Email = emailClaim?.Value,
                        Oid = oidClaim.Value,
                    };

                    // if the muser is null, create a new user and return it
                    if (muser == null)
                    {
                        _logger.Log(LogLevel.Information, $"{nameof(UpdateAppStateWithUserAsync)}: Creating new user, since the user does not exist with the oid: {oidClaim.Value}");
                        var tmuser = await _userWebRepository.CreateUserAsync(newMUserDto);
                        if (tmuser)
                        {
                            _logger.Log(LogLevel.Information, $"{nameof(UpdateAppStateWithUserAsync)}: User created successfully and retrieved from database user with the oid: {oidClaim.Value}");
                            muser = await GetUserInformationFromDB(oidClaim.Value);
                        }
                    }

                    GlobalState newGlobalState = CreateGlobalStateFromMUser(muser);
                    
                    _logger.Log(LogLevel.Information, $"{nameof(UpdateAppStateWithUserAsync)}: Setting global state with the oid: {oidClaim.Value}");
                    await _localStorageService.SetItemAsync("globalState", newGlobalState);
                    OnGlobalStateChanged?.Invoke();
                }

                _userState.SetUser(muser);
            }
            else
            {
            }
        }

        // Creates a global state from the given MUser
        private static GlobalState CreateGlobalStateFromMUser(MUser muser)
        {
            return new GlobalState
            {
                Name = muser.Name,
                Email = muser.Email,
                Oid = muser.Oid,
                HasDatabaseUser = true,
                IsActivated = muser.HasBeenActivated,
                FirstLogin = muser.IsFirstLogin,
                DefaultTenantId = muser.DefaultTenantId,
                IsUpToDate = true
            };
        }

        // Event that gets triggered when the global state changes
        public event Action OnGlobalStateChanged;

        // Retrieves the user information from the database
        public async Task<MUser> GetUserInformationFromDB(string oid)
        {
            // Get the user information from the database
            MUser? user = await _userWebRepository.GetUserByOid(oid);

            if (!string.IsNullOrEmpty(user?.Email))
            {
                _logger.Log(LogLevel.Information, $"{nameof(GetUserInformationFromDB)}: User retrieved from database with the oid: {oid}");
                return user;
            }
            else
            {
                _logger.Log(LogLevel.Warning, $"{nameof(GetUserInformationFromDB)}: User does not exist with the oid: {oid}");
                return new MUser();
            }
        }

        public async Task<GlobalState> GetGlobalStateAsync()
        {
            _logger.Log(LogLevel.Information, $"{nameof(GetGlobalStateAsync)}: Getting global state");
            return await _localStorageService.GetItemAsync<GlobalState>("globalState");
        }
    }
}

