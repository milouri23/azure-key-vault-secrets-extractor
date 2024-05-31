namespace AzureKeyVaultSecretsExtractor.Console;

public sealed class AzureKeyVaultSettings
{
    public string TenantId { get; set; } = string.Empty;

    public string AppId { get; set; } = string.Empty;

    public string AppSecret { get; set; } = string.Empty;

    public string KeyVault {  get; set; } = string.Empty;
}
