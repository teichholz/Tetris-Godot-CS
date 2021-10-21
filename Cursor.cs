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
        
        public Cursor move(Vector2 vec)
        {
            x += (int) vec.x;
            y += (int) vec.y;
            return this;
        }

        public Cursor tick()
        {
            y += 1;
            return this;
        }
    }
}