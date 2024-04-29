using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Contains static functions for easing and modify these functions.
/// </summary>
public class Easing {
	public static float Reverse(Func<float, float> f, float t) => 1 - f(1 -t);
	public static float Mix(Func<float, float> a, Func<float, float> b, float t) => (1 - t) * a(t) + t * b(t);
	public static float SmoothStart(float t) => t * t;
	public static float SmoothEnd(float t) => Reverse(SmoothStart, t);
	public static float SmoothStep(float t) => Mix(SmoothStart, SmoothEnd, t);
}
