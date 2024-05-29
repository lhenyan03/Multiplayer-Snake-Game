//Authors: Hudson Bowman and Lindsey Henyan
//Last Updated: November 2023
//This class is the controller of the Snake Client.
// is responsible for parsing information received from the NetworkController, and updating the model. After updating the model, 
//it should then "inform" the View that the world has changed, so that it can redraw.

using NetworkUtil;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace GameController
{
    public class Controller
    {
        
        private SocketState? state; //creates the socket state
        public int worldSize = -1, playerId = -1; //placeholders for world size and player ID
        private Model.Model model; //creates a model object
        public delegate void OnFrame(); //delegate for what is to be drawn
        OnFrame reDraw; //using the onframe delegate to redraw

        /// <summary>
        /// Creates a new controller with the given model and a method to call to re-draw each frame
        /// </summary>
        public Controller(Model.Model model, OnFrame onFrame)
        {
            reDraw = onFrame;
            this.model = model;
        }

        /// <summary>
        /// Connects to the server
        /// </summary>
        public bool SetupServerConnection(string ip, int port, string name)
        {
            bool wasCalled = false;
            void toCall(SocketState x)
            {
                state = x;
                state.OnNetworkAction = OnServerRecieve;
                wasCalled = true;
            }

            Networking.ConnectToServer(toCall, ip, port);   //should take at most 3 seconds to connect or timeout

            //wait for the connection to be successful or not
            while (!wasCalled)
            {
                Thread.Sleep(10);
            }

            if(state is null || state!.ErrorOccurred)
            {
                return false;   //failed to connect to server
            } else
            {
                Networking.Send(state.TheSocket, name + "\n");  //send player name to server
                
                Networking.GetData(state);
                return true;
            }
        }

        /// <summary>
        /// Gets data from the server and sends it to the model
        /// </summary>
        /// <param name="s"></param>
        /// <exception cref="Exception"></exception>
        private void OnServerRecieve(SocketState s)
        {

            string data = s.GetData();

            //if there was multiple things passed into the data, we can split them into tokens since each token ends with a \n
            string[] tokens = data.Split("\n");

            foreach (string token in tokens)
                if(token != "")
                if (!token.StartsWith("{")) //All the data is in JSON except the player ID and worldSize
                {
                    //token is either worldSize or playerID
                    if (playerId == -1)
                    {
                        if (!int.TryParse(token, out playerId))
                        {
                            throw new Exception("Failed to get player id from string: " + token);
                        }
                    } else if(worldSize == -1)
                    {
                        if (!int.TryParse(token, out worldSize))
                        {
                            throw new Exception("Failed to get world size from string: " + token);
                        }
                    }
                }
                else 
                    model.DeconstructJson(token);   //send JSON to model

            if(data.Length > 0)
                reDraw();   //re-draws the canvas after getting the data

            //clear the data from the buffer
            s.RemoveData(0, data.Length);

            Networking.GetData(state);  //continue
        }        

        /// <returns>The ID of the player given from the server. -1 if not recieved yet</returns>
        public int GetPlayerId()
        {
            return playerId;
        }

        /// <returns>The controllers reference to the model</returns>
        public Model.Model GetModel()
        {
            return model;
        }

        /// <summary>
        /// Sends a string of data to the server
        /// </summary>
        /// <param name="direction"></param>
        public void Send(string direction)
        {
            //ensure that the player ID, world size and walls are not null before data can be sent
            if (playerId == -1 || worldSize == -1)
                return;
            if (state is not null)
            {
                Networking.Send(state.TheSocket, direction+"\n");
            }
        }
    }
}