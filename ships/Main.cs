using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Ships;

using static Constants;

partial class MyGame : Game
{
    //Game
    public static ColorRectangle selected = new(Rectangle.Empty);
    static Rectangle selectedRectDebug = Rectangle.Empty;
    static int player = 0;

    //Game state
    public enum GameState { Setup, Fight, End }
    private static GameState gameState = GameState.Setup;

    static Point currentGridStart = Point.Zero;

    static Ship[]? currentShips = null;
    public static Ship[]? CurrentShips => currentShips;

    static bool[,] currentBoard = new bool[10,10];
    bool[,] board1 = new bool[10, 10];
    bool[,] board2 = new bool[10, 10];

    static List<ColorRectangle> shots = new(100);

    //////////////////////////////////////////////////////////Update game states
    private void UpdateSetup()
    {
        Ship.UpdateShips();
        UpdateButton();
    }

    private void UpdateEndScreen()
    {
        StartButton();
    }

    private void UpdateFight()
    {
        for (int x = 0; x < boardSize; ++x)
        {
            for (int y = 0; y < boardSize; ++y)
            {
                if (currentBoard[x, y]) continue;
                Rectangle square = new(x * rectSize + currentGridStart.X, y * rectSize + currentGridStart.Y, rectSize, rectSize);
                
                if (square.Contains(mouse.Position) && !clicking)
                {
                    Mouse.SetCursor(MouseCursor.Hand);
                    selected.rect = square;

                    if (mouse.LeftButton == ButtonState.Pressed)
                    {
                        currentBoard[x, y] = true;

                        shots.Add(new ColorRectangle(selected.rect));
                        if (shots.Count > 99) shots.Capacity = 200;

                        bool haveShot = false;
                        foreach (Ship ship in currentShips)
                        {
                            if (ship.rect.Contains(selected.rect))
                            {
                                haveShot = true;
                                //Console.WriteLine();
                                shots[^1] = new ColorRectangle(shots.Last().rect, Color.IndianRed);

                                //If all ships defeated
                                if (ship.CheckAndMakeShot(CurrentShips, shots))
                                {
                                    EndGame();
                                    return;
                                }
                                if (ship.isDestroyed) DestroyedShipsArea(ship);

                            }
                        }
                        
                        if(!haveShot) MakeTurn();
                    }
                    break;
                }
            }
            if (selected.rect != Rectangle.Empty) break;
        }
    }

    private void DestroyedShipsArea(Ship ship)
    {
        //Board change
        Rectangle area = new();
        area.Location = new Point(ship.rect.X - rectSize, ship.rect.Y - rectSize);
        area.Size = new Point(ship.rect.Width + rectSize * 2, ship.rect.Height + rectSize*2);

        //Out of range corrections
        if(area.X + area.Width > currentGridStart.X + boardSize * 32)   area.Width -= rectSize;
        if (area.Y + area.Height > currentGridStart.Y + boardSize * 32) area.Height -= rectSize;

        if (area.X < currentGridStart.X) 
        { 
            area.X = currentGridStart.X;
            area.Width -= rectSize;
        }
        if (area.Y < currentGridStart.Y)  
        { 
            area.Y = currentGridStart.Y; 
            area.Height -= rectSize; 
        }

        //Place shots into the current board
        int a = (area.X - currentGridStart.X) / rectSize;
        int b = (area.Y - currentGridStart.Y) / rectSize;

        int a1 = a + area.Width / rectSize;
        int b2 = b + area.Height / rectSize;

        //Debug
        /*Console.WriteLine("Starting point: " + (a + " " + b));
        Console.WriteLine("Condition: " + (a1 + " " + b2));
        Console.WriteLine("Size: " + area.Size);*/

        for (int x = a; x < a1; ++x)
            for (int y = b; y < b2; ++y)
                currentBoard[x, y] = true;

        //Add visible shots
        shots.Add(new ColorRectangle(area, Color.Gray));
        shots.Add(new ColorRectangle(ship.rect, Color.DarkRed));
    }

    //////////////////////////////////////////////////////////Initialize game states
    private void StartGame()
    {
        //Console.WriteLine("Starting game");
        gameState = GameState.Fight;
        selected.color = Color.LightGreen;

        graphics.PreferredBackBufferWidth = screenGameSize.X;
        graphics.ApplyChanges();

        MakeTurn();
    }

    private void MakeTurn()
    {
        //if (gameState == GameState.End) return;

        if (player == 1)
            player = 2;
        else
            player = 1;

        if (player == 1)
        {
            Console.WriteLine("Player 1");
            
            gameGrid1Color = Color.Black;
            gameGrid2Color = Color.LightBlue;

            currentGridStart = gameGrid2Start;
            currentShips = Ship.P2;
            currentBoard = board2;
        }
        if (player == 2)
        {
            Console.WriteLine("Player 2");
            gameGrid1Color = Color.LightBlue;
            gameGrid2Color = Color.Black;

            currentGridStart = gameGrid1Start;
            currentShips = Ship.P1;
            currentBoard = board1;
        }
    }

    public void EndGame()
    {
        Console.WriteLine("Player " + player + " wins!");
        //Console.WriteLine("Hello");
        gameGrid1Color = Color.Black;
        gameGrid2Color = Color.Black;
        gameState = GameState.End;

        currentShips = player == 1 ? Ship.P1 : Ship.P2;

        foreach (Ship ship in currentShips)
        {
            ship.rect.Location -= player == 1 ? gameGrid1Start : gameGrid2Start;
            ship.rect.Location += new Point(placeGridStart, placeGridStart);
        }

        graphics.PreferredBackBufferWidth = screenSize;
        graphics.ApplyChanges();
    }

    //////////////////////////////////////////////////////////Draw game states
    public void DrawSetup()
    {
        DrawGrid(new(placeGridStart, placeGridStart), Color.LightBlue);

        spriteBatch.FillRectangle(button.rect, button.color);
        spriteBatch.FillRectangle(selected.rect, selected.color);

        string text = player == 0 ? "Player 2" : "Start Game";

        Vector2 textMiddle = buttonTextFont.MeasureString(text) / 2;
        Vector2 buttonMiddle = button.rect.Location.ToVector2() + button.rect.Size.ToVector2() / 2;
        textMiddle.Y += 3;
        spriteBatch.DrawString(buttonTextFont, text, buttonMiddle, textColor, 0, textMiddle, 1, SpriteEffects.None, 1);

        Ship.DrawShips(spriteBatch, null);
    }

    public void DrawGame()
    {
        DrawGrid(new(gameGrid1Start.X, gameGrid1Start.Y), gameGrid1Color);
        DrawGrid(new(gameGrid2Start.X, gameGrid2Start.Y), gameGrid2Color);

        spriteBatch.FillRectangle(selected.rect, selected.color);

        foreach (ColorRectangle shot in shots)
            spriteBatch.FillRectangle(shot.rect, shot.color);

        // TODO: Debug stuff
        if(showShipsPlacement)
        {
            foreach (Ship ship in Ship.P1)
                spriteBatch.DrawRectangle(ship.rect, new Color(Color.Red, 255));

            foreach (Ship ship in Ship.P2)
                spriteBatch.DrawRectangle(ship.rect, new Color(Color.Red, 255));
        }
    }

    public void DrawEndScreen()
    {
        DrawGrid(new(placeGridStart, placeGridStart), Color.LightBlue);
        spriteBatch.FillRectangle(button.rect, button.color);

        string text = "New Game";

        //Drawing button text
        Vector2 textMiddle = buttonTextFont.MeasureString(text) / 2;
        Vector2 buttonMiddle = button.rect.Location.ToVector2() + button.rect.Size.ToVector2() / 2;
        textMiddle.Y += 3;
        spriteBatch.DrawString(buttonTextFont, text, buttonMiddle, Color.White, 0, textMiddle, 1, SpriteEffects.None, 1);

        Ship.DrawShips(spriteBatch, CurrentShips);
    }

    public void DrawGrid(Point gridStart, Color color)
    {
        //Frame
        const int thick = 4;
        Rectangle frame = new(gridStart.X - thick, gridStart.Y - thick, gridSize + thick * 2, gridSize + thick * 2);
        spriteBatch.DrawRectangle(frame, color, thick);

        //Lines
        for (int x = 1; x < boardSize; ++x)
        {
            float lineX = x * rectSize + gridStart.X;
            Vector2 linePoint1 = new Vector2(lineX, gridStart.Y);
            Vector2 linePoint2 = new Vector2(lineX, gridStart.Y + gridSize);

            spriteBatch.DrawLine(linePoint1, linePoint2, color, 2);
        }
        for (int y = 1; y < boardSize; ++y)
        {
            float lineY = y * rectSize + gridStart.Y;
            Vector2 linePoint1 = new Vector2(gridStart.X, lineY);
            Vector2 linePoint2 = new Vector2(gridStart.X + gridSize, lineY);

            spriteBatch.DrawLine(linePoint1, linePoint2, color, 2);
        }
    }

    //////////////////////////////////////////////////////////Reset
    private void ResetGame()
    {
        gameState = GameState.Setup;

        Ship.ResetShips();
        currentShips = null;
        currentGridStart = Point.Zero;

        for (int x=0; x < currentBoard.GetLength(0); ++x)
            for (int y = 0; y < currentBoard.GetLength(1); ++y)
                currentBoard[x, y] = false;

        shots.Clear();
        player = 0;
    }

}

//////////////////////////////// Start button
partial class MyGame : Game
{
    static readonly Rectangle buttonSetupPos = new Rectangle(512 - 120, 512 - 70, 100, 50);
    static readonly Rectangle buttonEndPos = new Rectangle(512 - 120, 512 - 70, 100, 50);
    Color textColor = Color.Gray;
    SpriteFont buttonTextFont;

    ColorRectangle button = new
    (
        buttonSetupPos,
        Color.DarkGreen
    );

    private void StartButton()
    {
        if (button.rect.Contains(mouse.Position) && !clicking)
        {
            Mouse.SetCursor(MouseCursor.Hand);
            button.color = Color.LightGreen;

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if(gameState == GameState.Setup)
                {
                    player++;
                    Ship.StoreShips(player);
                    if (player == 2) StartGame();
                }
                if (gameState == GameState.End)
                    ResetGame();
            }
        }
        else
        {
            button.color = Color.Green;
            textColor = Color.White;
        }
    }

    private void UpdateButton()
    {
        if (Ship.ShipsPlaced)
            StartButton();
        else
        {
            button.color = Color.DarkGreen;
            textColor = Color.Gray;
        }
    }
}