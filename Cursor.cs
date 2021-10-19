using Godot;

namespace Tetris
{
    public struct Cursor
    {
        private int x, y;

        public Cursor(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        public Vector2 asVector()
        {
            return new Vector2(x, y);
        }
        
        public Cursor move(InputEvent evt)
        {
            if (evt.IsActionPressed("ui_down"))
            {
                y += 1;
            } else if (evt.IsActionPressed("ui_right"))
            {
                x += 1;
            } else if (evt.IsActionPressed("ui_left"))
            {
                x -= 1;
            }

            return this;
        }

        public Cursor tick()
        {
            y += 1;
            return this;
        }
    }
}