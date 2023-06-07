using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Ships;

using static Constants;

struct ColorRectangle
{
    public Rectangle rect = Rectangle.Empty;
    public Color color = Color.Gray;
    public ColorRectangle(Rectangle rect) => this.rect = rect;
    public ColorRectangle(Rectangle rect, Color color) : this(rect) => this.color = color;
    public ColorRectangle() { }
}

//////////////////////////////// Starting point
partial class MyGame : Game
{
    
    //Stuff
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    public static MouseState mouse { get => Mouse.GetState(); }

    static bool pressingR = false;
    static bool clicking = false;
    public static bool HoldingLeft => clicking;
    public static bool HoldingR => pressingR;

    bool showShipsPlacement = false;

    //Main
    protected override void Update(GameTime gameTime)
    {
        //Exit
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

        Mouse.SetCursor(MouseCursor.Arrow);
        selected.rect = Rectangle.Empty;

        //Game state
        switch (gameState)
        {
            case GameState.Setup: UpdateSetup(); break;
            case GameState.Fight: UpdateFight(); break;
            case GameState.End: UpdateEndScreen(); break;
            default: Console.WriteLine("Unknown game state"); break;
        }

        Debug();

        //Keyboard, mouse
        clicking = (mouse.LeftButton == ButtonState.Pressed);
        pressingR = Keyboard.GetState().IsKeyDown(Keys.R);

        base.Update(gameTime);
    }

    private void Debug()
    {
        if (Keyboard.GetState().IsKeyDown(Keys.D1)) gameState = GameState.Fight;
        if (Keyboard.GetState().IsKeyDown(Keys.D2)) gameState = GameState.Setup;
        if (Keyboard.GetState().IsKeyDown(Keys.D3)) gameState = GameState.End;
        if (Keyboard.GetState().IsKeyDown(Keys.D4)) showShipsPlacement = !showShipsPlacement;
        if (Keyboard.GetState().IsKeyDown(Keys.O)) Ship.ForceReady();
        if (Keyboard.GetState().IsKeyDown(Keys.NumPad1)) { player = 1; MakeTurn(); }
        if (Keyboard.GetState().IsKeyDown(Keys.NumPad2)) { player = 2; MakeTurn(); }
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(Color.White);

        spriteBatch.Begin();
        {
            switch (gameState)
            {
                case GameState.Setup:   DrawSetup();        break;
                case GameState.Fight:   DrawGame();         break;
                case GameState.End:     DrawEndScreen();    break;
                default: Console.WriteLine("Unknown game state in Draw"); break;
            }
        }
        spriteBatch.End();

        base.Draw(gameTime);
    }

    //Setups
    protected override void Initialize()
    {
        Window.AllowUserResizing = false;
        Window.Title = "Ships";
        IsMouseVisible = true;
        graphics.PreferredBackBufferWidth = screenSize;
        graphics.PreferredBackBufferHeight = screenSize;
        graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        buttonTextFont = Content.Load<SpriteFont>("Fonts/Bahnschrift");
        if(buttonTextFont == null)
        {
            Console.WriteLine("null");
        }
    }
    public MyGame() : base()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }
}

class Program
{
    public static void Main()
    {
        using (MyGame game = new MyGame())
        {
            game.Run();
        }
    }
}

//Old
/*Rectangle[] ships =
{
    new Rectangle(0,0,quad, rectSize),
    new Rectangle(0,0,triple, rectSize),
    new Rectangle(0,0,triple, rectSize),
    new Rectangle(0,0,doub, rectSize),
    new Rectangle(0,0,doub, rectSize),
    new Rectangle(0,0,doub, rectSize),
    new Rectangle(0,0,single, rectSize),
    new Rectangle(0,0,single, rectSize),
    new Rectangle(0,0,single, rectSize),
    new Rectangle(0,0,single, rectSize),
};*/
/*    Point[] shipPlaces =
{
    new Point(0,384),
    new Point(0,384+32),
    new Point(0,384+64),
    new Point(128,384),
    new Point(128,384+32),
    new Point(128,384+64),
    new Point(256,384),
    new Point(256,384+32),
    new Point(256,384+64),
    new Point(256+32,384)
};*/
//bool[] grabbingShip = new bool[shipCount];
/*private void BoardSelection(int i)
{
    bool conditions = false;

    for (int x = 0; x < boardSize; ++x)
    {
        for (int y = 0; y < boardSize; ++y)
        {
            Rectangle rect = new(x * rectSize + placeGridStart, y * rectSize + placeGridStart, rectSize, rectSize);
            Point shipLeftSquare = new Point(ships[i].Location.X + 16, ships[i].Location.Y + 16);

            conditions =
            (
                rect.Contains(shipLeftSquare) &&
                ships[i].Right <= placeGridStart + boardSize * rectSize &&
                ships[i].Bottom <= placeGridStart + boardSize * rectSize
            );

            if (conditions)
            {
                selectedRect = rect;
                selectedRect.Size = ships[i].Size;
                break;
            }
        }

        if (conditions) break;
    }

    if (MyGame.mouse.LeftButton == ButtonState.Released)
    {
        ships[i].Location = conditions ? selectedRect.Location : shipPlaces[i];
    }
}

private void GrabShip()
{
    for (int i=0; i < ships.Length; ++i)
    {
        if (grabbingShip[i])
        {
            if(Keyboard.GetState().IsKeyDown(Keys.R) && !MyGame.HoldingR)
            {
                int shipSizeX = ships[i].Size.X;
                ships[i].Size = new Point(ships[i].Size.Y, shipSizeX);   
            }

            Mouse.SetCursor(MouseCursor.Hand);
            ships[i].Location = new Point(MyGame.mouse.X - ships[i].Width / 2, MyGame.mouse.Y - ships[i].Height / 2);
            BoardSelection(i);
        }

        if (grabbingShip.Contains(true))
            continue;

        if (ships[i].Contains(MyGame.mouse.Position) && !clicking)
        {
            Mouse.SetCursor(MouseCursor.Hand);

            if (MyGame.mouse.LeftButton == ButtonState.Pressed)
                grabbingShip[i] = true;
        }
    }

    if (MyGame.mouse.LeftButton == ButtonState.Released)
        grabbingShip = Enumerable.Repeat(false, grabbingShip.Length).ToArray();

    clicking = (MyGame.mouse.LeftButton == ButtonState.Pressed);
    MyGame.HoldingR = Keyboard.GetState().IsKeyDown(Keys.R);
}*/