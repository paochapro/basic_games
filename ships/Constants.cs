using Microsoft.Xna.Framework;

namespace Ships;

static class Constants
{
    //General
    public static readonly Point screenGameSize = new(512 + screenSize / 2, screenSize);
    public const int boardSize = 10;
    public const int screenSize = 512;
    public const int rectSize = 32;

    //Grid
    public static Color gameGrid1Color = Color.LightBlue;
    public static Color gameGrid2Color = Color.LightBlue;

    //Grid placing
    public static readonly Point boxSize = new(screenGameSize.X / 2, screenGameSize.Y);
    public static readonly Rectangle leftBox = new(0, 0, boxSize.X, boxSize.Y);
    public static readonly Rectangle rightBox = new(screenGameSize.X / 2, 0, boxSize.X, boxSize.Y);

    public const int placeGridStart = screenSize / 2 - gridSize / 2;
    public static readonly Point gameGrid1Start = new(leftBox.Center.X - gridSize / 2, leftBox.Center.Y - gridSize / 2);
    public static readonly Point gameGrid2Start = new(rightBox.Center.X - gridSize / 2, rightBox.Center.Y - gridSize / 2);

    //Game grid
    public const int gridSize = rectSize * boardSize;

    static Constants()
    {
        boxSize = new(screenGameSize.X / 2, screenGameSize.Y);
        leftBox = new(0, 0, boxSize.X, boxSize.Y);
        rightBox = new(screenGameSize.X / 2, 0, boxSize.X, boxSize.Y);

        gameGrid1Start = new(leftBox.Center.X - gridSize / 2, leftBox.Center.Y - gridSize / 2);
        gameGrid2Start = new(rightBox.Center.X - gridSize / 2, rightBox.Center.Y - gridSize / 2);
    }
}
