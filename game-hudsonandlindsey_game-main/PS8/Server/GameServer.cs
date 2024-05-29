//Author: Hudson Bowman and Lindsey Henyan
//Last Updated: December 2023
//This class operates as the server for the Snake Client handling all of the user data and providing the 
//operations of the game.

using System.Xml;
using System.Xml.Serialization;
using Model;
using SnakeGame;
using NetworkUtil;
using System.Text.Json;
using System.Transactions;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Runtime.ConstrainedExecution;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Server;
public class GameServer
{

    private int msPerFrame; //miliseconds per frame
    private int PixelsPerFrame = 8; //pixels to move snake per frame
    private int RespawnRate; //time it takes for something to respawn
    private int worldSize; //size of world
    private List<Wall> walls; //list of all walls in the world
    public List<Snake> snakes; //list of all snakes in the world
    private List<Powerup> powerups; //list of all powerups in the world
    internal List<Client> clients; //list of all the clients connected to the server
    private string wallJson; //Json for walls

    private bool doSpecialFeature = false;
    private int specialFeatureId = 0;

    private int powerupCount = 0; //counter for powerups
    private int maxPowerups = 20; //number of max powerups

    private Dictionary<int, long> deathTimes = new();   //maps a snake id to the time it died



    public static void Main(string[] args)
    {
        var s = new GameServer();

    }

    /// <summary>
    /// GameServer Constructor
    /// </summary>
    public GameServer()
    {
        walls = new();
        snakes = new();
        powerups = new();
        clients = new();
        wallJson = "";

        ReadFile(); //load the data from the settings file

        Console.WriteLine("Ms/frame: " + msPerFrame);
        Console.WriteLine("Respawn rate: " + RespawnRate);
        Console.WriteLine("World size: " + worldSize);
        Console.WriteLine("Special feature enabled: " + doSpecialFeature);

        if(doSpecialFeature)    //allows for the special feature to be random
        {
            //Random rand = new();
            //if (rand.NextDouble() > 0.5)   //randombly choose between the 1st or second special feature
            //    specialFeatureId = 1;   //powerups move
            //else
            //    specialFeatureId = 2;   //snake reverses when it gets a powerup
            specialFeatureId = 1;
        }

        //because the walls are static we can get the json data once and store it instead of doing this for each
        //new client each time one connects.
        foreach (Wall w in walls)
        {
            wallJson += JsonSerializer.Serialize<Wall>(w)+"\n";
        }


        SetupServer();  //start listening for clients

        Stopwatch timer = new();    //keeps track of how long each frame is and when to send data for the next frame
        timer.Start();
        long lastFrame = 0L;
        long lastPowerupSpawn = 0L;
        long lastFPSMessage = 0L;

        while (true)    //perform infinite loop updating game each frame
        {
            //ensures that each frame has at least msPerFrame ms of time since last frame
            while (timer.ElapsedMilliseconds < lastFrame + msPerFrame)
            {
                //Thread.Sleep(1);
            }

            if (powerupCount <= maxPowerups && lastPowerupSpawn + RespawnRate < timer.ElapsedMilliseconds)
            {
                //spawns powerup after the respawn rate
                SpawnPowerup();
                lastPowerupSpawn = timer.ElapsedMilliseconds;
            }
               

            UpdateSnakes(timer.ElapsedMilliseconds); //updates snake every frame

            //recalculate time for next frame
            long timeSinceLastFrame = timer.ElapsedMilliseconds - lastFrame;
            float framesPerSecond = 0;
            if (timeSinceLastFrame != 0)
                framesPerSecond = 1000f / timeSinceLastFrame;
            lastFrame = timer.ElapsedMilliseconds;

            //prints the fps every 1000 ms (1 second)
            if (lastFPSMessage + 1000 < timer.ElapsedMilliseconds)
            {
                lastFPSMessage = timer.ElapsedMilliseconds;
                Console.WriteLine("FPS: " + framesPerSecond);
            }


            //get list of all the data each client needs before sending data so that it is
            //not computed for every client
            List<string> dataToSend = new();

            //lock each list in case they are being modified by other threads
            lock (snakes)
            {
                foreach (Snake snake in snakes)
                {
                    if (!snake.dc)
                    {
                        //if the client has disconnected, make the snake die and mark it as dc
                        if (clients[snake.getId()].HasError())
                        {
                            snake.dc = true;
                            snake.alive = false;
                            snake.died = false;
                            Console.WriteLine("Client(" + snake.getId() + ") Disconnected.");
                        }
                        dataToSend.Add(JsonSerializer.Serialize<Snake>(snake));
                    }
                }
            }

            lock (powerups) //applying special feature to powerup
            {
                foreach (Powerup powerup in powerups)
                {
                    if (doSpecialFeature && specialFeatureId == 1 && !powerup.died)
                    {
                        powerup.MoveInRandomDirection(worldSize);
                    }
                    dataToSend.Add(JsonSerializer.Serialize<Powerup>(powerup));
                }
                    
            }

            //send the data to each client
            lock (clients)
            {
                foreach (Client c in clients)
                {
                    if (!c.HasError())
                        foreach (string data in dataToSend)
                            c.sendClientData(data);
                }
            }

        }
    }

    /// <summary>
    /// Loads the ServerSettings file information
    /// </summary>
    internal void ReadFile()
    {

        using (XmlReader xmlReader = XmlReader.Create(@"ServerSettings.xml"))
        {
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement())
                {
                    switch (xmlReader.Name.ToString())
                    {
                        case "MSPerFrame":
                            msPerFrame = int.Parse(xmlReader.ReadString());
                            break;
                        case "RespawnRate":
                            RespawnRate = int.Parse(xmlReader.ReadString());
                            break;
                        case "UniverseSize":
                            worldSize = int.Parse(xmlReader.ReadString());
                            break;
                        case "PixelsPerFrame":
                            int.TryParse(xmlReader.ReadString(), out PixelsPerFrame);
                            break;
                        case "SpecialFeatureEnabled":
                            bool.TryParse(xmlReader.ReadString(), out doSpecialFeature);
                            break;
                        case "ID":
                            //load a wall. Each wall starts with an ID, then the next 4 ints are the coordinates for the wall
                            int id = int.Parse(xmlReader.ReadString());
                            while (xmlReader.Name.ToString() != "x")
                                xmlReader.Read();
                            int p1x = int.Parse(xmlReader.ReadString());
                            while (xmlReader.Name.ToString() != "y")
                                xmlReader.Read();
                            int p1y = int.Parse(xmlReader.ReadString());
                            while (xmlReader.Name.ToString() != "x")
                                xmlReader.Read();
                            int p2x = int.Parse(xmlReader.ReadString());
                            while (xmlReader.Name.ToString() != "y")
                                xmlReader.Read();
                            int p2y = int.Parse(xmlReader.ReadString());
                            Vector2D p1 = new(p1x, p1y);
                            Vector2D p2 = new(p2x, p2y);
                            walls.Add(new(id, p1, p2));
                            break;
                    }
                }
            }
        }
    }

    ///starts the server
    private void SetupServer()
    {
        Console.WriteLine("Starting server, Now Accepting Clients.");
        Networking.StartServer(NewClient, 11000);
    }

    /// <summary>
    /// Handles new clients when they connect to server
    /// </summary>
    /// <param name="state"></param>
    private void NewClient(SocketState state)
    {
        lock (clients)
        {
            int id = clients.Count;
            Console.WriteLine("New client(" + id + ") Joined");
            clients.Add(new(id, state, this));
        }
    }

    /// <summary>
    /// Getter for the Walls in JSON format
    /// </summary>
    /// <returns></returns>
    public string WallJson()
    {
        return wallJson;
    }

    /// <summary>
    /// Getter for WorldSize
    /// </summary>
    /// <returns></returns>
    public int WorldSize()
    {
        return worldSize;
    }


    /// <summary>
    /// Gets a snake at a specific id
    /// </summary>
    public Snake GetSnake(int id)
    {
        if (snakes[id] is not null)
            return snakes[id];
        throw new Exception("No snake at ID: " + id);
    }

    /// <summary>
    /// Updates the snake each frame
    /// </summary>
    private void UpdateSnakes(long time)
    {
        //move all of the snakes, then check for collision
        lock (snakes)   //maybe dont need
        {
            foreach (Snake snake in snakes)
            {
                if (snake.died)
                {
                    deathTimes[snake.getId()] = time;
                    snake.died = false; //want this to only be true for one frame
                }
                    
                if (snake.alive)
                {
                    snake.Move(PixelsPerFrame, worldSize);

                    lock (walls)
                    {
                        foreach (Wall wall in walls)
                            if (snake.CheckColAgainstWall(wall)) //checks if snake collides with wall
                                break;
                    }

                    foreach (Snake otherSnake in snakes)    //check the snake colision against all other snakes
                    {
                        if (snake != otherSnake)    //make sure to not check against self here
                        {
                            if (snake.CheckColAgainstSnake(otherSnake)) //if it collides with another snake, end the loop
                                break;
                        }
                        else
                            snake.CheckSnakeColAgainstSelf();
                    }

                    if (!snake.alive)   //if the snake collided with another snake we dont need to do any more checking
                        continue;

                    lock (powerups)
                    {
                        foreach(Powerup power in powerups)
                        {

                            if (power.died) //powerup eaten
                                continue;

                            if(snake.CollidesWith(power.loc, 10)){ //powerup eaten by snake, update snake
                                snake.EatPowerup();
                                powerupCount--;
                                power.died = true;
                            }
                        }
                    }
                } else
                {
                    //if the snake is dead and it has been more ms than the respawn time, respawn the snake
                    if (!deathTimes.ContainsKey(snake.getId()))
                        continue;
                    if (deathTimes[snake.getId()] + RespawnRate < time)
                    {
                        lock(snakes)
                        {
                            spawnSnake(snake.getId(), snake.name);
                        }
                        
                    }
                }
            }
        }
    }

    /// <summary>
    /// Spawns powerups in random locations
    /// </summary>
    private void SpawnPowerup()
    {
        Random rand = new Random();
        while (true)
        {
            //randomizes x y coords within world
            double x = (rand.NextDouble() - .5) * worldSize * .95;
            double y = (rand.NextDouble() - .5) * worldSize * .95;
            Vector2D powerLoc = new(x, y);

            bool foundCoord = true;
            foreach (Wall wall in walls)
            {
                if (wall.CollidesWithWall(powerLoc, 10)) //if it collides with coords find new coords
                {
                    foundCoord = false;
                    break;
                }
            }
            if(foundCoord)
            { 
                //if its collision free add it to the world and update counter
                Powerup power = new(powerups.Count, powerLoc, false);
                powerups.Add(power);
                powerupCount++;
                return;
            }
        }
    }


    /// <summary>
    /// Spawns new Snake
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    public void spawnSnake(int id, string name)
    {
        Random rand = new Random();
        int snakeStartLength = PixelsPerFrame * 10;

        Vector2D dir = new(0,0);
        int randomDir = rand.Next(4);
        

        while (true)
        {
            //randomizes coordinates for spawn
            double x = (rand.NextDouble() - .5) * worldSize;
            double y = (rand.NextDouble() - .5) * worldSize;
            Vector2D snakeLoc = new(x, y);

            bool foundCoord = true;
            foreach (Wall wall in walls)
            {
                if (wall.CollidesWithWall(snakeLoc, snakeStartLength)) //checks if the coordinates collide with any walls
                {
                    foundCoord = false;
                    break;
                }
            }
            if (foundCoord)
            {
                //if its collision free randomize what direction the snake will face
                var secondSegment = new Vector2D(x, y);
                if (randomDir == 0)
                {
                    //up
                    dir.Y = -1;
                    secondSegment.Y += snakeStartLength;
                }
                else if (randomDir == 1)
                {
                    //down
                    dir.Y = 1;
                    secondSegment.Y -= snakeStartLength;
                }
                else if (randomDir == 2)
                {
                    //right
                    dir.X = 1;
                    secondSegment.X -= snakeStartLength;
                }
                else if (randomDir == 3)
                {
                    //left
                    dir.X = -1;
                    secondSegment.X += snakeStartLength;
                }

                List<Vector2D> body = new List<Vector2D>() { secondSegment, snakeLoc}; //adds new segment to body

                if(snakes.Count <= id)
                {
                    //create new
                    Snake snake = new Snake(id, name, body, dir, 0, false, true, false, true);
                    snakes.Add(snake);
                }
                else
                {
                    //update existing
                    snakes[id].respawn(body, dir);
                }
                return;
            }
        }
    }


}