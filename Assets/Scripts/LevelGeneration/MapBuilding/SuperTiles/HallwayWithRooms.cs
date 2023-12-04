using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HallwayWithRooms : Hallway
{
    public HallwayWithRooms(int width, int height, Directions exits = Directions.None) : base(width, height, exits)
    {
    }

    private void BuildSubRoom(int x, int y, int width, int height, ATile[,] tileGrid, Directions roomExits)
    {
        for (int i = 0; i < width; i++)
        {
            var edges = EdgeDirectinons(i, 0, width, height);
            var exits = (i == width / 2 && roomExits.North()) ? Directions.North : Directions.None;
            tileGrid[i + x, y] = new EdgeTile(edges, exits);

            edges = EdgeDirectinons(i, width - 1, width, height);
            exits = (i == width / 2 && roomExits.South()) ? Directions.South : Directions.None;
            tileGrid[i + x, y + height - 1] = new EdgeTile(edges, exits);
        }

        for (int i = 0; i < height; i++)
        {
            var edges = EdgeDirectinons(0, i, width, height);
            var exits = (i == height / 2 && roomExits.West()) ? Directions.West : Directions.None;
            var tile = tileGrid[x, i + y] as EdgeTile ?? new EdgeTile(edges);
            tile.Exits |= exits;
            tileGrid[x, i + y] = tile;


            edges = EdgeDirectinons(width - 1, i, width, height);
            exits = (i == height / 2 && roomExits.East()) ? Directions.East : Directions.None;

            tile = tileGrid[x + width - 1, i + y] as EdgeTile ?? new EdgeTile(edges);
            tile.Exits |= exits;
            tileGrid[x + width - 1, i + y] = tile;
        }

        for (int i = x; i < x + width; i++)
            for (int j = y; j < y + height; j++)
                if (tileGrid[i, j] == null)
                    tileGrid[i, j] = new EmptyTile();
    }

    public override EnemyParams BuildTiles(int x, int y, ATile[,] tileGrid)
    {
        EnemyParams enemyParams = base.BuildTiles(x, y, tileGrid);

        int roomWidth = Width / 2;
        int roomHeight = Height / 2;

        var nw = Directions.None;
        if (Exits.West()) nw |= Directions.South;
        if (Exits.North()) nw |= Directions.East;

        var ne = Directions.None;
        if (Exits.East()) ne |= Directions.South;
        if (Exits.North()) ne |= Directions.West;

        var sw = Directions.None;
        if (Exits.West()) sw |= Directions.North;
        if (Exits.South()) sw |= Directions.East;

        var se = Directions.None;
        if (Exits.East()) se |= Directions.North;
        if (Exits.South()) se |= Directions.West;


        if (Exits.North() || Exits.West())
            BuildSubRoom(
                x + 0,
                y + 0,
                roomWidth, roomHeight,
                tileGrid, nw
                );

        if (Exits.North() || Exits.East())
            BuildSubRoom(
                x + Width / 2 + 1,
                y + 0,
                roomWidth, roomHeight,
                tileGrid, ne
                );

        if (Exits.South() || Exits.West())
            BuildSubRoom(
                x + 0,
                y + Height / 2 + 1,
                roomWidth, roomHeight,
                tileGrid, sw
                );

        if (Exits.South() || Exits.East())
            BuildSubRoom(
                x + Width / 2 + 1,
                y + Height / 2 + 1,
                roomWidth, roomHeight,
                tileGrid, se
                );

        if (tileGrid[x + Width / 2, y + roomHeight / 2] is EdgeTile northEdge)
            northEdge.Exits = Directions.East | Directions.West;

        if (tileGrid[x + Width / 2, y + Width - roomHeight / 2] is EdgeTile southEdge)
            southEdge.Exits = Directions.East | Directions.West;

        if (tileGrid[x + roomWidth / 2, y + Height / 2] is EdgeTile eastEdge)
            eastEdge.Exits = Directions.North | Directions.South;

        if (tileGrid[x + Width - roomWidth / 2, y + Height / 2] is EdgeTile westEdge)
            westEdge.Exits = Directions.North | Directions.South;

        return enemyParams;
    }
}