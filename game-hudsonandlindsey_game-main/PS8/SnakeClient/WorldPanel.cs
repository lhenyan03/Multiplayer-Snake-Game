//Authors: Hudson Bowman and Lindsey Henyan
//Last Updated: November 2023
//This class creates the world panel for the snake game
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using Windows.Devices.PointOfService;
using Model;
using GameController;
using Microsoft.UI.Xaml.Media.Animation;



namespace SnakeGame;
public class WorldPanel : IDrawable
{
    private IImage background; //background object
    public Model.Model model; //model object
    public Controller Control; // controller object
    

    private bool initializedForDrawing = false; //flag for when to draw

    /// <summary>
    /// Loads the images from the resources file
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeClient.Resources.Images";
        using (Stream stream = assembly.GetManifestResourceStream($"{path}.{name}"))
        {
#if MACCATALYST
            return PlatformImage.FromStream(stream);
#else
            return new W2DImageLoadingService().FromStream(stream);
#endif
        }
    }

    /// <summary>
    /// Loads all of the images into a dictionary
    /// </summary>
    /// <returns>Dictionary of all images</returns>
    public Dictionary<string, IImage> loadImages()
    {
        var dictionary = new Dictionary<string, IImage>();
        string[] toLoad = { "wallsprite.png" };  //add names of all img to load

        foreach(string name in toLoad)
        {
            dictionary[name] = loadImage(name);
        }

        return dictionary;
    }

    /// <summary>
    /// empty constructor for the panel
    /// </summary>
    public WorldPanel()
    {
    }

    /// <summary>
    /// Loads images to be drawn
    /// </summary>
    private void InitializeDrawing()
    {
        background = loadImage( "background.png" );
        model.imageBuffer = loadImages();

        initializedForDrawing = true;
    }

    /// <summary>
    /// Draws everything to the canvas
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if ( !initializedForDrawing )
            InitializeDrawing();

        // undo previous transformations from last frame
        canvas.ResetState();

        int viewSize = 900;
        
        //translates the view to be centered on the snake
        if(model is not null)
        {
            var playerPos = model.GetPlayerXY(Control.GetPlayerId());
            canvas.Translate((float)(-playerPos.X + (viewSize / 2)), (float)(-playerPos.Y + (viewSize / 2)));
            canvas.DrawImage(background, -Control.worldSize/2, -Control.worldSize/2, Control.worldSize, Control.worldSize);
            model.Draw(canvas, dirtyRect);
        }


    }

}
