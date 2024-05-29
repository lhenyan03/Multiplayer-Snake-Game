///Authors: Hudson Bowman and Lindsey Henyan
///Updated: December 2023
///This class handles all of the data received from the client and establishing the connection
using Model;
using NetworkUtil;
using SnakeGame;

namespace Server
{

    internal class Client
    {
        private int id;
        private SocketState ss;
        private GameServer server;
        private string? name;

        public Client(int id, SocketState ss, GameServer server)
        {
            this.id = id;
            this.ss = ss;
            this.server = server;
            ss.OnNetworkAction = GetClientName;
            Networking.GetData(ss);
        }

        private void OnServerRecieve(SocketState s)
        {

            if (ss.ErrorOccurred == true)
                return;

            string data = s.GetData();

            //if there was multiple things passed into the data, we can split them into tokens since each token ends with a \n
            string[] tokens = data.Split("\n");

            foreach (string token in tokens)
            {

                lock (server.snakes)
                {
                    if (token == "")
                        continue;
                    Snake snake = server.GetSnake(id);  //get the snake that this client moves

                    //checks movement of snake and changes to appropriate direction
                    if (token.Contains("none"))
                    {
                        //dont do anything
                    }
                    else if (token.Contains("up"))
                    {
                        if (snake.dir.Y == 1)   //prevents snake from doing a 180 turn
                            continue;
                        snake.ChangeDirection(new Vector2D(0, -1));
                    }
                    else if (token.Contains("left"))
                    {
                        if (snake.dir.X == 1)
                            continue;
                        snake.ChangeDirection(new Vector2D(-1, 0));

                    }
                    else if (token.Contains("down"))
                    {
                        if (snake.dir.Y == -1)
                            continue;
                        snake.ChangeDirection(new Vector2D(0, 1));

                    }
                    else if (token.Contains("right"))
                    {
                        if (snake.dir.X == -1)
                            continue;
                        snake.ChangeDirection(new Vector2D(1, 0));

                    }
                    else
                    {
                        //bad data
                        Console.WriteLine("Direction \"" + token + "\" from client: " + id + " is invalid");
                    }
                }
            }
            //clear the data from the buffer
            s.RemoveData(0, data.Length);
            Networking.GetData(ss);  //continue
        }

        /// An error Occured in the socketstate
        /// <returns>True if the client has an error</returns>
        public bool HasError()
        {
            return ss.ErrorOccurred;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void GetClientName(SocketState state)
        {
            //client sends name
            string data = state.GetData();
            //get the name of the player

            if (data.Contains("\n"))
            {
                name = data.Substring(0, data.IndexOf("\n"));
                //gets all of the ID, worldSize, and walls in one string to send
                lock (server.clients)
                {
                    sendClientData("" + id);
                    sendClientData(server.WorldSize() + "");
                    sendClientData(server.WallJson());
                }

                //make new snake
                lock (server.snakes)
                {
                    server.spawnSnake(id, name);
                }
                state.RemoveData(0, name.Length);   //remove name from data
                ss.OnNetworkAction = OnServerRecieve;
            }
            Networking.GetData(ss);  //continue
        }

        /// <summary>
        /// Sends the client a string of data
        /// </summary>
        /// <param name="data"></param>
        public void sendClientData(string data)
        {
            if (name == null)   //dont send the data if the client has not reported a name yet
                return;
            Networking.Send(ss.TheSocket, data + "\n");
        }
    }
}