//Authors: Hudson Bowman and Lindsey Henyan
//Last Updated: November 2023
//This class creates powerups from the GameObject Class for the Snake Client 
using Microsoft.Maui.Graphics;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model;

public class Powerup : GameObject
{
    [JsonInclude]
    public int power = 0; //powerups unique id
    [JsonInclude]
    public Vector2D loc = new(); //location
    [JsonInclude]
    public bool died = false; //flag for if it needs to be drawn

    private double targetAngle = 0, currentAngle = 0;

    /// <summary>
    /// Creates a new powerup using the JSON constructor
    /// </summary>
    [JsonConstructor]
    public Powerup(int power, Vector2D loc, bool died) : base(null!, "powerup.png")
    {
        this.power = power;
        this.died = died;
        this.loc = loc;

        X = loc.GetX();
        Y = loc.GetY();
    }

    /// <summary>
    /// Draws the powerup to the canvas
    /// </summary>
    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (died)
            return;
        canvas.FillColor = Colors.Coral;
        int size = 10;
        canvas.FillEllipse((float)(X-size/2),(float)(Y-size/2), size, size);
    }

    /// <summary>
    /// </summary>
    /// <returns>The ID of the powerup</returns>
    public override int getId()
    {
        return power;
    }

    /// <summary>
    /// Moves the powerup in a random angle
    /// </summary>
    /// <param name="worldSize"></param>
    public void MoveInRandomDirection(int worldSize)
    {
        int distToMove = 5; //pixels to move each frame
        int degreesPerFrame = 3;    //degrees to change each frame

        if(currentAngle == targetAngle)     //if the angle reaches the target
        {
            Random rand = new();    //choose a new target
            targetAngle = rand.NextDouble() * 360;
        }
        double distance = Math.Min(Math.Abs(targetAngle - currentAngle), (currentAngle + targetAngle) % 360);   //calculates the distance between the angles
        if(distance <= degreesPerFrame) //if it is one tick away, set angle to target
        {
            currentAngle = targetAngle;
        } else
        {       //otherwise move angle towards target. This makes the movement smooth between frames
            if(targetAngle - currentAngle > 0)
            {
                //add to current
                currentAngle += degreesPerFrame;
            }
            else
            {
                //subtract from current
                currentAngle -= degreesPerFrame;
            }

            if(currentAngle < 0)    //wrap angle around so that 0 <= angle <= 360
            {
                currentAngle = 360 + currentAngle;
            }
            if(currentAngle > 360)
            {
                currentAngle = currentAngle - 360;
            }
        }

        double xOffset = Math.Cos(currentAngle * Math.PI / 180);    //calculate the offsets based on the angle
        double yOffset = Math.Sin(currentAngle * Math.PI / 180);

        X += (int)(distToMove * xOffset);   //add them to the x and y values
        Y += (int)(distToMove * yOffset);

        
        if (X > worldSize/2)
            X = -worldSize/2+5;
        if(X < -worldSize/2)
            X = worldSize/2-5;
        if (Y > worldSize / 2)
            Y = -worldSize / 2 + 5;
        if (Y < -worldSize / 2)
            Y = worldSize / 2 - 5;

        loc.X = X;
        loc.Y = Y;

    }
}
