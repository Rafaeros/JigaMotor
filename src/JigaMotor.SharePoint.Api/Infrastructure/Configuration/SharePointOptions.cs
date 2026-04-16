namespace JigaMotor.SharePoint.Api.Infrastructure.Configuration;
public class SharePointOptions
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SiteUrl { get; set; } = string.Empty;
    public string ListName { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
