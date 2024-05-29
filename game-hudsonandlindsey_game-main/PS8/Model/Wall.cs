//Authors: Hudson Bowman and Lindsey Henyan
//Last Updated: November 2023
//This class creates walls that are game objects for the snake client
using Microsoft.Maui.Graphics;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model
{
    public class Wall : GameObject
    {
        [JsonInclude]
        public int wall = 0; //walls unique id
        [JsonInclude]
        public Vector2D p1; // one endpoint of the wall
        [JsonInclude]
        public Vector2D p2; // one endpoint of the wall

        private int width; //width of walls
        private int height; //height of walls

        int numCols, numRows; //number of cols and rows of walls

        /// <summary>
        /// Creates a wall segment using the JSON constructor
        /// </summary>
        [JsonConstructor]
        public Wall(int wall, Vector2D p1, Vector2D p2) : base(null!, "wallsprite.png")
        {
            this.wall = wall;
            this.p1 = p1;
            this.p2 = p2;

            //calculates the number of columns and rows that the wall segment needs to draw
            numCols = (int)(Math.Abs(p1.X - p2.X) / 50) + 1;
            numRows = (int)(Math.Abs(p1.Y - p2.Y) / 50) + 1;

            //because we want to draw from the top left, we get the smaller value for the x/y so it draws to the right
            X = Math.Min(p1.X, p2.X);
            Y = Math.Max(p1.Y, p2.Y);

            width = (int)Math.Abs(p2.X - p1.X);
            
            height = (int)Math.Abs(p2.Y - p1.Y);
        }

        /// <summary>
        /// Draws the wall segment to the canvas
        /// </summary>
        public override void Draw(ICanvas canvas, RectF dirtyRect)
        {
            //because the wall can have multiple wall segment, we have to loop through the cols/rows and draw each one
            for (int i = 0; i < numCols; i++)
                for (int j = 0; j < numRows; j++)
                    canvas.DrawImage(model.getImage(imgName), (int)X - 25 + (50 * i), (int)Y - 25 - (50 * j), 50, 50);

        }

        /// <summary>
        /// Gets the ID of the wall segment
        /// </summary>
        /// <returns>The ID of the wall</returns>
        public override int getId()
        {
            return wall;
        }

        /// <summary>
        /// Determines if a point collides with a wall
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool CollidesWithWall(Vector2D point, int distance)
        {
            distance += 25;     //distance point can be from wall + 25 for the thickness of the wall
            if ((point.X >= X - distance &&     //if the point is between the x values of the wall
                point.X <= X + width + distance) &&

                (point.Y >= Y - height - distance &&     //and between the y values of the wall
                point.Y <= Y + distance)) return true;    //then it collides

            return false;
        }
    }
}
