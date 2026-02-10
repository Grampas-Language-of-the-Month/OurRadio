using Microsoft.AspNetCore.Components.Authorization;

namespace OurRadio.Data
{
    public class AuthGuard
    {
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthGuard(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public async Task EnsureAuthenticatedAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("Login required.");
            }
        }
    }
}
