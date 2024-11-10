using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Logging;

using TestingPlatform.Components;
using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Pages;
using TestingPlatform.Services.Api;
using TestingPlatform.Services.Graph;
using TestingPlatform.Services.Localization;
using TestingPlatform.Utilities;

namespace TestingPlatform
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
#if DEBUG
            if (OperatingSystem.IsWindows())
            {
                builder.Logging.ClearProviders();
                builder.Logging.AddNLog();
                builder.Logging.AddDebug();
            }
#endif
            builder.UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Nunito-Regular.ttf", "Nunito");
                    fonts.AddFont("Nunito-Bold.ttf", "NunitoBold");
                    fonts.AddFont("Nunito-Italic.ttf", "NunitoItalic");
                });

            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.appsettings.json")
                ?? throw new ApplicationException("Appsettings.json stream is null.");
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonStream(stream).Build();
            builder.Configuration.AddConfiguration(config);

            AzureAd azureAd = builder.Configuration.GetSection(nameof(AzureAd)).Get<AzureAd>()
                ?? throw new ApplicationException("Appsettings.json stream is null.");

            builder.Services.Configure<AzureAd>(options =>
            {
                options.ClientId = azureAd.ClientId;
                options.Tenant = azureAd.Tenant;
                options.RedirectUri = azureAd.RedirectUri;
                options.Instance = azureAd.Instance;
                options.Scopes = azureAd.Scopes;
            });

            ApiConfiguration apiConfiguration = builder.Configuration.GetSection(nameof(ApiConfiguration)).Get<ApiConfiguration>()
                ?? throw new ApplicationException("Appsettings.json stream is null.");

            builder.Services.Configure<ApiConfiguration>(options =>
            {
                options.Uri = apiConfiguration.Uri;
            });

            builder.Services.AddHttpClient(Constants.ApiHttpClient, client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    MaxConnectionsPerServer = 10
                };
            });

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<Home>();

            builder.Services.AddSingleton<LocalizationMenu>();
            builder.Services.AddSingleton<Login>();
            builder.Services.AddSingleton<LeftSideBar>();
            builder.Services.AddSingleton<HomeView>();

            builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
            builder.Services.AddSingleton<IGraphService, GraphService>();
            builder.Services.AddSingleton<IApiService, ApiService>();

            return builder.Build();
        }
    }
}
