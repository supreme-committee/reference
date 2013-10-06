using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Entities.Prefabs;

namespace GameProject
{
    /// <summary>
    /// Manages the players User Interface. 
    /// </summary>
    public class UI
    {
        /// <summary>
        /// Holds the owning game of this UI.
        /// </summary>
        Game1 game;

        /// <summary>
        /// Holds our spriteBatch.
        /// </summary>
        SpriteBatch spriteBatch;

        /// <summary>
        /// Font to use for UI text output.
        /// </summary>
        SpriteFont Font1;

        /// <summary>
        /// Holds the level manager.
        /// </summary>
        LevelManager levelManager;

        /// <summary>
        /// Holds our graphics device.
        /// </summary>
        GraphicsDevice graphics;

        /// <summary>
        /// Holds our camera.
        /// </summary>
        Camera camera;

        // Keep our viewport height and width
        int viewportHeight;
        int viewportWidth;

        /// <summary>
        /// Holds the buttons that the UI currently has.
        /// </summary>
        List<Button> buttons;

        /// <summary>
        /// Holds our main screen texture.
        /// </summary>
        public Texture2D mainScreen;

        /// <summary>
        /// Logo for Dark Crossroads.
        /// </summary>
        Texture2D DCLogo;
        int steps = 1;
        bool stepsPositive = true;
        int image = 0;

        /// <summary>
        /// Logo for Bepu.
        /// </summary>
        Texture2D BepuLogo;

        /// <summary>
        /// Splash textures.
        /// </summary>
        List<Texture2D> splashes;
        int currentSplash = 1;
        int maxSplash = 6;

        /// <summary>
        /// Overlay for our pause.
        /// </summary>
        Texture2D overlay;

        /// <summary>
        /// Menu music.
        /// </summary>
        SoundEffect sound1;
        SoundEffectInstance menuMusic;

        /// <summary>
        /// Level music.
        /// </summary>
        SoundEffect sound2;
        SoundEffectInstance levelMusic;

        /// <summary>
        /// Determines if our sound is playing.
        /// </summary>
        public bool sound;

        /// <summary>
        /// Button that deletes balls.
        /// </summary>
        public Button deleteButton;

        public UI(Game1 game, LevelManager levelManager, ContentManager Content, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            this.game = game;
            this.graphics = graphics;
            this.viewportHeight = graphics.Viewport.Height;
            this.viewportWidth = graphics.Viewport.Width;
            this.camera = levelManager.camera;

            this.levelManager = levelManager;

            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = spriteBatch;

            buttons = new List<Button>();
            
            Font1 = Content.Load<SpriteFont>("Font1");

            mainScreen = Content.Load<Texture2D>("Textures\\logo1920x1080");
            overlay = Content.Load<Texture2D>("Textures\\overlay");
            DCLogo = Content.Load<Texture2D>("Textures\\DC");
            BepuLogo = Content.Load<Texture2D>("Textures\\bepu");

            splashes = new List<Texture2D>();
            for(int i = 0; i < maxSplash; i++) 
            {
                splashes.Add(Content.Load<Texture2D>("Textures\\splash" + i));
            }

            sound1 = Content.Load<SoundEffect>("Cue 1");
            menuMusic = sound1.CreateInstance();
            menuMusic.IsLooped = true;

            sound2 = Content.Load<SoundEffect>("Cue 2");
            levelMusic = sound2.CreateInstance();
            levelMusic.IsLooped = true;
            levelMusic.Volume = 0.25f;
        }

        /// <summary>
        /// Updates our UI.
        /// </summary>
        /// <param name="oldKeyboardState"></param>
        /// <param name="keyboardState"></param>
        /// <param name="oldMouseState"></param>
        /// <param name="mouseState"></param>
        public void Update(KeyboardState oldKeyboardState, KeyboardState keyboardState, MouseState oldMouseState, MouseState mouseState)
        {
            if (game.currentState == GameProject.Game1.GameState.INTRO)
            {
                if (oldKeyboardState.IsKeyDown(Keys.Escape) && keyboardState.IsKeyUp(Keys.Escape))
                {
                    loadMenu();
                }

                if (stepsPositive)
                {
                    steps += 2;
                }
                else
                {
                    steps -= 2;
                }

                if (steps > 240)
                {
                    stepsPositive = false;
                }
                else if (steps < 0)
                {
                    if (image == 0)
                    {
                        image = 1;
                        steps = 1;
                        stepsPositive = true;
                    }
                    else if (image == 1)
                    {
                        loadMenu();
                    }
                }
            }
            else if (game.currentState == GameProject.Game1.GameState.LEVELS)
            {
                if (deleteButton == null)
                {
                    deleteButton = new Button("Delete Ball", new Vector2(400, viewportHeight - 20), Color.CornflowerBlue, Color.White, Font1, levelManager.gravitationalFieldManager.deleteField);
                }
                else
                {
                    deleteButton.buttonFunction = levelManager.gravitationalFieldManager.deleteField;
                }

                if (levelManager.character.CharacterController.SupportFinder.HasSupport)
                {
                    if (levelMusic.Volume > 0.35f)
                    {
                        levelMusic.Volume -= 0.005f;
                    }
                }
                else
                {
                    if (levelMusic.Volume < 0.7f)
                    {
                        levelMusic.Volume += 0.005f;
                    }
                }

                if (oldKeyboardState.IsKeyDown(Keys.Escape) && keyboardState.IsKeyUp(Keys.Escape))
                {
                    if (levelManager.paused)
                    {
                        levelManager.paused = false;
                        buttons.Clear();
                    }
                    else
                    {
                        levelManager.paused = true;
                        buttons.Add(new Button("Resume", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 50), Color.CornflowerBlue, Color.White, Font1, resume));
                        buttons.Add(new Button("Main Menu", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 100), Color.CornflowerBlue, Color.White, Font1, loadMenu));
                        buttons.Add(new Button("Exit", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 150), Color.CornflowerBlue, Color.White, Font1, game.Exit));
                    }
                }
            }
            else if (game.currentState == GameProject.Game1.GameState.SPLASH)
            {
                if ((oldKeyboardState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyUp(Keys.Enter)) || (oldKeyboardState.IsKeyDown(Keys.Space) && keyboardState.IsKeyUp(Keys.Space)))
                {
                    if (currentSplash < maxSplash)
                    {
                        currentSplash++;
                    }
                    else
                    {
                        currentSplash = 1;
                    }

                    game.currentState = GameProject.Game1.GameState.LEVELS;
                }
            }

            if (oldMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
            {
                Button clickedButton = null;
                foreach (Button button in buttons)
                {
                    if (button.isClicked(mouseState))
                    {
                        clickedButton = button;
                        break;
                    }
                }

                if (clickedButton != null)
                {
                    clickedButton.buttonFunction();
                }

                if (deleteButton != null)
                {
                    if (deleteButton.isClicked(mouseState))
                    {
                        if (!levelManager.paused)
                        {
                            deleteButton.buttonFunction();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Draws our UI.
        /// </summary>
        public void Draw(LevelManager levelManager, MouseState oldMouseState)
        {
            spriteBatch.Begin();

            // Draw our text
            string output;
            if (game.currentState == GameProject.Game1.GameState.INTRO)
            {
                if (image == 0)
                {
                    spriteBatch.Draw(DCLogo, new Rectangle((viewportWidth / 2) - (DCLogo.Width / 2), (viewportHeight / 2) - (DCLogo.Height / 2), DCLogo.Width, DCLogo.Height), new Color(steps, steps, steps));
                }
                else if (image == 1)
                {
                    spriteBatch.Draw(BepuLogo, new Rectangle((viewportWidth / 2) - ((BepuLogo.Width) / 2), (viewportHeight / 2) - ((BepuLogo.Height) / 2), BepuLogo.Width, BepuLogo.Height), Color.White);
                    output = "This game was made possible by the BEPU Physics Library.";
                    Vector2 FontOrigin2 = Font1.MeasureString(output) / 2;
                    spriteBatch.DrawString(Font1, output, new Vector2(viewportWidth / 2, viewportHeight / 2 + 150), Color.CornflowerBlue,
                        0, FontOrigin2, 1.0f, SpriteEffects.None, 0.5f);
                }
            }
            else if (game.currentState == GameProject.Game1.GameState.LEVELS)
            {
                if (levelManager.paused)
                {
                    spriteBatch.Draw(overlay, new Rectangle((viewportWidth / 2) - (overlay.Width / 2), (viewportHeight / 2) - (overlay.Height / 2), overlay.Width, overlay.Height), Color.White);
                }

                if (levelManager.gravitationalFieldManager.myFields.Count > 0 && levelManager.gravitationalFieldManager.currentField != null && !levelManager.paused)
                {
                    Sphere sphere = levelManager.gravitationalFieldManager.currentField.sphere.entity as Sphere;
                    Vector3 position = graphics.Viewport.Project(sphere.Position, camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

                    output = "Strength: " + levelManager.gravitationalFieldManager.currentField.Multiplier.ToString();
                    Vector2 FontOrigin2 = Font1.MeasureString(output) / 2;
                    spriteBatch.DrawString(Font1, output, new Vector2(position.X, position.Y - 100 * sphere.Radius), Color.LightGreen,
                        0, FontOrigin2, 1.0f, SpriteEffects.None, 0.5f);
                }

                if (levelManager.gravitationalFieldManager.EditMode)
                {
                    output = "Edit Mode ";
                    Vector2 FontOrigin2 = Font1.MeasureString(output) / 2;
                    spriteBatch.DrawString(Font1, output, new Vector2(viewportWidth - 100, viewportHeight - 20), Color.LightGreen,
                        0, FontOrigin2, 1.0f, SpriteEffects.None, 0.5f);
                }

                output = levelManager.gravitationalFieldManager.numFields + " / " + levelManager.gravitationalFieldManager.maxFields + " balls placed";
                Vector2 FontOrigin = Font1.MeasureString(output) / 2;
                spriteBatch.DrawString(Font1, output, new Vector2(150, viewportHeight - 20), Color.LightGreen,
                    0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);

                if (levelManager.gravitationalFieldManager.fieldsActive)
                {
                    output = "Gravity On";
                    Vector2 FontOrigin2 = Font1.MeasureString(output) / 2;
                    spriteBatch.DrawString(Font1, output, new Vector2(viewportWidth - 100, viewportHeight - 20), Color.LightGreen,
                        0, FontOrigin2, 1.0f, SpriteEffects.None, 0.5f);
                }

                if (deleteButton != null)
                {
                    deleteButton.Draw(spriteBatch, oldMouseState);
                }
            }
            else if(game.currentState == GameProject.Game1.GameState.SPLASH)
            {
                spriteBatch.Draw(splashes[currentSplash - 1], new Rectangle(0, 0, viewportWidth, viewportHeight), Color.White);

                output = "Press ENTER to continue...";
                Vector2 FontOrigin = Font1.MeasureString(output) / 2;
                spriteBatch.DrawString(Font1, output, new Vector2(200, viewportHeight - 20), Color.White,
                    0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            }
            else
            {
                spriteBatch.Draw(mainScreen, new Rectangle(0, 0, viewportWidth, viewportHeight), Color.White);
            }

            foreach (Button button in buttons)
            {
                button.Draw(spriteBatch, oldMouseState);
            }

            spriteBatch.End();
        }

        public void loadMenu()
        {
            game.currentState = GameProject.Game1.GameState.MENU;

            if (sound)
            {
                levelMusic.Stop();
                menuMusic.Play();
            }

            buttons.Clear();
            deleteButton = null;
            buttons.Add(new Button("New Game", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 100), Color.CornflowerBlue, Color.White, Font1, LoadLevel1));
            if (game.currentLevel > 1)
            {
                buttons.Add(new Button("Resume", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 150), Color.CornflowerBlue, Color.White, Font1, resumeFromMenu));
                buttons.Add(new Button("Level Select", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 200), Color.CornflowerBlue, Color.White, Font1, loadLevelMenu));
                buttons.Add(new Button("Options", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 250), Color.CornflowerBlue, Color.White, Font1, loadOptionsMenu));
                buttons.Add(new Button("Exit", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 300), Color.CornflowerBlue, Color.White, Font1, game.Exit));
            }
            else
            {
                buttons.Add(new Button("Level Select", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 150), Color.CornflowerBlue, Color.White, Font1, loadLevelMenu));
                buttons.Add(new Button("Options", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 200), Color.CornflowerBlue, Color.White, Font1, loadOptionsMenu));
                buttons.Add(new Button("Exit", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 250), Color.CornflowerBlue, Color.White, Font1, game.Exit));
            }
        }

        public void loadLevelMenu()
        {
            buttons.Clear();

            buttons.Add(new Button("Level 1", new Vector2(viewportWidth * 0.25f, (viewportHeight / 2) + 100), Color.CornflowerBlue, Color.White, Font1, LoadLevel1));
            buttons.Add(new Button("Level 2", new Vector2(viewportWidth * 0.25f, (viewportHeight / 2) + 150), Color.CornflowerBlue, Color.White, Font1, LoadLevel2));
            buttons.Add(new Button("Level 3", new Vector2(viewportWidth * 0.25f, (viewportHeight / 2) + 200), Color.CornflowerBlue, Color.White, Font1, LoadLevel3));
            buttons.Add(new Button("Level 4", new Vector2(viewportWidth * 0.25f, (viewportHeight / 2) + 250), Color.CornflowerBlue, Color.White, Font1, LoadLevel4));
            buttons.Add(new Button("Level 5", new Vector2(viewportWidth * 0.25f, (viewportHeight / 2) + 300), Color.CornflowerBlue, Color.White, Font1, LoadLevel5));
            buttons.Add(new Button("Level 6", new Vector2(viewportWidth * 0.5f, (viewportHeight / 2) + 100), Color.CornflowerBlue, Color.White, Font1, LoadLevel6));
            buttons.Add(new Button("Level 7", new Vector2(viewportWidth * 0.5f, (viewportHeight / 2) + 150), Color.CornflowerBlue, Color.White, Font1, LoadLevel7));
            buttons.Add(new Button("Level 8", new Vector2(viewportWidth * 0.5f, (viewportHeight / 2) + 200), Color.CornflowerBlue, Color.White, Font1, LoadLevel8));
            buttons.Add(new Button("Level 9", new Vector2(viewportWidth * 0.5f, (viewportHeight / 2) + 250), Color.CornflowerBlue, Color.White, Font1, LoadLevel9));
            buttons.Add(new Button("Level 10", new Vector2(viewportWidth * 0.5f, (viewportHeight / 2) + 300), Color.CornflowerBlue, Color.White, Font1, LoadLevel10));
            buttons.Add(new Button("Level 11", new Vector2(viewportWidth * 0.75f, (viewportHeight / 2) + 100), Color.CornflowerBlue, Color.White, Font1, LoadLevel11));
            buttons.Add(new Button("Level 12", new Vector2(viewportWidth * 0.75f, (viewportHeight / 2) + 150), Color.CornflowerBlue, Color.White, Font1, LoadLevel12));
            buttons.Add(new Button("Main Menu", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 350), Color.CornflowerBlue, Color.White, Font1, loadMenu));
        }

        public void loadOptionsMenu()
        {
            buttons.Clear();

            buttons.Add(new Button("Sound On", new Vector2(viewportWidth * 0.4f, (viewportHeight / 2) + 100), Color.CornflowerBlue, Color.White, Font1, SoundOn));
            buttons.Add(new Button("Sound Off", new Vector2(viewportWidth * 0.6f, (viewportHeight / 2) + 100), Color.CornflowerBlue, Color.White, Font1, SoundOff));
            buttons.Add(new Button("Main Menu", new Vector2(viewportWidth / 2, (viewportHeight / 2) + 350), Color.CornflowerBlue, Color.White, Font1, loadMenu));
        }

        public void LoadLevel1()
        {
            LoadLevel(1);
        }

        public void LoadLevel2()
        {
            LoadLevel(2);
        }

        public void LoadLevel3()
        {
            LoadLevel(3);
        }

        public void LoadLevel4()
        {
            LoadLevel(4);
        }

        public void LoadLevel5()
        {
            LoadLevel(5);
        }

        public void LoadLevel6()
        {
            LoadLevel(6);
        }

        public void LoadLevel7()
        {
            LoadLevel(7);
        }

        public void LoadLevel8()
        {
            LoadLevel(8);
        }

        public void LoadLevel9()
        {
            LoadLevel(9);
        }

        public void LoadLevel10()
        {
            LoadLevel(10);
        }

        public void LoadLevel11()
        {
            LoadLevel(11);
        }

        public void LoadLevel12()
        {
            LoadLevel(12);
        }

        public void SoundOn()
        {
            sound = true;
            game.playerData.Sound = true;
            menuMusic.Play();

            game.Save(game.playerData);
        }

        public void SoundOff()
        {
            sound = false;
            game.playerData.Sound = false;
            menuMusic.Stop();

            game.Save(game.playerData);
        }

        public void LoadLevel(int level)
        {
            game.currentState = GameProject.Game1.GameState.LEVELS;
            levelManager.paused = false;

            if (sound)
            {
                menuMusic.Stop();
                levelMusic.Play();
            }

            if (game.currentLevel != level)
            {
                levelManager.unloadLevel();
                game.currentLevel = level;
                levelManager.LoadLevel(game.currentLevel);
            }

            buttons.Clear();
        }

        public void resumeFromMenu()
        {
            game.currentState = GameProject.Game1.GameState.LEVELS;
            levelManager.paused = false;

            if (sound)
            {
                menuMusic.Stop();
                levelMusic.Play();
            }

            buttons.Clear();
        }

        public void loadSplash()
        {
            game.currentState = GameProject.Game1.GameState.SPLASH;

            game.playerData.Level = game.currentLevel;
            game.Save(game.playerData);
        }

        public void resume()
        {
            levelManager.paused = false;
            buttons.Clear();
        }

        public void loadIntro()
        {
            game.currentState = GameProject.Game1.GameState.INTRO;
        }
    }
}
