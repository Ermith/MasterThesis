using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Utils
{
    public static IEnumerable<(int, int)> GetShortPath(int startX, int startY, int endX, int endY, bool yFirst = false)
    {
        if (yFirst)
        {
            int step = MathF.Sign(endY - startY);
            for (int y = startY; y != endY; y += step)
                yield return (startX, y);

            step = MathF.Sign(endX - startX);
            for (int x = startX; x != endX; x += step)
                yield return (x, endY);

        } else
        {
            int step = MathF.Sign(endX - startX);
            for (int x = startX; x != endX; x += step)
                yield return (x, startY);

            step = MathF.Sign(endY - startY);
            for (int y = startY; y != endY; y += step)
                yield return (endX, y);
        }

        yield return (endX, endY);
    }
}