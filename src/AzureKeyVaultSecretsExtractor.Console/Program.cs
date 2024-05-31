using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureKeyVaultSecretsExtractor.Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddProvider(new SimpleConsoleLoggerProvider());
});
ILogger logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("========== Application Start ==========");
logger.LogInformation("");

// Build configuration
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

AzureKeyVaultSettings? settings = configuration
    .GetRequiredSection("AzureKeyVaultSettings")
    .Get<AzureKeyVaultSettings>();

if (settings == null)
{
    throw new InvalidOperationException("Azure Key Vault settings are not configured properly.");
}

logger.LogInformation("========== Azure Key Vault Settings ==========");
logger.LogInformation("TenantId: {TenantId}", settings.TenantId);
logger.LogInformation("AppId: {AppId}", settings.AppId);
logger.LogInformation("AppSecret: {AppSecret}", settings.AppSecret);
logger.LogInformation("KeyVault: {KeyVault}", settings.KeyVault);
logger.LogInformation("");

// Initialize SecretClient
SecretClient client = new(
    vaultUri: new Uri(settings!.KeyVault),
    credential: new ClientSecretCredential(settings.TenantId, settings.AppId, settings.AppSecret)
);

IConfigurationRoot keyVaultSecrets = new ConfigurationBuilder()
    .AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions())
    .Build();

IConfigurationSection secretsSection = configuration.GetSection("Secrets");

logger.LogInformation("========== Secrets ==========");

if (secretsSection.Exists() && secretsSection.GetChildren().Any())
{
    var secretKeys = secretsSection.AsEnumerable().Skip(1).Select(x => x.Value);

    foreach (string? secretKey in secretKeys)
    {
        if (secretKey is null) continue;

        string? secretValue = keyVaultSecrets.GetValue<string>(secretKey);

        if (secretValue is null) continue;

        logger.LogInformation("");
        logger.LogInformation("{SecretKey}", secretKey);
        logger.LogInformation("{SecretValue}", secretValue);
    }
}
else
{
    foreach (var item in keyVaultSecrets.AsEnumerable())
    {
        logger.LogInformation("");
        logger.LogInformation("{SecretKey}", item.Key);
        logger.LogInformation("{SecretValue}", item.Value);
    };
}

logger.LogInformation("");
logger.LogInformation("========== Application End ==========");
