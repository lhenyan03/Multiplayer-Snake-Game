//Authors: Hudson Bowman and Lindsey Henyan
//Last Updated: November 2023
//This class creates the maui program for the snake client
namespace SnakeGame;

public static class MauiProgram
{
    /// <summary>
    /// creates the intitial view
    /// </summary>
    /// <returns>build</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }

}

