//Author: Hudson Bowman and Lindsey Henyan
//Last Updated: November 2023
//This class creates snake objects using the gameobject interface for the snake client
using Microsoft.Maui.Graphics;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Model;

public class Snake : GameObject
{
    [JsonInclude]
    public int snake = 0; //snake id
    [JsonInclude]
    public string name = ""; //player name
    [JsonInclude]
    public List<Vector2D> body = new(); //list of coords for the body
    [JsonInclude]
    public Vector2D dir = new(); //direction of snake
    [JsonInclude]
    public int score = 0; //number of powerups eaten
    [JsonInclude]
    public bool died = false; //flag for if it dies and needs the death animation
    [JsonInclude]
    public bool alive = false; //flag for if alive
    [JsonInclude]
    public bool dc = false; //flag for disconnection
    [JsonInclude]
    public bool join = false; //flag for joining the server

    long deathTime = 0L;

    //keeps track of which direction each segment was going for checking snake collision against self
    private Dictionary<Vector2D, Vector2D> directions = new();

    //keeps track of how many times the snake has extended so that when a powerup is eaten it knows to not remove from tail
    private int numTailExtensions = 0;

    private int SegmentsPerPowerup = 5; //number of segments to add per powerup

    private DeathAnimation? Animation; //death animation object

    //list of all colors for the snakes to switch through
    private static Color[] SnakeColors = { Colors.Gold, Colors.Blue, Colors.Indigo, Colors.Green, Colors.MediumPurple, Colors.PaleVioletRed, Colors.MistyRose, Colors.Yellow };


    /// <summary>
    /// Creates a new snake using the JSON constructor
    /// </summary>
    [JsonConstructor]
    public Snake(int snake, string name, List<Vector2D> body, Vector2D dir, int score, bool died, bool alive, bool dc, bool join) : base(null!, "Snake.png")
    {
        this.snake = snake;
        this.name = name;
        this.body = body;
        this.dir = dir;
        this.score = score;
        this.died = died;
        this.alive = alive;
        this.dc = dc;
        this.join = join;
        directions[body[body.Count - 2]] = dir;
    }

    /// <summary>
    /// Returns the id of the snake
    /// </summary>
    /// <returns></returns>
    public override int getId()
    {
        return snake;
    }

    /// <summary>
    /// Draws each segment of the snake to the canvas
    /// </summary>
    /// <param name="canvas"></param>
    private void SnakeSegmentDrawer(ICanvas canvas)
    {

        canvas.FillColor = SnakeColors[getId() % SnakeColors.Length];

        int thickness = 10;     //adjust how thick the snake is drawn
        //coords are middle of segment, draw 5-10? pixels outwards from each endpoint
        int extend = thickness / 2; //num of pixels to extend on each end

        for (int cur = 1; cur < body.Count; cur++)
        {
            Vector2D current = body[cur];
            Vector2D last = body[cur - 1];
            float width = (int)(last.X - current.X);
            float height = (int)(last.Y - current.Y);
            bool vertical = width == 0;     //if the segment is moving upwards then the width will be 0

            //if the width or height is negative, its easier to draw from the previous segment. If the width/height is positve, draw from currrent segment
            if (width < 0 || height < 0)
            {
                if (vertical)    //if the segment is vertical it has different calculations from when it is horizontal
                    canvas.FillRoundedRectangle((float)last.X - (thickness / 2), (float)last.Y - extend, thickness, Math.Abs(height) + 2 * extend, extend);
                else
                    canvas.FillRoundedRectangle((float)last.X - extend, (float)last.Y - (thickness / 2), Math.Abs(width) + 2 * extend, thickness, extend);
            }
            else
            {
                if (vertical)
                    canvas.FillRoundedRectangle((float)current.X - (thickness / 2), (float)current.Y - extend, thickness, height + 2 * extend, extend);
                else
                    canvas.FillRoundedRectangle((float)current.X - extend, (float)current.Y - (thickness / 2), width + 2 * extend, thickness, extend);
            }

        }
    }

    /// <summary>
    /// Draws the snake along with the name to the canvas.
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!alive)
        {
            if (died && Animation == null || Animation == null)
            {
                var headPos = GetHeadCoords();
                Animation = new(model!, (int)headPos.X, (int)headPos.Y);
                Animation.Draw(canvas, dirtyRect);
                died = false;
            }
            else
                Animation!.Draw(canvas, dirtyRect);
            return;
        }
        SnakeSegmentDrawer(canvas);

        string message = name + ": " + score;
        var headCoords = GetHeadCoords();

        canvas.DrawString(message, (float)(headCoords.X), (float)(headCoords.Y + 15), HorizontalAlignment.Center);
    }

    /// <summary>
    /// gets the Vector2D that represents the head coordinates of the snake
    /// </summary>
    /// <returns></returns>
    public Vector2D GetHeadCoords()
    {
        return body[body.Count - 1];    //returns the second to last index of the body list
    }

    /// <summary>
    /// Checks if the head of the snake collides with a given point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool CollidesWith(Vector2D point, int width)
    {
        var head = GetHeadCoords();
        double dist = Math.Sqrt(Math.Pow(head.Y - point.Y, 2) + Math.Pow(head.X - point.X, 2));

        if (dist <= width)
            return true;
        return false;

    }

    private bool CheckColisionAgainstSegment(Vector2D cur, Vector2D next, int width)
    {
        double dist;
        var head = GetHeadCoords();

        if (dir.X < 0 || dir.X > 0) //moving left or right
        {
            //head is above or below segment
            if ((head.Y < cur.Y && head.Y < next.Y) || (head.Y > cur.Y && head.Y > next.Y))
            {

                dist = Math.Min(Math.Sqrt((head.Y * head.Y + cur.Y * cur.Y)), Math.Sqrt((head.Y * head.Y + next.Y * next.Y)));
               
                //calculates distance based on closest segment
                if (cur.Y < next.Y)
                    dist = Math.Sqrt((head.Y * head.Y + cur.Y * cur.Y));
                else dist = Math.Sqrt((head.Y * head.Y + next.Y * next.Y));
            }
            else //head is in line with the segment
            {
                dist = Math.Abs(head.X - cur.X);
            }
            //if distance is less than the pixel length to segment collide
            if (dist < width)
            {
                died = true;
                alive = false;
                return true;
            }
        }

        else if (dir.Y < 0 || dir.Y > 0) //moving up or down
        {
            //head is above or below segment
            if ((head.X < cur.X && head.X < next.X) || (head.X > cur.X && head.X > next.X))
            {
                dist = Math.Min(Math.Sqrt((head.X * head.X + cur.X * cur.X)), Math.Sqrt((head.X * head.X + next.X * next.X)));

                //calculates distance based on closest wall
                if (cur.X < next.X) 
                    dist = Math.Sqrt((head.X * head.X + cur.X * cur.X));
                else dist = Math.Sqrt((head.X * head.X + next.X * next.X));
            }
            else //head is in line with the segment
            {
                dist = Math.Abs(head.Y - cur.Y);

            }
            if (dist < width)
            {
                died = true;
                alive = false;
                return true;
            }

        }
        return false;
    }

    /// <summary>
    /// Checks the snake for collision against self
    /// </summary>
    public void CheckSnakeColAgainstSelf()
    {

        Vector2D head = GetHeadCoords();

        int index = -1;  //index to start checking against

        for (int i = body.Count - 2; i >= 1; --i)
        {
            //find part that is going away from head

            if (!directions.ContainsKey(body[i]))
                continue;
            var segmentDir = directions[body[i]];

            if (dir.IsOppositeCardinalDirection(segmentDir))
            {
                index = i;
                break;
            }

        }

        if (index != -1)
        {
            for (int i = 0; i < index - 1; i++) //might need to subtract 2 from index
            {
                //PUT STUFF BACK HERE
                CheckColisionAgainstSegment(body[i], body[i + 1], 8);
            }

        }

    }

    public bool CheckColAgainstSnake(Snake snake)
    {
        if (!alive || !snake.alive || dc || snake.dc)
            return false;
        for (int i = 0; i < snake.body.Count - 1; i++)
            if (CheckColisionAgainstSegment(snake.body[i], snake.body[i + 1], 10))
                return true;

        return false;
    }

    /// <summary>
    /// Moves the snake PixelsPerFrame length based on its current direction
    /// </summary>
    /// <param name="PixelsPerFrame"></param>
    /// <exception cref="Exception"></exception>
    public void Move(int PixelsPerFrame, int worldSize)
    {
        //add to head remove from tail
        //calc width and height of head segment
        Wraparound(PixelsPerFrame, worldSize);

        Vector2D head = body[body.Count - 1];
        Vector2D last = body[body.Count - 2];
        double width = Math.Abs(last.X - head.X);
        double height = Math.Abs(last.Y - head.Y);


        //if width is 0, snake is going up or down
        //if height is 0, snake is going right or left
        if (width == 0 && height == 0)
            throw new Exception("Width and height is 0");

        if (width == 0)
        {
            //up or down
            if (dir.X == 0)
            {
                //continue where snake is going
                head.Y += dir.Y * PixelsPerFrame;

            }
            else
            {
                //add new segment with width of PixelsPerFrame
                Vector2D newVector = new(head.X, head.Y);
                newVector.X += dir.X * PixelsPerFrame;
                body.Add(newVector);
            }
        }
        if (height == 0)
        {
            //left or right
            if (dir.Y == 0)
            {
                //continue where snake is going
                head.X += dir.X * PixelsPerFrame;
            }
            else
            {
                //add new segment with height of PixelsPerFrame
                Vector2D newVector = new(head.X, head.Y);
                newVector.Y += dir.Y * PixelsPerFrame;
                body.Add(newVector);
            }
        }

        //remove length from tail if there was not a powerup eaten
        if (score * SegmentsPerPowerup > numTailExtensions)
        {
            numTailExtensions++;
            return;
        }
        Vector2D tail = body[0];
        last = body[1];
        width = tail.X - last.X;
        height = tail.Y - last.Y;

        //adjusts length of snake based on powerups collected
        if (width == 0)
        {
            if (height > 0)
                tail.Y -= PixelsPerFrame;
            else
                tail.Y += PixelsPerFrame;
        }
        else if (height == 0)
        {
            if (width > 0)
                tail.X -= PixelsPerFrame;
            else
                tail.X += PixelsPerFrame;
        }

        if ((int)(tail.X) == (int)(last.X) && (int)(tail.Y) == (int)(last.Y))
        {
            body.Remove(tail);

            Vector2D newTail = body[0];
            Vector2D nextSegment = body[1]; //threw error once because body was only length 1 somehow? To duplicate try adding 50 ai clients
            width = Math.Abs(nextSegment.X - newTail.X);
            height = Math.Abs(nextSegment.Y - newTail.Y);

            if (width >= worldSize || height >= worldSize)
                body.Remove(newTail);
        }

    }

    /// <summary>
    /// Checks if the direction changes then changes it
    /// </summary>
    /// <param name="newDir"></param>
    public void ChangeDirection(Vector2D newDir)
    {
        if (dir.X != newDir.X || dir.Y != newDir.Y)
        {
            directions[GetHeadCoords()] = dir;
            dir = newDir;
        }
    }

    /// <summary>
    /// Checks for collision against lab
    /// </summary>
    /// <param name="wall"></param>
    /// <returns></returns>
    public bool CheckColAgainstWall(Wall wall)
    {
        if (wall.CollidesWithWall(GetHeadCoords(), 10))
        {
            died = true;
            alive = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// increments score if powerup was eaten
    /// </summary>
    public void EatPowerup()
    {
        score++;
    }


    /// <summary>
    /// respawns with a fresh snake but with the same id
    /// </summary>
    /// <param name="body"></param>
    /// <param name="dir"></param>
    public void respawn(List<Vector2D> body, Vector2D dir)
    {
        alive = true;
        died = false;
        score = 0;
        numTailExtensions = 0;
        this.body = body;
        this.dir = dir;
        directions[body[body.Count - 2]] = dir;
    }

    /// <summary>
    /// Checks if the snake hit the edge of the world
    /// </summary>
    /// <param name="worldSize"></param>
    /// <returns>direction</returns>
    public bool hitWorldEnd(int worldSize)
    {
        Vector2D head = GetHeadCoords();
        if (head.X < worldSize / 2 * -1 || head.X > worldSize / 2 ||
            head.Y < worldSize / 2 * -1 || head.Y > worldSize / 2)
            return true;
        return false;
    }

    public void Wraparound(int pixelsPerFrame, int worldSize)
    {

        if (hitWorldEnd(worldSize))
        {
            directions[GetHeadCoords()] = new(dir);
            Vector2D head = new(GetHeadCoords());
            Vector2D last = new(head);
            if (head.X < worldSize / 2 * -1 || head.X > worldSize / 2)
            {
                head.X = head.X * -1;
                if (head.X > 0)
                {
                    head.X = worldSize / 2;
                    last.X = head.X - pixelsPerFrame;
                }

                else
                {
                    head.X = -1 * worldSize / 2;
                    last.X = head.X + pixelsPerFrame;
                }

            }

            else
            {
                head.Y = head.Y * -1;
                if (head.Y > 0)
                {
                    head.Y = worldSize / 2;
                    last.Y = head.Y - pixelsPerFrame;
                }

                else
                {
                    head.Y = -1 * worldSize / 2;
                    last.Y = head.Y + pixelsPerFrame;
                }
            }


            body.Add(head);
            body.Add(last);
            directions[head] = new(dir);

        }
    }

}
