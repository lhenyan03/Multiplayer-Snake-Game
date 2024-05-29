//Author: Hudson Bowman and Lindsey Henyan
//Last Updated: November 2023
//Creates the graphics and operations of the death animation
using Microsoft.Maui.Graphics;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Model;
internal class DeathAnimation : GameObject
{
    //keeps track of how many game ticks the animation has been alive for
    private int ticks = 0;
    private double velocity = 10;    //pixels per frame to move each particle. Decrease each frame
    private List<Vector2D> positions = new();   //keep list of elipses to draw and make them further each tick for a few ticks
    private Random random = new();

    /// <summary>
    /// base constructor for the animation
    /// </summary>
    /// <param name="model"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public DeathAnimation(Model model, int x, int y) : base(model, "")
    {
        X = x;
        Y = y;
        for (int n = 0; n < 6; n++)
        {
            positions.Add(new Vector2D(X, Y));  //make n particles all start at the position of the snake
        }
    }

    /// <summary>
    /// Draws the animation to the canvas
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (velocity < .1)
            return;
        int size = 10 + (ticks * 1 / 10);
        canvas.FillColor = Colors.Red;
        foreach (var position in positions)
        {
            canvas.FillEllipse((float)(position.X - (size/2)), (float)(position.Y - (size/2)), size, size);
            //offset each particle to a random angle
            double angle = (random.NextDouble()-.5) * 2 * double.Pi; 
            double XOffset = Math.Sin(angle) * velocity;
            double YOffset = Math.Cos(angle) * velocity;
            position.X += XOffset;
            position.Y += YOffset;
        }
        velocity *= .95;
        ticks += 1;
    }

    /// <summary>
    /// DeathAnimation doesnt need to have an ID 
    /// </summary>
    /// <returns></returns>
    public override int getId()
    {
        return -1;
    }
}
