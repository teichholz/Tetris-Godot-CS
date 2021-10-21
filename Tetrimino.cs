using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Tetris
{
    public class Tetrimino
    {
        private List<Vector2> points;

        public List<Vector2> Points => points;

        public Tetrimino(List<Vector2> points)
        {
            this.points = points;
        }
        
        // ####
        public static Tetrimino one = new Tetrimino(createPoints((0, 0), (1,0), (2,0), (3,0)));
        // #
        // ###
        public static Tetrimino two = new Tetrimino(createPoints((0, 0), (0,-1), (1,0), (2,0)));
        //   #
        // ###
        public static Tetrimino three = new Tetrimino(createPoints((0, 0), (2,-1), (1,0), (2,0)));
        // ##
        // ##
        public static Tetrimino four = new Tetrimino(createPoints((0, 0), (0,1), (1,0), (1,1)));
        //  ##
        // ##
        public static Tetrimino five = new Tetrimino(createPoints((0, 0), (0,-1), (0,-1), (1,-1)));
        //  #
        // ###
        public static Tetrimino six = new Tetrimino(createPoints((0, 0), (0,-1), (0, -1), (-1,0)));
        // ##
        //  ##
        public static Tetrimino seven = new Tetrimino(createPoints((0, 0), (0,1), (-1, 0), (-1,-1)));
        
        
        // if only these could be enums
        public static Tetrimino[] Tetriminos = { one, two, three, four, five, six, seven };

        private static  Random rand = new Random(42);
        public static Tetrimino getRandomTetrimonio()
        {
            return Tetriminos[rand.Next(0, Tetriminos.Length - 1)];
        }

        private static List<Vector2> createPoints(params (int, int)[] args)
        {
            return args
                .Select(point => new Vector2(point.Item1, point.Item2))
                .ToList();
        }

        // rotates by 90 degree
        public Tetrimino Rotate()
        {
            double degree = Math.PI * 0.5; // 90
            return new Tetrimino(points.Select(x => rotatePoint(x, degree)).ToList());
        }

        Vector2 rotatePoint(Vector2 point, double degree)
        {
            int x = (int) Math.Round(point.x * Math.Cos(degree) - point.y * Math.Sin(degree));
            int y = (int) Math.Round(point.x * Math.Sin(degree) + point.y * Math.Cos(degree));
            return new Vector2(x, y);
        }
    }
}