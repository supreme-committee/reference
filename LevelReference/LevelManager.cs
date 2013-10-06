using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Entities;
using BEPUphysics;
using BEPUphysics.DataStructures;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.UpdateableSystems.ForceFields;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace GameProject
{
    public struct PlatformInfo
    {
        public string direction;
        public Vector3 movement;
        public float seconds;
        public string texture;
        public int platformID;
    }

    public class LevelManager
    {
        /// <summary>
        /// Space that is used to represent physical entities. 
        /// </summary>
        public Space space;

        /// <summary>
        /// The camera that is used for display.
        /// </summary>
        public Camera camera;

        /// <summary>
        /// The character object that represents the player. 
        /// </summary>
        public CharacterControllerInput character;

        /// <summary>
        /// Manages our gravitational fields. 
        /// </summary>
        public GravitationalFieldManager gravitationalFieldManager;

        // Models and textures used
        public Model CubeModel;
        public Model SphereModel;
        public Model RobotModel;
        public Model SpikesModel;
        public Model CannonModel;
        public Texture2D CubeTexture;
        public Texture2D UpTexture;
        public Texture2D DownTexture;
        public Texture2D LeftTexture;
        public Texture2D RightTexture;
        public Texture2D SphereTexture;
        public Texture2D RobotTexture;
        public Texture2D SpikesTexture;
        public Texture2D CannonTexture;
        public Texture2D BoxTexture;

        public Texture2D KeyATexture;
        public Texture2D KeyDTexture;
        public Texture2D KeyETexture;
        public Texture2D KeyFTexture;
        public Texture2D KeyWTexture;
        public Texture2D KeySpaceTexture;
        public Texture2D LeftClickTexture;
        public Texture2D RightClickTexture;

        /// <summary>
        /// Contains our basic lighting information.
        /// </summary>
        public LightingEffect basicLighting;

        /// <summary>
        /// Highlight effects.
        /// </summary>
        public LightingEffect highLighting;
        public LightingEffect redLighting;
        public LightingEffect greenLighting;

        /// <summary>
        /// List of all entity/model pairs in the space.
        /// </summary>
        public List<EntityModel> Models;

        /// <summary>
        /// List of our platforms that give us a win.
        /// </summary>
        public List<Entity> winPlatforms;

        /// <summary>
        /// List of our platforms that give us a loss.
        /// </summary>
        public List<Entity> losePlatforms;

        /// <summary>
        /// List of our platforms that allow us to switch gravity.
        /// </summary>
        public List<Entity> switchPlatforms;

        /// <summary>
        /// List of platforms that wait for the user to touch them to move.
        /// </summary>
        public List<List<EntityModel>> elevators;

        /// <summary>
        /// List of loose objects that need to be updated when the gravity is changed.
        /// </summary>
        public List<EntityModel> looseObjects;

        /// <summary>
        /// List of our cannons.
        /// </summary>
        public List<Cannon> cannons;

        /// <summary>
        /// Keeps track of the projectiles created by the cannons.
        /// </summary>
        public List<EntityModel> projectiles;
        public List<EntityModel> projectilesToRemove;

        /// <summary>
        /// List of back walls.
        /// </summary>
        public List<EntityModel> backWalls;

        /// <summary>
        /// Determines if the game is paused.
        /// </summary>
        public bool paused = false;

        /// <summary>
        /// Representation of our character.
        /// </summary>
        EntityModel characterModel;

        /// <summary>
        /// Determines if the character is turned.
        /// </summary>
        bool characterTurnedLeft = false;
        bool characterTurnedRight = false;

        /// <summary>
        /// Holds our particle system.
        /// </summary>
        public ParticleSystem explosionParticles;
        public ParticleSystem explosionSmokeParticles;

        /// <summary>
        /// Holds our explosions.
        /// </summary>
        public List<Explosion> explosions;

        /// <summary>
        /// Explosion sound.
        /// </summary>
        public SoundEffect expSound;
        public SoundEffectInstance explosionSound;

        /// <summary>
        /// Holds the ID of the current platform.
        /// </summary>
        int currPlatformID = 0;

        /// <summary>
        /// Determines which elevator we are adding to.
        /// </summary>
        int currElevatorID = -1;

        /// <summary>
        /// Steps.
        /// </summary>
        int steps = 0;

        // Parent game of the levelManager
        public Game1 game;

        public LevelManager(Game1 game, GraphicsDeviceManager graphics,  ContentManager Content)
        {
            this.game = game;

            Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000);
            camera = new Camera(new Vector3(-20, 2, 0), 10, 0f, MathHelper.ToRadians(270), projectionMatrix, graphics.GraphicsDevice);

            Models = new List<EntityModel>();
            winPlatforms = new List<Entity>();
            losePlatforms = new List<Entity>();
            switchPlatforms = new List<Entity>();
            cannons = new List<Cannon>();
            elevators = new List<List<EntityModel>>();
            looseObjects = new List<EntityModel>();

            explosionParticles = new ExplosionParticleSystem(game, Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(game, Content);

            explosions = new List<Explosion>();

            projectiles = new List<EntityModel>();
            projectilesToRemove = new List<EntityModel>();

            backWalls = new List<EntityModel>();

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            explosionParticles.DrawOrder = 400;
            explosionSmokeParticles.DrawOrder = 200;

            // Audio for our explosion
            expSound = Content.Load<SoundEffect>("explosionSound");

            // Register the particle system components.
            game.Components.Add(explosionParticles);
            game.Components.Add(explosionSmokeParticles);

            // Load our models and textures
            CubeModel = Content.Load<Model>("Models\\Cube");
            CubeTexture = Content.Load<Texture2D>("Textures\\CubeTexture");

            UpTexture = Content.Load <Texture2D>("Textures\\UpTexture");
            DownTexture = Content.Load<Texture2D>("Textures\\DownTexture");
            LeftTexture = Content.Load<Texture2D>("Textures\\LeftTexture");
            RightTexture = Content.Load<Texture2D>("Textures\\RightTexture");
            BoxTexture = Content.Load<Texture2D>("Textures\\BoxTexture");

            KeyATexture = Content.Load<Texture2D>("Textures\\KeyA");
            KeyDTexture = Content.Load<Texture2D>("Textures\\KeyD");
            KeyETexture = Content.Load<Texture2D>("Textures\\KeyE");
            KeyFTexture = Content.Load<Texture2D>("Textures\\KeyF");
            KeyWTexture = Content.Load<Texture2D>("Textures\\KeyW");
            KeySpaceTexture = Content.Load<Texture2D>("Textures\\KeySpace");

            LeftClickTexture = Content.Load<Texture2D>("Textures\\LeftClick");
            RightClickTexture = Content.Load<Texture2D>("Textures\\RightClick");

            SphereModel = Content.Load<Model>("Models\\sphere");
            SphereTexture = Content.Load<Texture2D>("Textures\\sphere_map");

            RobotModel = Content.Load<Model>("Models\\Robot");
            RobotTexture = Content.Load<Texture2D>("Textures\\TexturedRobotSkin");

            SpikesModel = Content.Load<Model>("Models\\Spikes");
            SpikesTexture = Content.Load<Texture2D>("Textures\\SpikesSkin");

            CannonModel = Content.Load<Model>("Models\\cannon");
            CannonTexture = Content.Load<Texture2D>("Textures\\cannonSkin");

            // Load the basic shader
            basicLighting = new LightingEffect(Content.Load<Effect>("Shader"), camera, graphics);
            basicLighting.ambientLightIntensity = 0.2f;

            // Load our highlight effect
            highLighting = new LightingEffect(Content.Load<Effect>("Shader"), camera, graphics);
            highLighting.ambientLightIntensity = 0.4f;
            highLighting.ambientLightColor = Color.Yellow.ToVector4();

            // Smexy red light
            redLighting = new LightingEffect(Content.Load<Effect>("Shader"), camera, graphics);
            redLighting.ambientLightIntensity = 0.4f;
            redLighting.ambientLightColor = Color.Red.ToVector4();

            // Green light
            greenLighting = new LightingEffect(Content.Load<Effect>("Shader"), camera, graphics);
            greenLighting.ambientLightIntensity = 0.4f;
            greenLighting.ambientLightColor = Color.LawnGreen.ToVector4();

            // Create our space 
            space = new Space();

            // Load level  
            LoadLevel(game.currentLevel);
        }

        public bool Update(KeyboardState oldKeyboardState, KeyboardState keyboardState, MouseState oldMouseState, MouseState mouseState, GameTime gameTime)
        {
            bool win = false;

            if (!paused)
            {
                gravitationalFieldManager.Update(oldKeyboardState, keyboardState, oldMouseState, mouseState, backWalls);

                // Update our camera
                camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds, keyboardState, oldMouseState, mouseState, gravitationalFieldManager.EditMode);

                // Update our character
                character.Update((float)gameTime.ElapsedGameTime.TotalSeconds, oldKeyboardState, keyboardState, oldMouseState, mouseState);

                // Check if we need to reactivate the character after a death
                ResetUpdate();

                // Update our cannons
                List<Cannon> deadCannons = new List<Cannon>();
                foreach (Cannon cannon in cannons)
                {
                    cannon.Update();

                    if (cannon.alive == false)
                    {
                        deadCannons.Add(cannon);
                    }
                }

                foreach (Cannon cannon in deadCannons)
                {
                    Models.Remove(cannon.cannonEntity);
                    cannons.Remove(cannon);
                }

                // Update our projectiles
                foreach (EntityModel projectile in projectiles)
                {
                    foreach (CollidablePairHandler handler in projectile.entity.CollisionInformation.Pairs)
                    {
                        if (handler.Contacts.Count > 0)
                        {
                            projectilesToRemove.Add(projectile);
                        }

                        // Temp solution - only a projectile can kill its parent cannon
                        /* if (cannon.cannonEntity.entity.CollisionInformation.Pairs.Contains(handler))
                        {
                            alive = false;
                            projectilesToRemove.Add(projectile);
                        } */

                        // In level 9 the cannons are killing each other. need to figure out how to 
                        // make this work
                        foreach (Cannon cannon in cannons)
                        {
                            if (cannon.cannonEntity.entity.CollisionInformation.Pairs.Contains(handler))
                            {
                                cannon.alive = false;
                                projectilesToRemove.Add(projectile);
                            }
                        }
                        
                        // If our projectile hit a destructible object, remove it from the space
                        foreach (EntityModel item in looseObjects)
                        {
                            if (item.entity.CollisionInformation.Pairs.Contains(handler))
                            {
                                if (space.Entities.Contains(item.entity))
                                {
                                    space.Remove(item.entity);
                                    Models.Remove(item);
                                    projectilesToRemove.Add(projectile);
                                }
                            }
                        }
                    }
                }

                foreach (EntityModel projectile in projectilesToRemove)
                {
                    if (space.Entities.Contains(projectile.entity))
                    {
                        playExplosionSound(projectile.entity.Position);
                        explosions.Add(new Explosion(explosionParticles, explosionSmokeParticles, projectile.entity.Position));
                        space.Remove(projectile.entity);
                        Models.Remove(projectile);
                    }
                }
                projectilesToRemove.Clear();

                // Update our particles
                List<Explosion> explosionsToRemove = new List<Explosion>();
                foreach (Explosion explosion in explosions)
                {
                    if (!explosion.Update(gameTime))
                    {
                        explosionsToRemove.Add(explosion);
                    }
                }

                foreach (Explosion explosion in explosionsToRemove)
                {
                    explosions.Remove(explosion);
                }

                // Lock our loose objects to the Y-Z Plane. This also forces all our loose objects to update
                // on a gravity switch
                foreach (EntityModel item in looseObjects)
                {
                    if (item.entity.Position.X != 0)
                    {
                        item.entity.Position = new Vector3(0, item.entity.Position.Y, item.entity.Position.Z);
                    }
                }

                // Update our space
                space.Update();

                // Turn the character if needed
                if (character.alive && !characterTurnedLeft && keyboardState.IsKeyDown(Keys.A))
                {
                    if (!characterTurnedRight)
                    {
                        characterModel.Transform *= Matrix.CreateRotationY(MathHelper.ToRadians(-90f));
                    }
                    else
                    {
                        characterModel.Transform *= Matrix.CreateRotationY(MathHelper.ToRadians(-180f));
                        characterTurnedRight = false;
                    }
                    characterTurnedLeft = true;
                }

                if (character.alive && !characterTurnedRight && keyboardState.IsKeyDown(Keys.D))
                {
                    if (!characterTurnedLeft)
                    {
                        characterModel.Transform *= Matrix.CreateRotationY(MathHelper.ToRadians(90f));
                    }
                    else
                    {
                        characterModel.Transform *= Matrix.CreateRotationY(MathHelper.ToRadians(180f));
                        characterTurnedLeft = false;
                    }
                    characterTurnedRight = true;
                }

                // Update all our models
                foreach (EntityModel entityModel in Models)
                {
                    entityModel.Update();
                }

                // Check what the player is standing on
                foreach (SupportContact contact in character.CharacterController.SupportFinder.supports)
                {
                    // If the player hit some spikes
                    Collidable coll1 = contact.Support;
                    foreach (Entity entity in losePlatforms)
                    {
                        Collidable coll2 = entity.CollisionInformation;
                        if (coll1 == coll2)
                        {
                            Reset();
                        }
                    }

                    // If the player is standing on an elevator, activate it
                    foreach (List<EntityModel> elevator in elevators)
                    {
                        bool activate = false;
                        foreach (EntityModel elevatorPart in elevator)
                        {
                            Collidable coll2 = elevatorPart.entity.CollisionInformation;
                            if (coll1 == coll2)
                            {
                                activate = true;
                            }
                        }

                        if (activate)
                        {
                            foreach (EntityModel elevatorPart in elevator)
                            {
                                elevatorPart.isMoving = true;
                            }
                        }
                    }
                }

                // See what the players head hit
                foreach (OtherContact contact in character.CharacterController.SupportFinder.headContacts)
                {
                    // If the player hit some spikes
                    Collidable coll1 = contact.Collidable;
                    foreach (Entity entity in losePlatforms)
                    {
                        Collidable coll2 = entity.CollisionInformation;
                        if (coll1 == coll2)
                        {
                            Reset();
                        }
                    }

                    // If the player got hit by a projectile
                    foreach (EntityModel projectile in projectiles)
                    {
                        Collidable coll2 = projectile.entity.CollisionInformation;
                        if (coll1 == coll2)
                        {
                            Reset();
                        }
                    }
                }

                // See what the players side hit
                foreach (OtherContact contact in character.CharacterController.SupportFinder.sideContacts)
                {
                    // If the player hit some spikes
                    Collidable coll1 = contact.Collidable;
                    foreach (Entity entity in losePlatforms)
                    {
                        Collidable coll2 = entity.CollisionInformation;
                        if (coll1 == coll2)
                        {
                            Reset();
                        }
                    }

                    // If the player got hit by a projectile
                    foreach (EntityModel projectile in projectiles)
                    {
                        Collidable coll2 = projectile.entity.CollisionInformation;
                        if (coll1 == coll2)
                        {
                            Reset();
                        }
                    }
                }

                // Check if the player won the level
                foreach (SupportContact contact in character.CharacterController.SupportFinder.supports)
                {
                    Collidable coll1 = contact.Support;
                    foreach (Entity entity in winPlatforms)
                    {
                        Collidable coll2 = entity.CollisionInformation;
                        if (coll1 == coll2)
                        {
                            Win();
                        }
                    }
                }

                // If the player wants to change gravity
                if (oldKeyboardState.IsKeyDown(Keys.E) && keyboardState.IsKeyUp(Keys.E))
                {
                    foreach (Entity switchEntity in switchPlatforms)
                    {
                        Box switchBox = switchEntity as Box;
                        if ((character.CharacterController.Body.Position.Y > (switchBox.Position.Y - switchBox.HalfHeight)) &&
                           (character.CharacterController.Body.Position.Y < (switchBox.Position.Y + switchBox.HalfHeight)) &&
                           (character.CharacterController.Body.Position.Z > (switchBox.Position.Z - switchBox.HalfLength)) &&
                           (character.CharacterController.Body.Position.Z < (switchBox.Position.Z + switchBox.HalfLength)))
                        {
                            if (((PlatformInfo)switchBox.Tag).direction == "Up")
                            {
                                space.ForceUpdater.Gravity = new Vector3(0, 9.8f, 0);
                                character.CharacterController.Body.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(180f))) * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(270f))) * Quaternion.Identity;
                                camera.animateCameraRotation(180);
                            }
                            else if (((PlatformInfo)switchBox.Tag).direction == "Left")
                            {
                                space.ForceUpdater.Gravity = new Vector3(0, 0, -9.8f);
                                character.CharacterController.Body.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(90f))) * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(270f))) * Quaternion.Identity;
                                camera.animateCameraRotation(90);
                            }
                            else if (((PlatformInfo)switchBox.Tag).direction == "Right")
                            {
                                space.ForceUpdater.Gravity = new Vector3(0, 0, 9.8f);
                                character.CharacterController.Body.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(270f))) * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(270f))) * Quaternion.Identity;
                                camera.animateCameraRotation(270);
                            }
                            else if (((PlatformInfo)switchBox.Tag).direction == "Down")
                            {
                                space.ForceUpdater.Gravity = new Vector3(0, -9.8f, 0);
                                character.CharacterController.Body.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(0f))) * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(270f))) * Quaternion.Identity;
                                camera.animateCameraRotation(0);
                            }
                        }
                    }
                }

                win = WinUpdate();

                /* if (oldKeyboardState.IsKeyDown(Keys.N) && keyboardState.IsKeyUp(Keys.N))
                {
                    win = true;
                }

                if (oldKeyboardState.IsKeyDown(Keys.R) && keyboardState.IsKeyUp(Keys.R))
                {
                    Reset();
                } */
            }

            return win;
        }

        public void Draw()
        {
            // Draw our models
            foreach (EntityModel entityModel in Models)
            {
                entityModel.Draw();
            }

            // Draw our particles
            explosionParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
            explosionSmokeParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
        }

        public void LoadLevel(int level)
        {
            // Reset the camera
            camera.Reset();

            // Set the default gravity
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            // Create our character
            character = new CharacterControllerInput(space, camera);
            character.CharacterController.Body.Orientation *= Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(270f)));
            character.Activate();
            characterTurnedLeft = false;
            characterTurnedRight = false;

            // Adds a body to our character
            Cylinder cylinder = character.CharacterController.Body as Cylinder;
            characterModel = new EntityModel(space, character.CharacterController.Body, RobotModel, RobotTexture, basicLighting, Matrix.CreateScale(cylinder.Height / 5) * Matrix.CreateRotationY(MathHelper.ToRadians(180f)));
            Models.Add(characterModel);

            // Sets up our GravitationalFieldManager
            gravitationalFieldManager = new GravitationalFieldManager(this);

            System.IO.Stream stream = TitleContainer.OpenStream("Content\\Levels\\" + level + ".xml");

            XDocument doc = XDocument.Load(stream);

            IEnumerable<XElement> de =
                from el in doc.Descendants("PointLight")
                select el;

            foreach (XElement el in de)
            {
                string[] coords = el.Element("Position").Value.ToString().Split(new Char[] { ' ' });

                Vector3 position = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));

                basicLighting.pointLightPositions.Add(position);
            }

            de =
                from el in doc.Descendants("Cannon")
                select el;

            foreach (XElement el in de)
            {
                string[] coords = el.Element("Start").Value.ToString().Split(new Char[] { ' ' });
                Vector3 position = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));

                string[] dircoords = el.Element("Direction").Value.ToString().Split(new Char[] { ' ' });
                Vector3 direction = new Vector3(float.Parse(dircoords[0]), float.Parse(dircoords[1]), float.Parse(dircoords[2]));

                string[] moveCoords = el.Element("Movement").Value.ToString().Split(new Char[] { ' ' });
                Vector3 move = new Vector3(float.Parse(moveCoords[0]), float.Parse(moveCoords[1]), float.Parse(moveCoords[2]));

                Box box = new Box(position, 1, 1, 1);
                if (move.X != 0 || move.Y != 0 || move.Z != 0)
                {
                    PlatformInfo info = new PlatformInfo();
                    info.movement = move;
                    info.seconds = float.Parse(el.Element("Seconds").Value.ToString());
                    box.Tag = info;
                }

                float rate = float.Parse(el.Element("Rate").Value.ToString());

                EntityModel cannonEntity = new EntityModel(space, box, CannonModel, CannonTexture, basicLighting, Matrix.CreateRotationX(MathHelper.ToRadians(180f)));
                Cannon cannon = new Cannon(cannonEntity, direction, this);
                cannon.rate = rate;
                Models.Add(cannonEntity);
                cannons.Add(cannon);
            }

            de =
                from el in doc.Descendants("Box")
                select el;

            foreach (XElement el in de)
            {
                string[] coords = el.Element("Start").Value.ToString().Split(new Char[] { ' ' });
                Vector3 position = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));

                float size = float.Parse(el.Element("Size").Value.ToString());
                float mass = float.Parse(el.Element("Mass").Value.ToString());

                Texture2D texture = null;
                if (el.Element("Texture").Value.ToString() == "CubeTexture")
                {
                    texture = CubeTexture;
                }
                else if (el.Element("Texture").Value.ToString() == "BoxTexture")
                {
                    texture = BoxTexture;
                }

                Box box = new Box(position, size, size, size, mass);
                EntityModel thebox = new EntityModel(space, box, CubeModel, texture, basicLighting, Matrix.CreateScale(box.Width * 0.5f, box.Height * 0.5f, box.Length * 0.5f));
                Models.Add(thebox);
                looseObjects.Add(thebox);
            }

            foreach (Entity entity in getSurfaceEntities(doc, "Surface"))
            {
                Texture2D texture = null;
                if (((PlatformInfo)entity.Tag).texture == "CubeTexture")
                {
                    texture = CubeTexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "KeyATexture")
                {
                    texture = KeyATexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "KeyDTexture")
                {
                    texture = KeyDTexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "KeyETexture")
                {
                    texture = KeyETexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "KeyFTexture")
                {
                    texture = KeyFTexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "KeySpaceTexture")
                {
                    texture = KeySpaceTexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "KeyWTexture")
                {
                    texture = KeyWTexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "LeftClickTexture")
                {
                    texture = LeftClickTexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "RightClickTexture")
                {
                    texture = RightClickTexture;
                }
                Box box = entity as Box;
                EntityModel surface = new EntityModel(space, entity, CubeModel, texture, basicLighting, Matrix.CreateScale(box.Width * 0.5f, box.Height * 0.5f, box.Length * 0.5f));
                if (surface.entity.Position.X == 2.5f)
                {
                    backWalls.Add(surface);
                }
                Models.Add(surface);
            }

            foreach (Entity entity in getSurfaceEntities(doc, "Elevator"))
            {
                Texture2D texture = null;
                if (((PlatformInfo)entity.Tag).texture == "CubeTexture")
                {
                    texture = CubeTexture;
                }
                Box box = entity as Box;
                EntityModel surface = new EntityModel(space, entity, CubeModel, texture, basicLighting, Matrix.CreateScale(box.Width * 0.5f, box.Height * 0.5f, box.Length * 0.5f));
                surface.isMoving = false;
                if (((PlatformInfo)entity.Tag).platformID != currPlatformID)
                {
                    elevators.Add(new List<EntityModel>());
                    currElevatorID++;
                    elevators[currElevatorID].Add(surface);
                    currPlatformID = ((PlatformInfo)entity.Tag).platformID;
                }
                else
                {
                    elevators[currElevatorID].Add(surface);
                }
                Models.Add(surface);             
            }

            foreach(Entity entity in getSurfaceEntities(doc, "SpikedSurface")) 
            {
                Box box = entity as Box;
                EntityModel surface = new EntityModel(space, entity, CubeModel, CubeTexture, redLighting, Matrix.CreateScale(box.Width * 0.5f, box.Height * 0.5f, box.Length * 0.5f));
                EntityModel spikes1 = null;
                if (box.Height > 0.25f)
                {
                    spikes1 = new EntityModel(space, entity, SpikesModel, SpikesTexture, redLighting, Matrix.CreateScale(box.Height * 0.15f, box.Length * 2, box.Width * 0.15f));
                    spikes1.Transform *= Matrix.CreateRotationX(MathHelper.ToRadians(90f));
                }
                else
                {
                    spikes1 = new EntityModel(space, entity, SpikesModel, SpikesTexture, redLighting, Matrix.CreateScale(box.Width * 0.15f, box.Height * 2, box.Length * 0.15f));
                }
                spikes1.trackMovement = false;
                EntityModel spikes2 = null;
                if (box.Height > 0.25f)
                {
                    spikes2 = new EntityModel(space, entity, SpikesModel, SpikesTexture, redLighting, Matrix.CreateRotationX(MathHelper.ToRadians(180f)) * Matrix.CreateScale(box.Height * 0.15f, box.Length * 2, box.Width * 0.15f));
                    spikes2.Transform *= Matrix.CreateRotationX(MathHelper.ToRadians(90f));
                }
                else
                {
                    spikes2 = new EntityModel(space, entity, SpikesModel, SpikesTexture, redLighting, Matrix.CreateRotationX(MathHelper.ToRadians(180f)) * Matrix.CreateScale(box.Width * 0.15f, box.Height * 2, box.Length * 0.15f));
                }
                spikes2.trackMovement = false;
                losePlatforms.Add(surface.entity);
                Models.Add(surface);
                Models.Add(spikes1);
                Models.Add(spikes2);
            }

            foreach(Entity entity in getSurfaceEntities(doc, "WinSurface")) 
            {
                Box box = entity as Box;
                EntityModel surface = new EntityModel(space, entity, CubeModel, CubeTexture, highLighting, Matrix.CreateScale(box.Width * 0.5f, box.Height * 0.5f, box.Length * 0.5f));
                winPlatforms.Add(surface.entity);
                Models.Add(surface);
            }

            foreach (Entity entity in getSurfaceEntities(doc, "SwitchSurface"))
            {
                Texture2D texture = null;
                if (((PlatformInfo)entity.Tag).texture == "UpTexture")
                {
                    texture = UpTexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "DownTexture")
                {
                    texture = DownTexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "LeftTexture")
                {
                    texture = LeftTexture;
                }
                else if (((PlatformInfo)entity.Tag).texture == "RightTexture")
                {
                    texture = RightTexture;
                }
                Box box = entity as Box;
                EntityModel surface = new EntityModel(space, entity, CubeModel, texture, greenLighting, Matrix.CreateScale(box.Width * 0.5f, box.Height * 0.5f, box.Length * 0.5f));
                switchPlatforms.Add(surface.entity);
                if (surface.entity.Position.X == 2.5f)
                {
                    backWalls.Add(surface);
                }
                Models.Add(surface);
            }
        }

        public List<Entity> getSurfaceEntities(XDocument doc, String name)
        {
            List<Entity> returnList = new List<Entity>();

            IEnumerable<XElement> de =
                from el in doc.Descendants(name)
                select el;

            // Increment our platform ID
            int PlatformID = currPlatformID + 1;

            foreach (XElement el in de)
            {
                string[] startCoords = el.Element("Start").Value.ToString().Split(new Char[] { ' ' });
                Vector3 start = new Vector3(float.Parse(startCoords[0]), float.Parse(startCoords[1]), float.Parse(startCoords[2]));

                float tileSize = float.Parse(el.Element("TileSize").Value);
                int xtiles = int.Parse(el.Element("XTiles").Value);
                int ytiles = int.Parse(el.Element("YTiles").Value);
                int ztiles = int.Parse(el.Element("ZTiles").Value);

                String direction = "None";
                if(name == "SwitchSurface") {
                    direction = el.Element("Direction").Value.ToString();
                }

                string[] moveCoords = el.Element("Movement").Value.ToString().Split(new Char[] { ' ' });
                Vector3 move = new Vector3(float.Parse(moveCoords[0]), float.Parse(moveCoords[1]), float.Parse(moveCoords[2]));

                float seconds = 0;
                if (move.X != 0 || move.Y != 0 || move.Z != 0)
                {
                    seconds = float.Parse(el.Element("Seconds").Value.ToString());
                }

                float defaultHeight = 0.25f;
                if (name == "Elevator")
                {
                    defaultHeight = 1f;
                }

                string texture = el.Element("Texture").Value.ToString();

                if (ytiles == 0)
                {
                    for (int currx = 0; currx < xtiles; currx++)
                    {
                        for (int currz = 0; currz < ztiles; currz++)
                        {
                            Entity surface = new Box(new Vector3(start.X + currx * tileSize, start.Y, start.Z + currz * tileSize), tileSize, defaultHeight, tileSize);
                            PlatformInfo info = new PlatformInfo();
                            info.direction = direction;
                            info.movement = move;
                            info.seconds = seconds;
                            info.texture = texture;
                            info.platformID = PlatformID;
                            surface.Tag = info;
                            returnList.Add(surface);
                        }
                    }
                }
                else if (ztiles == 0)
                {
                    for (int currx = 0; currx < xtiles; currx++)
                    {
                        for (int curry = 0; curry < ytiles; curry++)
                        {
                            Entity surface = new Box(new Vector3(start.X + currx * tileSize, start.Y + curry * tileSize, start.Z), tileSize, tileSize, defaultHeight);
                            PlatformInfo info = new PlatformInfo();
                            info.direction = direction;
                            info.movement = move;
                            info.seconds = seconds;
                            info.texture = texture;
                            info.platformID = PlatformID;
                            surface.Tag = info;
                            returnList.Add(surface);
                        }
                    }
                }
                else if (xtiles == 0)
                {
                    for (int currz = 0; currz < ztiles; currz++)
                    {
                        for (int curry = 0; curry < ytiles; curry++)
                        {
                            Entity surface = new Box(new Vector3(start.X, start.Y + curry * tileSize, start.Z + currz * tileSize), defaultHeight, tileSize, tileSize);
                            PlatformInfo info = new PlatformInfo();
                            info.direction = direction;
                            info.movement = move;
                            info.seconds = seconds;
                            info.texture = texture;
                            info.platformID = PlatformID;
                            surface.Tag = info;
                            returnList.Add(surface);
                        }
                    }
                }
            }

            return returnList;
        }

        public void unloadLevel()
        {
            gravitationalFieldManager.Clear();
            winPlatforms.Clear();
            losePlatforms.Clear();
            switchPlatforms.Clear();
            cannons.Clear();
            elevators.Clear();
            looseObjects.Clear();
            backWalls.Clear();

            foreach (EntityModel entityModel in Models)
            {
                if (space.Entities.Contains(entityModel.entity))
                {
                    space.Remove(entityModel.entity);
                }
            }

            Models.Clear();
            basicLighting.pointLightPositions.Clear();

            currPlatformID = 0;
            currElevatorID = -1;
        }

        public void Win()
        {
            if (!character.hasWon)
            {
                character.hasWon = true;
                character.CharacterController.Body.LinearVelocity = new Vector3(0, 0, 2);
                character.CharacterController.Jump();
                character.CharacterController.Body.AngularVelocity = new Vector3(0, -5, 0);
                steps = 0;
            }
        }

        public bool WinUpdate()
        {
            if (character.hasWon)
            {
                if (steps < 120)
                {
                    steps++;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            if (character.alive && !character.hasWon)
            {
                explosions.Add(new Explosion(explosionParticles, explosionSmokeParticles, character.CharacterController.Body.Position));
                character.alive = false;
                character.CharacterController.Body.LinearVelocity = new Vector3(0, 0, 0);
                character.Deactivate(false);
                steps = 0;
            }
        }

        public void ResetUpdate() {
            if (!character.alive)
            {
                if (steps < 60)
                {
                    steps++;
                }
                else
                {
                    // Elevators need to be reset
                    foreach (List<EntityModel> elevator in elevators)
                    {
                        foreach (EntityModel elevatorPart in elevator)
                        {
                            elevatorPart.entity.Position = elevatorPart.originalPosition;
                            elevatorPart.isMoving = false;
                            elevatorPart.movingPositive = true;
                            elevatorPart.currentSteps = 0;
                        }
                    }

                    // Loose objects need to reset
                    foreach (EntityModel item in looseObjects)
                    {
                        item.entity.Position = item.originalPosition;
                        item.entity.WorldTransform = item.originalTransform;
                        item.entity.LinearVelocity = new Vector3(0, 0, 0);
                        item.entity.AngularVelocity = new Vector3(0, 0, 0);

                        if (!space.Entities.Contains(item.entity))
                        {
                            space.Add(item.entity);
                            Models.Add(item);
                        }
                    }

                    // Turn off the fields if they are on
                    if (gravitationalFieldManager.fieldsActive)
                    {
                        gravitationalFieldManager.ToggleActivation();
                    }

                    // Reset the character
                    character.Reset();
                }
            }
        }

        void playExplosionSound(Vector3 position)
        {
            explosionSound = expSound.CreateInstance();
            explosionSound.Volume = 0.05f;
            AudioListener listener = new AudioListener();
            listener.Position = character.CharacterController.Body.Position;
            listener.Forward = character.CharacterController.Body.OrientationMatrix.Backward;
            listener.Up = character.CharacterController.Body.OrientationMatrix.Up;
            AudioEmitter emitter = new AudioEmitter();
            emitter.Position = position;
            emitter.Up = Matrix.Identity.Up;
            emitter.Forward = Matrix.Identity.Forward;
            explosionSound.Apply3D(listener, emitter);
            explosionSound.Play();
        }
    }
}
