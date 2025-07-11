using ConstantContactUtilApi.EndPoints;
using Microsoft.Data.Sqlite;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public static class ConstantContactEndpoints
{
    public static void MapConstantContactEndpoints(this WebApplication app)
    {


        //app.MapGet("/api/contacts/search", async (string email) =>
        //{
        //    if (accessToken is null) return Results.Unauthorized();

        //    using var httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //    var response = await httpClient.GetAsync($"https://api.cc.email/v3/contacts?email={email}");
        //    var content = await response.Content.ReadAsStringAsync();

        //    return Results.Content(content, "application/json");
        //});

        app.MapGet("/api/contacts/search", async (string email, string userId) =>
        {
            var token = await ConstantContactTokenHelpers.EnsureValidTokenAsync(userId);
            if (token is null) return Results.Unauthorized();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var response = await httpClient.GetAsync($"https://api.cc.email/v3/contacts?email={email}");
            var content = await response.Content.ReadAsStringAsync();

            return Results.Content(content, "application/json");
        });

        //app.MapPost("/api/contacts/create", async (ConstantContactUtilApi.EndPoints.ContactDto contact) =>
        //{
        //    if (accessToken is null) return Results.Unauthorized();

        //    using var httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //    var payload = new
        //    {
        //        email_address = new
        //        {
        //            address = contact.Email,
        //            permission_to_send = "implicit",
        //        },
        //        first_name = contact.FirstName,
        //        last_name = contact.LastName,
        //        create_source = "Account",
        //        // list_memberships = new[] { "07936f78-662a-11eb-af0a-fa163e56c9b0" }
        //    };

        //    var json = JsonSerializer.Serialize(payload);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    var response = await httpClient.PostAsync("https://api.cc.email/v3/contacts", content);
        //    var result = await response.Content.ReadAsStringAsync();

        //    return Results.Content(result, "application/json");
        //});

        app.MapPost("/api/contacts/create", async (ContactDto contact, string userId) =>
        {
            var token = await ConstantContactTokenHelpers.EnsureValidTokenAsync(userId);
            if (token is null) return Results.Unauthorized();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var payload = new
            {
                email_address = new { address = contact.Email },
                first_name = contact.FirstName,
                last_name = contact.LastName,
                list_memberships = new[] { "<YOUR_LIST_ID>" }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.cc.email/v3/contacts", content);
            var result = await response.Content.ReadAsStringAsync();

            return Results.Content(result, "application/json");
        });
        //app.MapPost("/api/auth/exchange", async (AuthCodeDto form) =>
        //{
        //    if (form?.Code is null) return Results.BadRequest("Missing code");

        //    using var client = new HttpClient();
        //    var values = new Dictionary<string, string>
        //    {
        //        {"grant_type", "authorization_code"},
        //        {"code", form.Code},
        //        {"redirect_uri", redirectUri},
        //        {"client_id", clientId},
        //        {"client_secret", clientSecret}
        //    };

        //    var content = new FormUrlEncodedContent(values);
        //    var response = await client.PostAsync(constantContactTokenEndpoint, content);
        //    var body = await response.Content.ReadAsStringAsync();
        //    if (!response.IsSuccessStatusCode) return Results.BadRequest(body);

        //    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(body);
        //    accessToken = tokenResponse?.access_token;
        //    refreshToken = tokenResponse?.refresh_token;

        //    return Results.Ok(tokenResponse);
        //});

        app.MapPost("/api/auth/exchange", async (HttpRequest request) =>
        {
            var form = await request.ReadFromJsonAsync<AuthCodeDto>();
            if (form?.Code is null || string.IsNullOrEmpty(form.UserId)) return Results.BadRequest("Missing code or userId");

            var tokenResponse =  ConstantContactTokenHelpers.ExchangeCodeForTokenAsync(form.Code);
            if (tokenResponse is null) return Results.BadRequest("Token exchange failed");

            ConstantContactTokenHelpers.SaveToken(tokenResponse, form.UserId);
            return Results.Ok(tokenResponse);
        });
    }
        
}