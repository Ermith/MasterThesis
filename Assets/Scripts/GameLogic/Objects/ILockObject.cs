using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Spawnable object that contains <see cref="ILock"/>. Can be unlocked.
/// </summary>
public interface ILockObject
{
    ILock Lock { get; set; }

    /// <summary>
    /// Should remove the obstacle this lock presents.
    /// </summary>
    void Unlock();
}
