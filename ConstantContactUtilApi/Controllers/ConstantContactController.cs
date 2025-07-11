//using ConstantContactUtilApi.EndPoints;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.Sqlite;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;

//[ApiController]
//[Route("api")]
//public class ConstantContactController : ControllerBase
//{
//    private static string clientId = "";
//    private static string clientSecret = "";
//    private static string redirectUri = "http://localhost:5176/oauth/callback";
//    private static string constantContactTokenEndpoint = "https://authz.constantcontact.com/oauth2/default/v1/token";


//    string dbPath = "Data Source=tokenstore.db";


//    // In-memory token store for demo
//    private static string? refreshToken = null;

//    [HttpGet("contacts/search")]
//    public async Task<IActionResult> SearchContact([FromQuery] string email)
//    {
//        if (accessToken is null) return Unauthorized();

//        using var httpClient = new HttpClient();
//        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

//        var response = await httpClient.GetAsync($"https://api.cc.email/v3/contacts?email={email}");
//        var content = await response.Content.ReadAsStringAsync();

//        return Content(content, "application/json");
//    }

//    [HttpPost("contacts/create")]
//    public async Task<IActionResult> CreateContact([FromBody] ContactDto contact)
//    {
//        if (accessToken is null) return Unauthorized();

//        using var httpClient = new HttpClient();
//        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

//        var payload = new
//        {
//            email_address = new { 
//                address = contact.Email,
//                permission_to_send= "implicit",

//            },
//            first_name = contact.FirstName,
//            last_name = contact.LastName,
//            create_source= "Account",
//           // list_memberships = new[] { "07936f78-662a-11eb-af0a-fa163e56c9b0" }
//        };

//        var json = JsonSerializer.Serialize(payload);
//        var content = new StringContent(json, Encoding.UTF8, "application/json");

//        var response = await httpClient.PostAsync("https://api.cc.email/v3/contacts", content);
//        var result = await response.Content.ReadAsStringAsync();

//        return Content(result, "application/json");
//    }

//    [HttpPost("auth/exchange")]
//    public async Task<IActionResult> ExchangeAuthCodeOld([FromBody] AuthCodeDto form)
//    {
//        if (form?.Code is null) return BadRequest("Missing code");

//        using var client = new HttpClient();
//        var values = new Dictionary<string, string>
//        {
//            {"grant_type", "authorization_code"},
//            {"code", form.Code},
//            {"redirect_uri", redirectUri},
//            {"client_id", clientId},
//            {"client_secret", clientSecret}
//        };

//        var content = new FormUrlEncodedContent(values);
//        var response = await client.PostAsync(constantContactTokenEndpoint, content);
//        var body = await response.Content.ReadAsStringAsync();
//        if (!response.IsSuccessStatusCode) return BadRequest(body);

//        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(body);
//        accessToken = tokenResponse?.access_token;
//        refreshToken = tokenResponse?.refresh_token;

//        return Ok(tokenResponse);
//    }


//   public  TokenResponse? ExchangeCodeForTokenAsync(string code)
//    {
//        using var client = new HttpClient();
//        var values = new Dictionary<string, string>
//    {
//        {"grant_type", "authorization_code"},
//        {"code", code},
//        {"redirect_uri", redirectUri},
//        {"client_id", clientId},
//        {"client_secret", clientSecret}
//    };

//        var content = new FormUrlEncodedContent(values);
//        var response = client.PostAsync(constantContactTokenEndpoint, content).Result;
//        var body = response.Content.ReadAsStringAsync().Result;
//        return response.IsSuccessStatusCode
//            ? JsonSerializer.Deserialize<TokenResponse>(body)
//            : null;
//    }
  


//   public  async Task<AccessTokenEntry?> EnsureValidTokenAsync(string userId)
//    {
//        var token = GetToken(userId);
//        if (token is null) return null;

//        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//        if (token.Expiry > now - 60) return token; // still valid

//        if (string.IsNullOrEmpty(token.RefreshToken)) return null;

//        // Refresh token
//        using var client = new HttpClient();
//        var form = new Dictionary<string, string>
//    {
//        {"grant_type", "refresh_token"},
//        {"refresh_token", token.RefreshToken},
//        {"client_id", clientId},
//        {"client_secret", clientSecret}
//    };

//        var content = new FormUrlEncodedContent(form);
//        var response = await client.PostAsync(constantContactTokenEndpoint, content);
//        var body = await response.Content.ReadAsStringAsync();
//        if (!response.IsSuccessStatusCode) return null;

//        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(body);
//        SaveToken(tokenResponse, userId);
//        return new AccessTokenEntry(
//            tokenResponse!.access_token,
//            tokenResponse.refresh_token,
//            DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tokenResponse.expires_in
//        );
//    }
//    void SaveToken(TokenResponse? tokenResponse, string userId)
//    {
//        if (tokenResponse is null) return;

//        using var connection = new SqliteConnection(dbPath);
//        connection.Open();
//        var cmd = connection.CreateCommand();
//        cmd.CommandText = "REPLACE INTO Tokens (UserId, AccessToken, RefreshToken, Expiry) VALUES ($user, $access, $refresh, $expiry);";
//        cmd.Parameters.AddWithValue("$user", userId);
//        cmd.Parameters.AddWithValue("$access", tokenResponse.access_token);
//        cmd.Parameters.AddWithValue("$refresh", tokenResponse.refresh_token);
//        cmd.Parameters.AddWithValue("$expiry", DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tokenResponse.expires_in);
//        cmd.ExecuteNonQuery();
//    }

//    AccessTokenEntry? GetToken(string userId)
//    {
//        using var connection = new SqliteConnection(dbPath);
//        connection.Open();
//        var cmd = connection.CreateCommand();
//        cmd.CommandText = "SELECT AccessToken, RefreshToken, Expiry FROM Tokens WHERE UserId = $user LIMIT 1";
//        cmd.Parameters.AddWithValue("$user", userId);
//        using var reader = cmd.ExecuteReader();
//        if (!reader.Read()) return null;
//        return new AccessTokenEntry(
//            reader.GetString(0),
//            reader.IsDBNull(1) ? null : reader.GetString(1),
//            reader.GetInt64(2)
//        );
//    }


//}