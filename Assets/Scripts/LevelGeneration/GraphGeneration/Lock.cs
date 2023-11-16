using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface Lock
{
    public Key GetNewKey();
}

public interface Key
{
    Lock Lock { get; }
}

public class LockedDoor : Lock
{
    public Key GetNewKey() => new DoorKey(this);
}

public class DoorKey : Key
{
    public DoorKey(Lock l)
    {
        _lock = l;
    }

    private readonly Lock _lock;

    public Lock Lock => _lock;
}