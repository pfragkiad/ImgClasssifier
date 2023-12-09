using ImgClasssifier.ControlExtensions;
using ImgClasssifier.Rating;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImgClasssifier;

internal static class Program
{
    static IHost? _host;
    public static IServiceProvider? Provider;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices( (context,services) =>
            {
                services
                .AddSingleton<Form1>()
                .Configure<BrowserOptions2>(context.Configuration.GetSection("browser"))
                .AddTransient<BrowserForm>()

                .AddSingleton<PictureRater>()
                .Configure<RaterOptions>(context.Configuration.GetSection("rater"))
                ;
            })
            .Build();

        Provider = _host.Services;

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(Provider.GetRequiredService<Form1>());
    }
}