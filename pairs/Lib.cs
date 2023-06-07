using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Pairs;

static class Utils
{
    static public void print(params object[] args)
    {
        foreach (object var in args)
            Console.Write(var + " ");
        Console.WriteLine();
    }
    static public int clamp(int value, int min, int max)
    {
        if (value > max) value = max;
        if (value < min) value = min;
        return value;
    }
}

struct ColorRectangle
{
    public Rectangle rect = Rectangle.Empty;
    public Color color = Color.Gray;
    public ColorRectangle(Rectangle rect) => this.rect = rect;
    public ColorRectangle(Rectangle rect, Color color) : this(rect) => this.color = color;
    public ColorRectangle() { }
}

//Events
class Event
{
    double delay;
    double startTime;
    Action function;

    public Event(Action function, double delay)
    {
        this.delay = delay;
        this.function = function;
        startTime = MyGame.globalTime;
    }

    static List<Event> events = new();

    static public void Add(Action func, double delay) => events.Add(new Event(func, delay));

    static public void ExecuteEvents()
    {
        for (int i = 0; i < events.Count; ++i)
        {
            Event ev = events[i];
            if ((MyGame.globalTime - ev.startTime) > ev.delay)
            {
                ev.function.Invoke();
                events.Remove(ev);
                --i;
            }
        }
    }
    
    static public void ClearEvents() => events.Clear();
}

//Button
class Button
{
    static List<Button> buttons = new();
    static public int currentLayer { get; set; }

    Rectangle rect = Rectangle.Empty;
    Color color = Color.Black;
    Action<object[]> func;

    object[] args;
    string text;
    int layer;

    //Features
    Texture2D? custom_texture = null;
    public bool Locked { get; set; }

    public Button(Rectangle rect, Action<object[]> func, string text, int layer, Texture2D? texture = null,params object[] args)
    {
        this.rect = rect;
        this.args = args;
        this.func = func;
        this.text = text;
        this.layer = layer;
        this.custom_texture = texture;
    }

    private void Update()
    {
        if(Locked)
        {
            color = Color.Gray;
            return;
        }

        color = Color.Black;

        if (rect.Contains(MyGame.mouse.Position) && !MyGame.clicking)
        {
            color = new(255,201,14);
            Mouse.SetCursor(MouseCursor.Hand);

            if (MyGame.mouse.LeftButton == ButtonState.Pressed)
                func(args);
        }
    }

    private void Draw(SpriteBatch spriteBatch)
    {
        if(custom_texture != null)
        {
            spriteBatch.Draw(custom_texture, rect, Color.White);
            return;
        }

        spriteBatch.DrawRectangle(rect, color, 4);

        Vector2 measure = MyGame.textFont.MeasureString(text);
        Vector2 position = new Vector2((rect.X + rect.Width / 2) - measure.X / 2, (rect.Y + rect.Height / 2) - measure.Y / 2);

        spriteBatch.DrawString(MyGame.textFont, text, position, color);
    }

    static public void UpdateButtons()
    {
        foreach (Button button in buttons)
            if(button.layer == currentLayer)
                button.Update();
    }

    static public void DrawButtons(SpriteBatch spriteBatch)
    {
        foreach (Button button in buttons)
            if (button.layer == currentLayer)
                button.Draw(spriteBatch);
    }

    static public Button Add(Rectangle rect, Action<object[]> func, string text, int layer, Texture2D? texture = null, params object[] args)
    {
        buttons.Add(new Button(rect, func, text, layer, texture, args));
        return buttons.Last();
    }
}