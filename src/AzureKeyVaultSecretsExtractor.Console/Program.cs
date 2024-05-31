using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureKeyVaultSecretsExtractor.Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    //builder.AddConsole();
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
logger.LogInformation($"TenantId: {settings.TenantId}");
logger.LogInformation($"AppId: {settings.AppId}");
logger.LogInformation($"AppSecret: {settings.AppSecret}");
logger.LogInformation($"KeyVault: {settings.KeyVault}");
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
    foreach (KeyValuePair<string, string> secret in secretsSection.AsEnumerable().Skip(1))
    {
        logger.LogInformation("");
        string? secretValue = keyVaultSecrets.GetValue<string>(secret.Value);

        if (secretValue is null) continue;

        logger.LogInformation(secret.Value);
        logger.LogInformation(secretValue);
    }
}
else
{
    foreach (var item in keyVaultSecrets.AsEnumerable())
    {
        logger.LogInformation("");
        logger.LogInformation(item.Key);
        logger.LogInformation(item.Value);
    };
}

logger.LogInformation("");
logger.LogInformation("========== Application End ==========");
