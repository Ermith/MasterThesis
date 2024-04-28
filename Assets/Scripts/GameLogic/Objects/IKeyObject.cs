using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Spawnable objects that contains <see cref="IKey"/>.
/// </summary>
public interface IKeyObject
{
    IKey MyKey { get; set; }
}
