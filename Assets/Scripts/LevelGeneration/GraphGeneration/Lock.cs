using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface Lock<T>
{
    public Key<T> GetNewKey();
    public T Location { get; set; }
}

public interface Key<T>
{
    Lock<T> Lock { get; }
    public T Location { get; set; }
}

public class LockedDoor<T> : Lock<T>
{
    public LockedDoor(T location = default)
    {
        Location = location;
    }

    public T Location { get; set; }

    public Key<T> GetNewKey() => new DoorKey<T>(this, Location);
}

public class DoorKey<T> : Key<T>
{
    public DoorKey(Lock<T> l, T location = default)
    {
        _lock = l;
        Location = location;
    }

    private readonly Lock<T> _lock;

    public Lock<T> Lock => _lock;

    public T Location { get; set; }
}