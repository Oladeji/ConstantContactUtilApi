namespace ConstantContactUtilApi.EndPoints
{
  
     public    record ContactDto(string Email, string FirstName, string LastName);
  //  public record AuthCodeDto(string Code);
    public record TokenResponse(string access_token, string refresh_token, int expires_in);

    public record AuthCodeDto(string Code, string UserId);

    public record AccessTokenEntry(string AccessToken, string? RefreshToken, long Expiry);

}
