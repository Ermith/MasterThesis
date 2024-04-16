using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DoorSubTile : ASubTile
{
    public DoorLock DoorLock = null;
    public Directions Orientation = Directions.North;
    public bool Up;
    public bool Down;

    public string Name { get; private set; }
    public void SetName(string roomName)
    {
        Name = roomName;
        if (!Orientation.None()) Name += Orientation.ToString();
        if (Up) Name += "Up";
        if (Down) Name += "Down";
    }
}
