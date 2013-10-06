using System;
using System.Collections.Generic;
using System.Linq;
using System.IO; 
using System.IO.IsolatedStorage; 
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GameProject
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        /// <summary>
        /// Holds the user interface.
        /// </summary>
        public UI ui;

        // Input states
        KeyboardState oldKeyboardState;
        MouseState oldMouseState;

        /// <summary>
        /// Holds the current level.
        /// </summary>
        public int currentLevel = 1;

        /// <summary>
        /// Our maximum level.
        /// </summary>
        int maxLevel = 12;

        /// <summary>
        /// Manages our current level. 
        /// </summary>
        LevelManager levelManager;

        /// <summary>
        /// Holds the game states.
        /// </summary>
        public enum GameState { INTRO, MENU, LEVELS, SPLASH };

        /// <summary>
        /// Holds our current state
        /// </summary>
        public GameState currentState;

        public struct SaveGameData
        {
            public int Level;
            public bool Sound;
        }

        public SaveGameData playerData;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferMultiSampling = true;

            this.graphics.PreferredBackBufferWidth = 1920;
            this.graphics.PreferredBackBufferHeight = 1080;

            this.graphics.IsFullScreen = true;

            this.IsMouseVisible = true;
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

            oldKeyboardState = Keyboard.GetState();
            oldMouseState = Mouse.GetState();

            // Load a save if they have it
            playerData = Load();
            if (playerData.Level != 1)
            {
                currentLevel = playerData.Level;
            }

            // Set up our levelmanager
            levelManager = new LevelManager(this, graphics, Content);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);    

            // Set up our UI
            ui = new UI(this, levelManager, Content, spriteBatch, GraphicsDevice);

            if (playerData.Sound == true)
            {
                ui.sound = true;
            }
            else
            {
                ui.sound = false;
            }

            // Load up our intro
            ui.loadIntro();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (currentState == GameState.LEVELS)
            {
                // Update our level
                bool win = levelManager.Update(oldKeyboardState, keyboardState, oldMouseState, mouseState, gameTime);

                // Check if the player has won
                if (win)
                {
                    levelManager.unloadLevel();
                    if (currentLevel == maxLevel)
                    {
                        currentLevel = 1;
                        ui.loadMenu();
                    }
                    else
                    {
                        currentLevel++;
                        ui.loadSplash();
                    }
                    levelManager.LoadLevel(currentLevel);
                }
            }

            // Update our UI
            ui.Update(oldKeyboardState, keyboardState, oldMouseState, mouseState);

            oldKeyboardState = keyboardState;
            oldMouseState = mouseState;

            // Hides and shows our mouse
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                this.IsMouseVisible = false;
            }
            if (mouseState.RightButton == ButtonState.Released)
            {
                this.IsMouseVisible = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (currentState == GameState.LEVELS)
            {
                // Reset the graphics device settings so our models aren't transparent (silly spriteBatch)
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                // Draw our Level
                levelManager.Draw();
            }

            // Draw our UI
            ui.Draw(levelManager, oldMouseState);

            base.Draw(gameTime);
        }

        public void Save(SaveGameData data)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            IsolatedStorageFileStream stream = storage.CreateFile("savegame.xml");

            XmlSerializer xml = new XmlSerializer(typeof(SaveGameData));
            xml.Serialize(stream, data);

            stream.Close();
            stream.Dispose();
        }

        public static SaveGameData Load()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            SaveGameData temp;

            if (storage.FileExists("savegame.xml"))
            {
                IsolatedStorageFileStream stream = storage.OpenFile("savegame.xml", FileMode.Open);
                XmlSerializer xml = new XmlSerializer(typeof(SaveGameData));

                temp = (SaveGameData)xml.Deserialize(stream);

                stream.Close();
                stream.Dispose();
            }
            else
            {
                temp.Level = 1;
                temp.Sound = true;
            }

            return temp;
        }
    }
}
