//Authors: Hudson Bowman and Lindsey Henyan
//Last Updated: November 2023
//This class creates the Model for the snake client
using Microsoft.Maui.Graphics;
using System.Text.Json;
using SnakeGame;

namespace Model;

public class Model
{
    readonly Dictionary<int, Snake> snakes = new(); //contains all the snakes in the world
    readonly Dictionary<int, Wall> walls = new(); //contains all the walls in the world
    readonly Dictionary<int, Powerup> powerups = new(); //contains all the powerups in the world

    public Dictionary<string, IImage> imageBuffer = new(); //contains all the images needed to be loaded

    /// <returns>The image from the loaded images buffer</returns>
    /// <exception cref="Exception"></exception>
    internal IImage getImage(string imgName)
    {
        if (!imageBuffer.ContainsKey(imgName))
        {
            //if img does not exist, throw error
            throw new Exception("Img: " + imgName + " does not exist within buffer");
        }

        return imageBuffer[imgName];
    }

    /// <summary>
    /// Creates a new model
    /// </summary>
    public Model()
    {
    }

    /// <summary>
    /// Deconstructs some given JSON into a GameObject to draw
    /// </summary>
    /// <param name="json">The JSON string to deconstruct</param>
    /// <returns>true if successful, false if not</returns>
    public bool DeconstructJson(string json)
    {
        if (json == "" || (json.Count(c => c == '{') != json.Count(c => c == '}'))) //exits the method if there is something wrong with the string
            return false;

        int indexOfFirstQuote = json.IndexOf("\"");
        int indexOfSecondQuote = json.Substring(indexOfFirstQuote+1).IndexOf("\"");
        //The name of the object is between the first two quotes, so we can use that to find it
        string name = json.Substring(indexOfFirstQuote+1, indexOfSecondQuote);

        //identifies what object the json it is and create the appropriate objects
        switch (name)
        {
            case "snake":
                {
                    Snake? snake = JsonSerializer.Deserialize<Snake>(json);

                    if (snake is not null)
                    {
                        if (snake.died || snake.alive)
                        {
                            snake.model = this;
                            snakes[snake.getId()] = snake;
                        }
                        return true;
                    }
                } break;

            case "wall":
                {
                    Wall? wall = JsonSerializer.Deserialize<Wall>(json);

                    if(wall is not null)
                    {
                        wall.model = this;
                        walls[wall.getId()] = wall;
                        return true;
                    }
                }break;

            case "power": 
                { 
                   Powerup? powerup = JsonSerializer.Deserialize<Powerup>(json);

                    if(powerup is not null)
                    {
                        powerup.model = this;
                        powerups[powerup.getId()] = powerup;
                        return true;
                    }
                } break;
            default: break;
        }

        return false;
    }

    /// <summary>
    /// Draws all GameObjects to the canvas
    /// </summary>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        //lock each Dictionary because they may be added to at the same time they are drawn
        lock (snakes)
        {
            //draws every snake
            foreach (Snake s in snakes.Values)
                s.Draw(canvas, dirtyRect);
        }
        lock (powerups)
        {
            //draws every powerup
            foreach (Powerup p in powerups.Values)
                p.Draw(canvas, dirtyRect);
        }
        lock(walls)
        {
            //draws every wall
            foreach (Wall w in walls.Values)
                w.Draw(canvas, dirtyRect);
        }
    }

    /// <param name="playerId"></param>
    /// <returns>Vector2D representing the coordinates of the players snakes head</returns>
    public Vector2D GetPlayerXY(int playerId)
    {
        if(!snakes.ContainsKey(playerId) || snakes.Count == 0) 
            return new Vector2D(0, 0);      //if no snakes have been added, return nothing
        return snakes[playerId].GetHeadCoords();
    }
}


