using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Tetris;

public class Map : Node2D
{
    // Width 12
    // Height h

    private static int width = (int) ProjectSettings.GetSetting("display/window/size/width");
    private static int height = (int) ProjectSettings.GetSetting("display/window/size/height");
    private static int squareSize = 64;

    private static int cols = width / squareSize;
    private static int rows = height / squareSize;

    private TileMap map;
    // We save the map as a template to redraw. Saving happens at specific times:
    // 1. At the start
    // 2. When a tetrimonio hits the ground
    private TileMap map_sheet;
    // Basically a Vector2 which is used as origin to draw tetrimonios
    private Cursor cursor ;

    private InputEvent _lastInputEvent;

    private float _tick = 0.5f;
    private float _tick_ctr = 0f;
    private Tetrimino _current_tetrimonio;
    private bool _tetrimonio_alive = false;

    public enum Tile
    {
        CELL,
        WALL
    }
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        map = GetNode<TileMap>("TileMap");
        map_sheet = new TileMap();
        drawMap(map_sheet, map);
        cursor = makeNewCursor();
    }

    public override void _Input(InputEvent @event)
    {
        _lastInputEvent = @event;
    }

    public override void _Process(float delta)
    {
        if (!_tetrimonio_alive)
        {
            cursor = makeNewCursor();
            _current_tetrimonio = Tetrimino.one;
            _tetrimonio_alive = true;
        }

        if (hitGround(cursor, _current_tetrimonio))
        {
            GD.Print("Hit ground\n");
            drawMap(map_sheet, map);
            _tetrimonio_alive = false;
        }
        else
        {
            _tick_ctr += delta;
            if (_tick_ctr >= _tick)
            {
                cursor.tick();
                _tick_ctr = 0;
                
                drawMap(map,map_sheet);
                drawTet(cursor, _current_tetrimonio);
            }
        }

        if (_lastInputEvent != null)
            cursor.move(_lastInputEvent);
        
        _lastInputEvent = null;
    }

    public Cursor makeNewCursor()
    {
        return new Cursor(cols / 2, -1);
    }

    public void drawTet(Cursor orig, Tetrimino tet)
    {
        tet.Points.Select(vec => vec + orig.asVector()).ToList()
            .ForEach(vec => map.SetCell((int) vec.x, (int) vec.y, (int) Tile.CELL));
    }

    public void drawMap(TileMap to, TileMap from)
    {
        GD.Print("drawMap\n");
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                to.SetCell(row, col, from.GetCell(row, col));
            }
        }
    }

    public bool isTile(int x, int y, params Tile[] tiles)
    {
        return tiles
            .Select(tile => map.GetCell(x, y) == (int) tile)
            .Aggregate((b, b1) => b || b1);
    }
    
    public bool isTile(float x, float y, params Tile[] tiles)
    {
        return isTile((int) x, (int) y, tiles);
    }

    public bool hitGround(Cursor cursor, Tetrimino tet)
    {
        return tet.Points
            .Select(vec => vec + cursor.asVector())
            .Select(vec => isTile(vec.x, vec.y + 1, Tile.WALL, Tile.CELL))
            .Aggregate((b, b1) => b || b1);
    }

    public bool inBounds(Tetrimino tet)
    {
        return tet.Points
            .Select(vec => vec.x > 0 && vec.x < cols - 1)
            .Aggregate((b, b1) => b || b1);
    }
}
