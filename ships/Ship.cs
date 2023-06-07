using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Ships;

using static Constants;

//////////////////////////////// SHIP

class Ship : ICloneable
{
    //Static
    const int quad = 4;
    const int triple = 3;
    const int doub = 2;
    const int single = 1;

    private static Ship[] setupShips =
    {
        new Ship(quad),
        new Ship(triple),
        new Ship(triple),
        new Ship(doub),
        new Ship(doub),
        new Ship(doub),
        new Ship(single),
        new Ship(single),
        new Ship(single),
        new Ship(single)
    };

    public static Ship[] P1 { get; private set; }
    public static Ship[] P2 { get; private set; }

    private static bool grabbingAnyShip = false;
    public static bool ShipsPlaced { get; private set; }

    //Non-static
    private Point defaultPlace;
    private Point defaultSize;

    private int shipSize;
    public Rectangle rect = Rectangle.Empty;
    public Rectangle unplaceableArea = Rectangle.Empty;

    public enum State { none, grabed, placed }
    public State state = State.none;

    bool destroyed = false;
    public bool isDestroyed => destroyed;
    private int shots;

    //Methods
    public Ship(int w)
    {
        rect = new Rectangle(0, 0, w * rectSize, rectSize);
        defaultPlace = rect.Location;
        defaultSize = rect.Size;
        shipSize = w;
    }

    //Add shot and check if ship's destroyed.
    //If every ship's destroyed then end the game
    //Returns if defeated or not
    public bool CheckAndMakeShot(Ship[] currentShips, List<ColorRectangle> rectShots)
    {
        MakeShot(rectShots);
        return CheckDefeated(currentShips);
    }

    //Cloning for arrays
    public object Clone() => MemberwiseClone();

    //Local funcs
    private void Grab()
    {
        if (state == State.grabed)
        {
            ShipsPlaced = false;
            grabbingAnyShip = true;

            Mouse.SetCursor(MouseCursor.SizeAll);

            //Rotation
            if (Keyboard.GetState().IsKeyDown(Keys.R) && !MyGame.HoldingR)
            {
                int shipSizeX = rect.Size.X;
                rect.Size = new Point(rect.Size.Y, shipSizeX);
            }

            //Setting the position
            rect.Location = new Point(MyGame.mouse.X - rect.Width / 2, MyGame.mouse.Y - rect.Height / 2);

            //Trying to place
            Place();

            if (state != State.grabed)
                grabbingAnyShip = false;
        }

        if (grabbingAnyShip)
            return;

        if (rect.Contains(MyGame.mouse.Position) && !MyGame.HoldingLeft)
        {
            Mouse.SetCursor(MouseCursor.Hand);

            if (MyGame.mouse.LeftButton == ButtonState.Pressed)
            {
                state = State.grabed;
                grabbingAnyShip = true;
            }
        }
    }

    private void Place()
    {
        bool canPlace = false;

        for (int x = 0; x < boardSize; ++x)
        {
            for (int y = 0; y < boardSize; ++y)
            {
                Rectangle square = new(x * rectSize + placeGridStart, y * rectSize + placeGridStart, rectSize, rectSize);
                Point shipLeftSquare = new Point(rect.Location.X + 16, rect.Location.Y + 16);

                bool onBoard =
                (
                    square.Contains(shipLeftSquare) &&
                    rect.Right <= placeGridStart + boardSize * rectSize &&
                    rect.Bottom <= placeGridStart + boardSize * rectSize
                );

                if (onBoard)
                {
                    MyGame.selected.rect = square;
                    MyGame.selected.rect.Size = rect.Size;
                    MyGame.selected.color = Color.LightGreen;

                    canPlace = true;
                    foreach (Ship ship in setupShips)
                    {
                        if (ship == this) continue;

                        if (MyGame.selected.rect.Intersects(ship.unplaceableArea))
                        {
                            MyGame.selected.color = Color.IndianRed;
                            canPlace = false;
                        }
                    }
                    break;
                }
            }

            if (canPlace) break;
        }

        if (MyGame.mouse.LeftButton == ButtonState.Released)
        {
            if (canPlace)
            {
                rect.Location = MyGame.selected.rect.Location;

                unplaceableArea = new(rect.X - rectSize,
                                      rect.Y - rectSize,
                                      rect.Width + rectSize * 2,
                                      rect.Height + rectSize * 2);

                state = State.placed;
            }
            else
            {
                ResetShip();
                state = State.none;
            }

            ShipsPlaced = Ready();
        }
    }

    private void ResetShip()
    {
        rect.Location = defaultPlace;
        rect.Size = defaultSize;
        unplaceableArea = Rectangle.Empty;
    }


    private void MakeShot(List<ColorRectangle> rectShots)
    {
        shots++;
        if (shots == shipSize) destroyed = true;
    }

    private bool CheckDefeated(Ship[] currentShips)
    {
        bool defeated = true;

        foreach (Ship ship in currentShips)
            if (!ship.destroyed)
                defeated = false;

        return defeated;
    }

    //Static
    static Ship()
    {
        P1 = new Ship[setupShips.Length];
        P2 = new Ship[setupShips.Length];
    }

    private static bool Ready()
    {
        foreach (Ship ship in setupShips)
            if (ship.rect.Location == ship.defaultPlace)
                return false;

        return true;
    }

    public static void DrawShips(SpriteBatch spriteBatch, Ship[]? drawShips)
    {
        if (drawShips == null)
        {
            foreach (Ship ship in setupShips)
                spriteBatch.DrawRectangle(ship.rect, Color.Blue, 4);
            return;
        }

        foreach (Ship ship in drawShips)
            spriteBatch.DrawRectangle(ship.rect, Color.Blue, 4);  
    }

    public static void ForceReady() => ShipsPlaced = true;

    public static void StoreShips(int player)
    {
        //Setting correct positions
        foreach (Ship ship in setupShips)
        {
            ship.rect.Location -= new Point(placeGridStart, placeGridStart);
            ship.rect.Location += player == 1 ? gameGrid1Start : gameGrid2Start;
        }

        Ship[] dest = player == 1 ? P1 : P2;

        for (int i = 0; i < setupShips.Length; ++i)
            dest[i] = (Ship)setupShips[i].Clone();

        ResetShips();
    }

    public static void ResetShips()
    {
        foreach (Ship ship in setupShips)
            ship.ResetShip();

        ShipsPlaced = false;
    }

    public static void UpdateShips() { foreach (Ship ship in setupShips) ship.Grab(); }
}