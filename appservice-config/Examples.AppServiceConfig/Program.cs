using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using System;

namespace Examples.AppServiceConfig
{
    public class Program
    {
        public const string AzureKeyVaultUrlAppSettingName = "AzureKeyVaultUrl";

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var builtConfig = config.Build();

                    // If in production, authenticate with Key Vault using a Managed Identity and load all Key Vault values as configuration
                    // https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-2.2#use-managed-identities-for-azure-resources
                    if (context.HostingEnvironment.IsProduction())
                    {
                        // builds the local / app settings config so that the keyvault URL can be retrieved
                        if (string.IsNullOrEmpty(builtConfig[AzureKeyVaultUrlAppSettingName]))
                            throw new InvalidOperationException($"App Setting \"{AzureKeyVaultUrlAppSettingName}\" is missing.");

                        // Managed Identity provider
                        var azureServiceTokenProvider = new AzureServiceTokenProvider();
                        var keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(
                                azureServiceTokenProvider.KeyVaultTokenCallback));

                        // All Secrets in KeyVault will be loaded into Configuration at this point.
                        config.AddAzureKeyVault(
                            builtConfig[AzureKeyVaultUrlAppSettingName],
                            keyVaultClient,
                            new DefaultKeyVaultSecretManager());
                    }
                })
                .UseStartup<Startup>();
    }
}
