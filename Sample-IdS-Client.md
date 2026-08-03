# Sample Client Configuration

The following is only an example, your configuration might have different needs.
- A Client Secret is Required.
- The `authorization_code` is for sure required, `password` is likely not.
- The required scopes would include `openid`, `evolution` but I went a little overkill.
- `RequirePkce` should be checked. 

```json
{
          "Enabled": true,
          "ClientId": "000000000-0000-0000-0000-000000000000",
          "ClientName": "OpenId API Client",
          "Description": "",
          "ProtocolType": "oidc",
          "IncludeXFrameOptions": false,
          "RedirectUris": [
            "http://localhost:5180/authenticate",
            "https://localhost:7086/authenticate"      
          ],
          "AllowedFrameAncestors": [],
          "TokenSettings": {
            "IdentityTokenLifetime": 300,
            "AccessTokenLifetime": 3600,
            "UseAccessTokenLifetimeForUserSystemId": false,
            "AuthorizationCodeLifetime": 300,
            "AbsoluteRefreshTokenLifetime": 2592000,
            "SlidingRefreshTokenLifetime": 1296000,
            "RefreshTokenUsage": 0,
            "UpdateAccessTokenClaimsOnRefresh": false,
            "RefreshTokenExpiration": 0,
            "AccessTokenType": 0,
            "IncludeJwtId": false,
            "PairWiseSubjectSalt": "",
            "AlwaysIncludeUserClaimsInIdToken": false
          },
          "LogoutSettings": {
            "PostLogoutRedirectUris": [],
            "FrontChannelLogoutUri": null,
            "FrontChannelLogoutSessionRequired": true,
            "BackChannelLogoutUri": null,
            "BackChannelLogoutSessionRequired": true
          },
          "AuthenticationRestrictionSettings": {
            "EnableLocalLogin": true,
            "IdentityProviderRestrictions": [],
            "UserSsoLifetime": null,
            "AllowOfflineAccess": true,
            "AllowAccessTokensViaBrowser": true,
            "AllowedGrantTypes": [
              "authorization_code",
              "password"
            ],
            "AllowedScopes": [
              "openid",
              "profile",
              "profile.onbase",
              "evolution",
              "onbaseapi",
              "offline_access"
            ]
          },
          "PkceSettings": {
            "RequirePkce": true,
            "AllowPlainTextPkce": false
          },
          "DeviceFlowSettings": {
            "UserCodeType": "",
            "DeviceCodeLifetime": 300
          },
          "SecretSettings": {
            "ClientSecrets": [
              // To do: Insert Frodo "keep your secrets" meme
            ],
            "RequireClientSecret": true
          },
          "SecuritySettings": {
            "AllowedCorsOrigins": []
          }
        }
```