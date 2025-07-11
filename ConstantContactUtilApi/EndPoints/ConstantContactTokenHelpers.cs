using ConstantContactUtilApi.EndPoints;
using Microsoft.Data.Sqlite;
using System.Text.Json;

public static class ConstantContactTokenHelpers
{


    static string clientId = "";
    static string clientSecret = "";
    static string redirectUri = "http://localhost:5176/oauth/callback";
    static string constantContactTokenEndpoint = "https://authz.constantcontact.com/oauth2/default/v1/token";
    static string dbPath = "Data Source=tokenstore.db";

    // In-memory token store for demo
    //string? refreshToken = null;

   // static string clientId = "<YOUR_CLIENT_ID>";
   // static string clientSecret = "<YOUR_CLIENT_SECRET>";


    public static TokenResponse? ExchangeCodeForToken(
        string code,
        string clientId,
        string clientSecret,
        string redirectUri,
        string tokenEndpoint)
    {
        using var client = new HttpClient();
        var values = new Dictionary<string, string>
        {
            {"grant_type", "authorization_code"},
            {"code", code},
            {"redirect_uri", redirectUri},
            {"client_id", clientId},
            {"client_secret", clientSecret}
        };

        var content = new FormUrlEncodedContent(values);
        var response = client.PostAsync(tokenEndpoint, content).Result;
        var body = response.Content.ReadAsStringAsync().Result;
        return response.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<TokenResponse>(body)
            : null;
    }
   public static  async Task<AccessTokenEntry?> EnsureValidTokenAsync(string userId)
    {
        var token = GetToken(userId);
        if (token is null) return null;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (token.Expiry > now - 60) return token; // still valid

        if (string.IsNullOrEmpty(token.RefreshToken)) return null;

        // Refresh token
        using var client = new HttpClient();
        var form = new Dictionary<string, string>
    {
        {"grant_type", "refresh_token"},
        {"refresh_token", token.RefreshToken},
        {"client_id", clientId},
        {"client_secret", clientSecret}
    };

        var content = new FormUrlEncodedContent(form);
        var response = await client.PostAsync(constantContactTokenEndpoint, content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return null;

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(body);
        SaveToken(tokenResponse, userId);
        return new AccessTokenEntry(
            tokenResponse!.access_token,
            tokenResponse.refresh_token,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tokenResponse.expires_in
        );
    }
    //public static async Task<AccessTokenEntry?> EnsureValidTokenAsync(
    //    string userId,
    //    string clientId,
    //    string clientSecret,
    //    string tokenEndpoint,
    //    string dbPath)
    //{
    //    var token = GetToken(userId, dbPath);
    //    if (token is null) return null;

    //    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    //    if (token.Expiry > now - 60) return token; // still valid

    //    if (string.IsNullOrEmpty(token.RefreshToken)) return null;

    //    using var client = new HttpClient();
    //    var form = new Dictionary<string, string>
    //    {
    //        {"grant_type", "refresh_token"},
    //        {"refresh_token", token.RefreshToken},
    //        {"client_id", clientId},
    //        {"client_secret", clientSecret}
    //    };

    //    var content = new FormUrlEncodedContent(form);
    //    var response = await client.PostAsync(tokenEndpoint, content);
    //    var body = await response.Content.ReadAsStringAsync();
    //    if (!response.IsSuccessStatusCode) return null;

    //    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(body);
    //    SaveToken(tokenResponse, userId, dbPath);
    //    return new AccessTokenEntry(
    //        tokenResponse!.access_token,
    //        tokenResponse.refresh_token,
    //        DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tokenResponse.expires_in
    //    );
    //}

    //public static void SaveToken(TokenResponse? tokenResponse, string userId, string dbPath)
    //{
    //    if (tokenResponse is null) return;

    //    using var connection = new SqliteConnection(dbPath);
    //    connection.Open();
    //    var cmd = connection.CreateCommand();
    //    cmd.CommandText = "REPLACE INTO Tokens (UserId, AccessToken, RefreshToken, Expiry) VALUES ($user, $access, $refresh, $expiry);";
    //    cmd.Parameters.AddWithValue("$user", userId);
    //    cmd.Parameters.AddWithValue("$access", tokenResponse.access_token);
    //    cmd.Parameters.AddWithValue("$refresh", tokenResponse.refresh_token);
    //    cmd.Parameters.AddWithValue("$expiry", DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tokenResponse.expires_in);
    //    cmd.ExecuteNonQuery();
    //}
    public static void  SaveToken(TokenResponse? tokenResponse, string userId)
    {
        if (tokenResponse is null) return;

        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "REPLACE INTO Tokens (UserId, AccessToken, RefreshToken, Expiry) VALUES ($user, $access, $refresh, $expiry);";
        cmd.Parameters.AddWithValue("$user", userId);
        cmd.Parameters.AddWithValue("$access", tokenResponse.access_token);
        cmd.Parameters.AddWithValue("$refresh", tokenResponse.refresh_token);
        cmd.Parameters.AddWithValue("$expiry", DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tokenResponse.expires_in);
        cmd.ExecuteNonQuery();
    }

    public static AccessTokenEntry? GetToken(string userId)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT AccessToken, RefreshToken, Expiry FROM Tokens WHERE UserId = $user LIMIT 1";
        cmd.Parameters.AddWithValue("$user", userId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return new AccessTokenEntry(
            reader.GetString(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.GetInt64(2)
        );
    }
    //public static AccessTokenEntry? GetToken(string userId, string dbPath)
    //{
    //    using var connection = new SqliteConnection(dbPath);
    //    connection.Open();
    //    var cmd = connection.CreateCommand();
    //    cmd.CommandText = "SELECT AccessToken, RefreshToken, Expiry FROM Tokens WHERE UserId = $user LIMIT 1";
    //    cmd.Parameters.AddWithValue("$user", userId);
    //    using var reader = cmd.ExecuteReader();
    //    if (!reader.Read()) return null;
    //    return new AccessTokenEntry(
    //        reader.GetString(0),
    //        reader.IsDBNull(1) ? null : reader.GetString(1),
    //        reader.GetInt64(2)
    //    );
    //}
    public static TokenResponse? ExchangeCodeForTokenAsync(string code)
    {
        using var client = new HttpClient();
        var values = new Dictionary<string, string>
    {
        {"grant_type", "authorization_code"},
        {"code", code},
        {"redirect_uri", redirectUri},
        {"client_id", clientId},
        {"client_secret", clientSecret}
    };

        var content = new FormUrlEncodedContent(values);
        var response = client.PostAsync(constantContactTokenEndpoint, content).Result;
        var body = response.Content.ReadAsStringAsync().Result;
        return response.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<TokenResponse>(body)
            : null;
    }
}