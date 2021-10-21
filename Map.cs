using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Tetris;

public class Map : Node2D
{

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
    private bool _doRotate = false;

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
        if (@event.IsAction("ui_select")) _doRotate = true;
        else if (@event.IsAction("ui_down")) cursor.move(new Vector2(0, 1));
        else if (inBounds(cursor, _current_tetrimonio))
        {
            if (@event.IsAction("ui_left")) cursor.move(new Vector2(-1, 0));
            if (@event.IsAction("ui_right")) cursor.move(new Vector2(1, 0));
        }
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

            (int, int)? rws = rowsAreFull();
            if (rws.HasValue)
            {
                (int, int) rows = rws.Value;
                removeFullRows(rows.Item1, rows.Item2);
            }

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
                
                if (_doRotate)
                {
                    _current_tetrimonio = _current_tetrimonio.Rotate();
                    _doRotate = false;
                }
                
                drawMap(map,map_sheet);
                drawTet(cursor, _current_tetrimonio);
            }
        }
        
    }

    public Cursor makeNewCursor()
    {
        return new Cursor(cols / 2, 0);
    }

    public void drawTet(Cursor orig, Tetrimino tet)
    {
        tet.Points.Select(vec => vec + orig.asVector()).ToList()
            .ForEach(vec => map.SetCell((int) vec.x, (int) vec.y, (int) Tile.CELL));
    }
    
    public void iterateRows(int start_row, int end_row, Action<int, int> action)
    {
        for (int i = start_row; i <= end_row; ++i)
        {
            for (int j = 0; j < cols; j++)
            {
                action(j, i);
            }
        }
    }

    public void drawMap(TileMap to, TileMap from)
    {
        GD.Print("drawMap\n");
        iterateRows(0, rows -1, (x, y) =>
        {
                to.SetCell(x, y, from.GetCell(x, y));
        });
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
        float maxY = tet.Points.Max(vec => vec.y);
        return tet.Points
            .Where(vec => (int) vec.y == (int) maxY)
            .Select(vec => vec + cursor.asVector())
            .Select(vec => isTile(vec.x, vec.y + 1, Tile.WALL, Tile.CELL))
            .Aggregate((b, b1) => b || b1);
    }

    public bool inBounds(Cursor cursor, Tetrimino tet)
    {
        return tet.Points
            .Select(vec => vec + cursor.asVector())
            .Select(vec => vec.x > 0 && vec.x < cols - 1)
            .Aggregate((b, b1) => b || b1);
    }

    // We scan from top to bottom
    public (int, int)? rowsAreFull()
    {
        int start_row = 0, end_row = 0;
        bool firstFull = true;
        for (int cur_row = 0; cur_row <  rows - 1; ++cur_row)
        {
            bool isFull = true;
            for (int cur_col = 1; cur_col < cols - 1; ++cur_col)
            {
                if (!isTile(cur_col, cur_row, Tile.CELL))
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull)
            {
                if (firstFull)
                {
                    start_row = end_row = cur_row;
                    firstFull = false;
                }
                else
                {
                    end_row = cur_row;
                }
            }
        }

        if (start_row != 0)
        {
            return (start_row, end_row);
        } 
        return null;
    }


    public void removeFullRows(int start_row, int end_row)
    {
        // remove tiles (CELL) in the area from start_row to end_row
        iterateRows(start_row, end_row, (x, y) =>
        {
            if (isTile(x, y, Tile.CELL)) map.SetCell(x, y, -1); // remove CELL
        });
        
        // move tiles (CELL) above start_row (end_row - start_row + 1) rows down 
        int toMoveDown = end_row - start_row + 1;
        iterateRows(0, start_row - 1, (x, y) =>
        {
            if (isTile(x, y, Tile.CELL)) {
                map.SetCell(x, y, -1); 
                map.SetCell(x, y + toMoveDown, (int) Tile.CELL);
            }
        });
    }
}
