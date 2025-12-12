using System.Collections.Generic;

namespace SnakeGame.Client.Game;

public class Snake
{
    public LinkedList<Point> Body { get; } = new();
    public Direction CurrentDirection { get; set; } = Direction.Right;

    public Snake(int startX, int startY, int initialLength = 3)
    {
        for (int i = 0; i < initialLength; i++)
        {
            Body.AddLast(new Point(startX - i, startY));
        }
    }

    public Point Head => Body.First!.Value;

    public void Move(Point newHead, bool grow = false)
    {
        Body.AddFirst(newHead);
        if (!grow)
            Body.RemoveLast();
    }
}