using Microsoft.Extensions.Configuration;

namespace AzureKeyVaultSecretsExtractor.Console;

public static class ConfigurationExtensions
{
    public static T ResolveSecrets<T>(this IConfiguration configuration)
    {
        IConfigurationSection secretsSection = configuration.GetRequiredSection(typeof(T).Name);

        foreach (KeyValuePair<string, string> secret in secretsSection.AsEnumerable().Skip(1))
        {
            string secretValue = configuration.GetValue<string>(secret.Value);

            if (secretValue is null) continue;

            configuration[secret.Key] = secretValue;
        }

        return secretsSection.Get<T>();
    }
}
