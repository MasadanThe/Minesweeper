using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.IO;

namespace Minesweeper
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        string state = "Game";
        List<Ruta> Grid = new List<Ruta>();
        int bombs = 75;
        Texture2D UnClickedTexture;
        Texture2D ClickedTexture;
        Texture2D FlagTexture;
        Texture2D WhiteBackground;

        bool lost = false;
        bool changed;

        private SpriteFont font;

        int clicks = 0;

        Random random = new Random();

        public Game1()
        {
            IsMouseVisible = true; //Gör musen synlig
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 600;
            graphics.PreferredBackBufferHeight = 600;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here



            //Create Grid
            for (int i = 0; i < 20; i++)
            {
                for (int ii = 0; ii < 20; ii++)
                {
                    Grid.Add(new Ruta(ii*30, i*30));
                }
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Crea jte a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            font = Content.Load<SpriteFont>("File");

            UnClickedTexture = Content.Load<Texture2D>("Oklickad_ruta");
            ClickedTexture = Content.Load<Texture2D>("Blank_ruta");
            FlagTexture = Content.Load<Texture2D>("Flaggad_ruta");
            WhiteBackground = Content.Load<Texture2D>("VitBakgrund");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //If you have won the game
            if (clicks >= 400 - bombs)
            {
                //state = "Victory";
                lost = true;
                StreamWriter streamWriter = new StreamWriter("File.txt", true);
                streamWriter.WriteLine(clicks + "/" + (400 - bombs));
                streamWriter.Close();
                //this.Exit();
            }

            if (state == "Game" && lost == false)
            {
                //Start interaction by ai
                if (clicks == 0)
                {
                    int startXPos = random.Next(30) * 20;
                    int startYPos = random.Next(30) * 20;
                    Click(startXPos, startYPos, Grid, "Enter");
                }

                else
                {
                    UpdateBoxes();
                    changed = false;
                    foreach (Ruta ruta in Grid)
                    {
                        
                        if (ruta.unclickedBoxesNear != 0 && ruta.clickedBoxesNear > 0 && ruta.clicked == false)
                        {
                            for (int y = ruta.YPos - 30; y <= ruta.YPos + 30; y += 30)
                            {
                                for (int x = ruta.XPos - 30; x <= ruta.XPos + 30; x += 30)
                                {
                                    foreach (Ruta nextRuta in Grid)
                                    {
                                        if (nextRuta.XPos == x && nextRuta.YPos == y)
                                        {
                                            if (nextRuta.flaggedBoxesNear == nextRuta.bombsNear && nextRuta.clicked == true)
                                            {
                                                Click(ruta.XPos, ruta.YPos, Grid, "Enter");
                                            }

                                            else if (nextRuta.unclickedBoxesNear == nextRuta.bombsNear - nextRuta.flaggedBoxesNear && ruta.flag == false)
                                            {
                                                Click(ruta.XPos, ruta.YPos, Grid, "Space");
                                            }
                                            
                                        }
                                    }
                                    UpdateBoxes();

                                }
                            }
                        }   
                        
                    }
                    while (changed == false)
                    {
                        int startXPos = random.Next(30) * 20;
                        int startYPos = random.Next(30) * 20;
                        Click(startXPos, startYPos, Grid, "Enter");
                    }

                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            if (state == "Game")
            {
                //Draw buttons
                foreach (Ruta t in Grid)
                {
                    if (t.texture == "Oklickad_ruta")
                    {
                        spriteBatch.Draw(UnClickedTexture, new Vector2(t.XPos, t.YPos));

                    }
                    if (t.texture == "Blank_ruta")
                    {
                        spriteBatch.Draw(ClickedTexture, new Vector2(t.XPos, t.YPos));
                        if (t.bombsNear >= 0)
                        {
                            spriteBatch.DrawString(font, t.bombsNear.ToString(), new Vector2(t.XPos + 8, t.YPos), Color.Black);
                        }
                    }
                    if (t.texture == "Flaggad_ruta")
                    {
                        spriteBatch.Draw(FlagTexture, new Vector2(t.XPos, t.YPos));
                    }

               
                }
            }

            if (state == "REKT")
            {
                spriteBatch.Draw(WhiteBackground, new Vector2(0, 0));
                spriteBatch.DrawString(font, "You pressed: " + clicks.ToString() + "/" + (Grid.Count - bombs).ToString(), new Vector2(160, 250), Color.Black);
            }

            if (state == "Victory")
            {
                spriteBatch.Draw(WhiteBackground, new Vector2(0, 0));
                spriteBatch.DrawString(font, "VICTORY", new Vector2(160, 250), Color.Black);
            }

            // TODO: Add your drawing code here
            spriteBatch.End();
            base.Draw(gameTime);
        }

        void Click(int cursorX, int cursorY, List<Ruta> HQGrid, string click)
        {
            foreach (Ruta t in HQGrid)
            {
                for (int y = t.YPos; y < t.YPos + t.height; y++)
                {
                    for (int x = t.XPos; x < t.XPos + t.width; x++)
                    {
                        if (cursorX == x && cursorY == y)
                        {
                            Debug.WriteLine("Xpos: " + x);
                            Debug.WriteLine("Ypos: " + y);
                            switch (t.texture)
                            {
                                
                                case "Oklickad_ruta":
                                    changed = true;
                                    if (click == "Enter")
                                    {
                                        string surroundingBombs = t.EnterClick();
                                        
                                        if (clicks == 0)
                                        {
                                            x = t.XPos;
                                            y = t.YPos;
                                            Debug.WriteLine("X|Y: " + x + "|" + y);
                                            InitializeBombs(x, y);
                                        }
                                        
                                        else if (t.bomb == true)
                                        {
                                            Debug.WriteLine("Lost");
                                            lost = true;
                                            state = "REKT";
                                            StreamWriter streamWriter = new StreamWriter("File.txt", true);
                                            streamWriter.WriteLine(clicks + "/" + (400 - bombs));
                                            streamWriter.Close();
                                            this.Exit();
                                            //SendData();
                                        }

                                        clicks++;
                                        if (surroundingBombs != "bomb" && surroundingBombs != "okay")
                                        {
                                            string[] postions = surroundingBombs.Split('S');

                                            foreach (Ruta otherRuta in Grid)
                                            {
                                                for (int y_Pos = int.Parse(postions[1]) - 30; y_Pos <= int.Parse(postions[1]) + 30; y_Pos += 30)
                                                {

                                                    for (int x_Pos = int.Parse(postions[0]) - 30; x_Pos <= int.Parse(postions[0]) + 30; x_Pos += 30)
                                                    {
                                                        if (otherRuta.clicked == false && otherRuta.XPos == x_Pos && otherRuta.YPos == y_Pos)
                                                        {
                                                            Click(x_Pos, y_Pos, Grid, "Enter");
                                                        }
                                                        
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    else if (click == "Space")
                                    {
                                        t.SpaceClick();
                                    }
                                    break;
                                case "Flaggad_ruta":
                                    if (click == "Space")
                                    {
                                        t.SpaceClick();
                                    }
                                    break;
                                
                            }


                        }

                    }
                }

            }
        }

        void InitializeBombs(int xCo, int yCo)
        {

            //Gör så att varje ruta runt den första tryckningen inte innehåller en bomb
            for (int y = yCo - 30; y <= yCo + 30; y += 30)
            {

                for (int x = xCo - 30; x <= xCo + 30; x += 30)
                {
                    foreach (Ruta ruta in Grid)
                    {
                        if (ruta.XPos == x && ruta.YPos == y)
                        {
                            ruta.noBomb = true;
                        }
                    }
                }
            }

            //Placerar bomber
            for (int n = 0; n < bombs; n++)
            {
                while (true)
                {
                    int randomRuta = random.Next(Grid.Count);
                    if (Grid[randomRuta].noBomb == false && Grid[randomRuta].bomb == false)
                    {
                        Grid[randomRuta].bomb = true;
                        break;
                    }
                }
            }

            //Kollar hur många bomber som kretsar runt varje ruta
            foreach (Ruta otherRuta in Grid)
            {
                for (int y = otherRuta.YPos - 30; y <= otherRuta.YPos + 30; y += 30)
                {

                    for (int x = otherRuta.XPos - 30; x <= otherRuta.XPos + 30; x += 30)
                    {
                        foreach (Ruta nextRuta in Grid)
                        {
                            if (nextRuta.XPos == x && nextRuta.YPos == y && nextRuta.bomb == true)
                            {
                                otherRuta.bombsNear++;
                            }
                        }
                    }
                }
                
            }

        }

        void SendData()
        {
            try
            {
                string address = "127.0.0.1";
                int port = 8001;

                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(address, port);
                Byte[] bmessage = Encoding.UTF8.GetBytes("Antalet öppnade rutor: " + clicks.ToString());
                Socket socket = tcpClient.Client;
                socket.Send(bmessage);
            }
            catch
            {

            }
        }

        
        void UpdateBoxes()//Uppdaterar infon om rutorna
        {
            foreach (Ruta ruta in Grid)
            {
                ruta.flaggedBoxesNear = 0;
                ruta.unclickedBoxesNear = 0;
                ruta.chance = 0;
                for (int y = ruta.YPos - 30; y <= ruta.YPos + 30; y += 30)
                {

                    for (int x = ruta.XPos - 30; x <= ruta.XPos + 30; x += 30)
                    {
                        foreach (Ruta nextRuta in Grid)
                        {
                            if (nextRuta.XPos == x && nextRuta.YPos == y )
                            {
                                if (nextRuta.flag == true)
                                {
                                    ruta.flaggedBoxesNear++;
                                }

                                else if (nextRuta.clicked == false)
                                {
                                    ruta.unclickedBoxesNear++;
                                }

                                else if (nextRuta.clicked == true)
                                {
                                    ruta.clickedBoxesNear++;
                                }

                            }
                        }
                    }
                }

            }
        }
    }
}
