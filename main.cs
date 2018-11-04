using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    public static int allowedBombCount = 1;
    public static int bombRange = 3;
    static string[] inputBuffer;

    static void Main(string[] args)
    {
        var game = new GameState();

        var gameInfo = ParseGameInfo();

        var target = new Coord(0, 0);

        while (true)
        {
            game.ParseRound(gameInfo);

            //var legitBoxes = FilterBoxesAboutToExplode(game.Bombs, game.Boxes);
            //Console.Error.WriteLine($"found {game.Boxes.Count()}/{legitBoxes.Count()} boxes AND {game.Bombs.Count()}/{game.MyBombs.Count()} bombs");
            //Console.Error.WriteLine($"target {target.X} {target.Y} / my {game.MyLocation.X} {game.MyLocation.Y}");

            target = FindBestTarget(gameInfo, game.Boxes, game.MyLocation, game.MyBombs);
            
            Console.Error.WriteLine($"items {game.Items.Count()}/{game.Bombs.Count()}");
            Console.Error.WriteLine($"target {target.X} {target.Y}/ myLoc {game.MyLocation.X} {game.MyLocation.Y}");
            
            if (target.X == game.MyLocation.X &&
                target.Y == game.MyLocation.Y &&
                game.MyBombs.Count() < allowedBombCount)
            {
                Bomb(target.X, target.Y);
            }
            else
            {
                Move(target.X, target.Y);
            }
        }
    }

    static void Move(int x, int y)
    {
        Console.WriteLine($"MOVE {x} {y}");
    }

    static void Bomb(int x, int y)
    {
        Console.WriteLine($"BOMB {x} {y}");
    }

    static List<Coord> FilterBoxesAboutToExplode(List<Coord> bombs, List<Coord> boxes)
    {
        var result = boxes;
        foreach(var bomb in bombs){
            var boxesFound = BoxesInRange(boxes, bomb, bombRange);
            result.RemoveAll(b => boxesFound.Contains(b));
        }
        return result;
    }

    static Coord FindBestTarget(
        GameInfo info, 
        IEnumerable<Coord> boxes, 
        Coord currentLocation, 
        IEnumerable<Coord> bombs)
    {
        var bestHit = 1;
        var closestDistance = 999;
        var result = new Coord(0, 0);
        
        for (int x = 0; x < info.Width; x++)
        {
            for (int y = 0; y < info.Height; y++)
            {
                if(boxes.Any(b=>b.equals(x,y)) || bombs.Any(b=>b.equals(x,y)))
                    continue;
                    
                var boxesInRange = BoxesInRange(boxes, new Coord(x, y), bombRange);
                var boxCount = boxesInRange.Count();
                var distance = Math.Abs(currentLocation.X - x) + Math.Abs(currentLocation.Y - y);

                if (boxCount > bestHit)
                {
                    bestHit = boxCount;
                    closestDistance = 999;
                }

                if (boxCount == bestHit && distance < closestDistance)
                {
                    closestDistance = distance;
                    result = new Coord(x, y);
                }
            }
        }

        Console.Error.WriteLine($"{result.X} {result.Y}");
        return result;
    }

    static IEnumerable<Coord> BoxesInRange(IEnumerable<Coord> boxes, Coord from, int range)
    {
        return boxes.Where(b =>
            (
                (b.X > from.X - bombRange && b.X < from.X + bombRange &&
                    b.X != from.X && b.Y == from.Y) ||
                (b.Y > from.Y - bombRange && b.Y < from.Y + bombRange &&
                    b.Y != from.Y && b.X == from.X)
            ) && !from.equals(b.X, b.Y)
        );
    }

    static GameInfo ParseGameInfo()
    {
        var gameInfo = new GameInfo();

        inputBuffer = Console.ReadLine().Split(' ');
        gameInfo.Width = int.Parse(inputBuffer[0]);
        gameInfo.Height = int.Parse(inputBuffer[1]);
        gameInfo.MyId = int.Parse(inputBuffer[2]);

        return gameInfo;
    }

    class GameState
    {
        public List<Coord> Boxes = new List<Coord>();
        public List<Coord> Bombs = new List<Coord>();
        public Coord MyLocation = new Coord(0, 0);

        public List<Coord> MyBombs  = new List<Coord>();
        public List<Item> Items = new List<Item>();

        internal void ParseRound(GameInfo info)
        {
            Boxes.Clear();
            MyBombs.Clear();
            Bombs.Clear();
            Items.Clear();

            for (int y = 0; y < info.Height; y++)
            {
                string row = Console.ReadLine();
               // Console.Error.WriteLine(row);
                for (int x = 0; x < row.Count(); x++)
                {
                    if (row[x] != '.')
                    {
                        Boxes.Add(new Coord(x, y));
                    }
                }
            }

            int entities = int.Parse(Console.ReadLine());
            for (int i = 0; i < entities; i++)
            {
                inputBuffer = Console.ReadLine().Split(' ');

                int entityType = int.Parse(inputBuffer[0]);
                int owner = int.Parse(inputBuffer[1]);
                int x = int.Parse(inputBuffer[2]);
                int y = int.Parse(inputBuffer[3]);
                int param1 = int.Parse(inputBuffer[4]);
                int param2 = int.Parse(inputBuffer[5]);
                if (entityType == 0 && owner == info.MyId)
                {
                    MyLocation.X = x;
                    MyLocation.Y = y;
                }

                if (entityType == 1)
                {
                    Bombs.Add(new Coord(x, y));
                    if(owner == info.MyId)
                        MyBombs.Add(new Coord(x, y));
                }

                if (entityType == 2)
                {
                    Items.Add(new Item(){Location = new Coord(x, y), Type = (ItemType)param1});
                }
            }
        }
    }

    class GameInfo
    {
        public int Width;
        public int Height;
        public int MyId;
    }
    
    public class Item
    {
        public Coord Location;
        public ItemType Type;
    }

    public enum ItemType
    {
        ExtraRange = 1,
        ExtraBomb = 2
    }

    public class Coord
    {
        public int X;
        public int Y;

        public Coord(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public bool equals(int cx, int cy)
        {
            return cx == X && cy == Y;
        }

        public bool equals(Coord c)
        {
            return c.X == X && c.Y == Y;
        }
    }
}
