using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Pairs;

using static Utils;

//////////////////////////////// Starting point
partial class MyGame : Game
{
    //Game data
    /*Old
    const int pairCount = (cardsX * cardsY) / 2;
    const int cardsX = 4;
    const int cardsY = 4;
    int[] cardsInPair;= new int[pairCount];
    int[,] cardPairIndex; = new int[cardsX,cardsY];*/

    const int cardSize = 48;
    const double failResetTime = 0.8f;
    const double successResetTime = 0.2f;

    //Difficulties
    const int diff_easy = 4;
    const int diff_med = 6;
    const int diff_hard = 8;

    //Custom difficulty
    Point diff_custom = new(5, 4);
    const int maxCustom = 8;
    const int minCustom = 1;
    bool isCustomEven = true;

    //Cards and pairs
    int collectedPairs = 0;
    int pairCount = 0;
    int[] cardsInPair;
    int[,] cardPairIndex;

    bool inGame = false;
    bool win = false;
    int steps;

    Rectangle select = Rectangle.Empty;

    readonly static Point EMPTY = new Point(-1, -1);
    Point chosed1 = EMPTY;
    Point chosed2 = EMPTY;
    Texture2D? chosed1Shape = null;
    Texture2D? chosed2Shape = null;
    Point gridOffset;
    

    //Cards and pairs
    void GeneratePairs(int cardsX, int cardsY)
    {
        if (cardsX * cardsY % 2 != 0)
        {
            throw new Exception("card count " + cardsX * cardsY + " is not even, pairs wouldn't fit (GeneratePairs)");
        }

        gridOffset = new(screenSize.X / 2 - (cardsX*cardSize) / 2, screenSize.Y / 2 - (cardsY * cardSize) / 2);

        //Setup
        pairCount = (cardsX * cardsY) / 2;
        cardsInPair = new int[pairCount];
        cardPairIndex = new int[cardsX, cardsY];

        /*for (int i = 0; i < pairCount; ++i)
            cardsInPair[i] = 0;*/

        //Generate
        for(int x = 0; x < cardsX; ++x)
        {
            for (int y = 0; y < cardsY; ++y)
            {
                cardPairIndex[x, y] = -10;

                int pair = new Random((int)DateTime.Now.Ticks).Next(pairCount);

                //If this pair is ready (two cards), choose another pair
                if (cardsInPair[pair] == 2)
                {
                    //Add or subtract
                    int rand = new Random((int)DateTime.Now.Ticks).Next(1);

                    //Add
                    if (rand == 0)
                        while (cardsInPair[pair] == 2)
                            pair = pair < pairCount - 1 ? pair + 1 : 0;

                    //Subtract
                    if (rand == 1)
                        while (cardsInPair[pair] == 2)
                            pair = pair > 0 ? pair - 1 : pairCount - 1;
                }

                cardsInPair[pair]++;
                cardPairIndex[x, y] = pair;
            }
        }
    }

    void CheckPair()
    {
        double resetTime;

        if (cardPairIndex[chosed1.X, chosed1.Y] == cardPairIndex[chosed2.X, chosed2.Y])
        {
            cardPairIndex[chosed1.X, chosed1.Y] = -1;
            cardPairIndex[chosed2.X, chosed2.Y] = -1;
            if (++collectedPairs >= pairCount) Event.Add(WinGame, successResetTime);
            resetTime = successResetTime;
        }
        else 
            resetTime = failResetTime;

        Event.Add(ResetChosenCards, resetTime);
    }

    void SelectCards()
    {
        for (int x = 0; x < cardPairIndex.GetLength(0); ++x)
        {
            for (int y = 0; y < cardPairIndex.GetLength(1); ++y)
            {
                //If card is collected skip
                if (cardPairIndex[x, y] == -1) continue;

                //If card is chosed skip
                if (chosed1 == new Point(x, y) || chosed2 == new Point(x,y)) 
                    continue;

                Rectangle card = new Rectangle(x * cardSize, y * cardSize, cardSize, cardSize);
                card.Location += gridOffset;

                if (card.Contains(mouse.Position) && !clicking)
                {
                    Mouse.SetCursor(MouseCursor.Hand);
                    select = card;

                    if (mouse.LeftButton == ButtonState.Pressed)
                    {
                        steps++;
                        int icon = cardPairIndex[x, y] + 1;
                        Texture2D shape = Content.Load<Texture2D>("shape" + icon);

                        if (chosed1 == EMPTY)
                        {
                            chosed1Shape = shape;
                            chosed1 = new Point(x, y);
                        }
                        else
                        {
                            chosed2Shape = shape;
                            chosed2 = new Point(x, y);
                            CheckPair();
                        }
                    }
                }
            }
        }
    }

    void ResetChosenCards()
    {
        chosed1 = chosed2 = EMPTY;
        chosed1Shape = chosed2Shape = null;
    }

    void DrawCards()
    {
        for (int x = 0; x < cardPairIndex.GetLength(0); ++x)
        {
            for (int y = 0; y < cardPairIndex.GetLength(1); ++y)
            {
                if (cardPairIndex[x, y] == -1) continue;

                Rectangle card = new Rectangle(x * cardSize, y * cardSize, cardSize, cardSize);
                card.Location += gridOffset;
                spriteBatch.FillRectangle(card, Color.LightBlue);
            }
        }

        if (chosed1 != EMPTY)
        {
            Rectangle dest = new(chosed1.X * cardSize, chosed1.Y * cardSize, cardSize, cardSize);
            dest.Location += gridOffset;
            spriteBatch.Draw(chosed1Shape, dest, Color.Purple);
        }
        if (chosed2 != EMPTY)
        {
            Rectangle dest = new(chosed2.X * cardSize, chosed2.Y * cardSize, cardSize, cardSize);
            dest.Location += gridOffset;
            spriteBatch.Draw(chosed2Shape, dest, Color.Purple);
        }

        spriteBatch.FillRectangle(select, Color.LightGreen);
    }

    //Set game state
    void EndGame()
    {
        Event.ClearEvents();
        ResetChosenCards();

        Button.currentLayer = 0;
        collectedPairs = 0;
        steps = 0;
        win = false;
        inGame = false;
    }

    void StartGame(int x, int y)
    {
        Event.ClearEvents();
        GeneratePairs(x, y);
        Button.currentLayer = 1;
        inGame = true;
    }

    void StartCustomGame() => StartGame(diff_custom.X, diff_custom.Y);

    void WinGame() => win = true;

    //Boring stuff
    public static readonly Point screenSize = new(1980, 1080);
    const string gameName = "Pairs";
    public static SpriteFont textFont { get; set; }

    GraphicsDeviceManager graphics;
    static SpriteBatch spriteBatch;
    public static MouseState mouse { get => Mouse.GetState(); }
    public static KeyboardState keys { get => Keyboard.GetState(); }

    static public double globalTime { get; private set; }
    static public bool clicking { get; private set; }

    protected override void Update(GameTime gameTime)
    {
        //Exit
        if (keys.IsKeyDown(Keys.Escape)) Exit();

        Mouse.SetCursor(MouseCursor.Arrow);
        select = Rectangle.Empty;

        if(inGame)
        {
            if (chosed1 == EMPTY || chosed2 == EMPTY)
                SelectCards();
        }
        
        Button.UpdateButtons();
        Event.ExecuteEvents();

        globalTime += gameTime.ElapsedGameTime.TotalSeconds;

        //Mouse
        clicking = (mouse.LeftButton == ButtonState.Pressed);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(Color.White);

        spriteBatch.Begin();
        {
            if (inGame)
            {
                DrawCards();

                //Attempts text
                Vector2 measure = textFont.MeasureString("Attempts: 0");
                Vector2 textCenter = new(screenSize.X / 2 - measure.X / 2, screenSize.Y / 2 - measure.Y / 2);
                Vector2 pos = win ? textCenter : new Vector2(10,10);

                spriteBatch.DrawString(textFont, "Attempts: " + Math.Floor((decimal)steps / 2), pos, Color.Black);

                //Joke about 1x2
                if(win && steps == 2)
                {
                    spriteBatch.DrawString(textFont, "WOW!! dude you are crazy!!! one attempt?!?", pos + new Vector2(-160,64), Color.Black);
                    spriteBatch.DrawString(textFont, "Can you teach me how to play like this? :OO", pos + new Vector2(-160, 64+32), Color.Black);
                }
            }
            else
            {
                //Custom difficutly
                spriteBatch.DrawString(textFont, "" + diff_custom.X, new Vector2(284+2, 377+18), Color.Black);
                spriteBatch.DrawString(textFont, "" + diff_custom.Y, new Vector2(284+64+2, 377 + 18), Color.Black);
                spriteBatch.DrawString(textFont, "x", new Vector2(284 + 32 + 2, 377 + 18), Color.Black);

                if (!isCustomEven)
                {
                    Vector2 measure = textFont.MeasureString("0x0 is not even, pairs won't fit");
                    Vector2 pos = new(screenSize.X / 2 - measure.X / 2, 450);
                    spriteBatch.DrawString(textFont, diff_custom.X + "x" + diff_custom.Y + " is not even, pairs won't fit" , pos, Color.DarkRed);
                }
            }

            //Buttons
            Button.DrawButtons(spriteBatch);
        }
        spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        textFont = Content.Load<SpriteFont>("bahnschrift");
        if (textFont == null) Console.WriteLine("null font");
    }

    //Setups
    protected override void Initialize()
    {
        Window.AllowUserResizing = false;
        Window.Title = gameName;
        graphics.IsFullScreen = true; 
        IsMouseVisible = true;
        graphics.PreferredBackBufferWidth = 1680;
        graphics.PreferredBackBufferHeight = 1050;
        graphics.ApplyChanges();

        //Buttons
        Button.currentLayer = 0;
        int w = 380;
        int h = 90;
        int offsetX = screenSize.X / 2 - w / 2;
        int offsetY = 100;
        var startGameFunc = (object[] args) => StartGame((int)args[0], (int)args[1]);

        //Difficulty buttons
        Button.Add(new Rectangle(offsetX, offsetY, w, h), startGameFunc, "Easy (4x4)", 0, null, diff_easy, diff_easy);
        Button.Add(new Rectangle(offsetX, offsetY + h + 16, w, h), startGameFunc, "Medium (6x6)", 0, null, diff_med, diff_med);
        Button.Add(new Rectangle(offsetX, offsetY + h * 2 + 16 * 2, w, h), startGameFunc, "Hard (8x8)", 0, null, diff_hard, diff_hard);

        Button custom = Button.Add(new Rectangle(103, 377, w, h), (object[] args) => StartCustomGame(), "Custom", 0);

        var lockButton = () =>
        {
            custom.Locked = false;
            if (diff_custom.X * diff_custom.Y % 2 != 0) custom.Locked = true;
            isCustomEven = !custom.Locked;
        };

        Button.Add(new Rectangle(284, 377, 16, 16),             (object[] args) => { diff_custom.X = clamp(++diff_custom.X, minCustom, maxCustom); lockButton(); }, "UP_X", 0,   Content.Load<Texture2D>("diff_up"));
        Button.Add(new Rectangle(284 + 64, 377, 16, 16),        (object[] args) => { diff_custom.Y = clamp(++diff_custom.Y, minCustom, maxCustom); lockButton(); }, "UP_Y", 0,   Content.Load<Texture2D>("diff_up"));
        Button.Add(new Rectangle(284, 377 + 48, 16, 16),        (object[] args) => { diff_custom.X = clamp(--diff_custom.X, minCustom, maxCustom); lockButton(); }, "DOWN_X", 0,  Content.Load<Texture2D>("diff_down"));
        Button.Add(new Rectangle(284 + 64, 377 + 48, 16, 16),   (object[] args) => { diff_custom.Y = clamp(--diff_custom.Y, minCustom, maxCustom); lockButton(); }, "DOWN_Y", 0,   Content.Load<Texture2D>("diff_down"));

        //Back button
        Button.Add(new Rectangle(screenSize.X - w, screenSize.Y - h + 4, w-16,h-16), (object[] args) => EndGame(), "Back", 1);

        base.Initialize();
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