using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EnemyParams
{
    public (int, int) Spawn;
    public IEnumerable<(int, int)> Patrol;
}
