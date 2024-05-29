//Author: Hudson Bowman and Lindsey Henyan
//Last Updated: November 2023
//An abstract class of Game Objects for the basic requirements of an object in the snake client game.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Model;

    public abstract class GameObject : IDrawable
    {
        internal double X { get; set; } //x coordinate
        internal double Y { get; set; } //y coordinate
        internal Model model; //model object
        protected string imgName; //name of image

    /// <summary>
    /// The default constructor for all GameObjects
    /// </summary>
    public GameObject(Model model, string imgName)
    {
        this.model = model;
        this.imgName = imgName;
        X = 0;
        Y = 0;
    }

    /// <summary>
    /// Returns the ID of the GameObject
    /// </summary>
    /// <returns>The ID of the GameObject</returns>
    public abstract int getId();

    /// <summary>
    /// Draws the GameObject to the screen
    /// </summary>
    public abstract void Draw(ICanvas canvas, RectF dirtyRect);
}

