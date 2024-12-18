using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCArena.Code.Utils
{
    public static class HelperFunctions
    {
        public static Vector2 RotatedBy(this Vector2 v, float degrees)
        {
            float angle = MathF.Atan2(v.Y, v.X);

            angle += MathHelper.ToRadians(degrees);

            return new Vector2(MathF.Cos(angle) * v.Length(), MathF.Sin(angle) * v.Length());
        }
        public static float Angle(this Vector2 v)
        {
            return MathHelper.ToDegrees(MathF.Atan2(v.Y, v.X));
        }

        public static float DistanceTo(this Vector2 v, Vector2 other)
        {
            return (v - other).Length();
        }

        public static float FloatApproach(float start, float end, float interval)
        {
            if (Math.Abs(start - end) <= interval)
            {
                return end;
            }
            if (start < end)
                return Math.Min(start + interval, end);
            else
                return Math.Max(start - interval, end);
        }

        public static void Approach(this ref float start, float end, float interval)
        {
            if (Math.Abs(start - end) <= interval)
            {
                start = end;
                return;
            }
            if (start < end)
                start = Math.Min(start + interval, end);
            else
                start = Math.Max(start - interval, end);
        }

        #region curves

        public static Vector2 QuadraticCurve(Vector2 start, Vector2 mid, Vector2 end, float percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            return MathF.Pow(1 - percent, 2) * start + 2 * (1 - percent) * percent * mid + MathF.Pow(percent, 2) * end;
        }
        public static Vector2 QuadraticCurveDerivative(Vector2 start, Vector2 mid, Vector2 end, float percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            return 2 * (1 - percent) * (mid - start) + 2 * percent * (end - mid);
        }
        public static Vector2 QuadraticCurve(Vector2[] points, float percent)
        {
            if (points.Length < 3) throw new Exception("Quadratic curve function requires 3 points as input.");
            percent = Math.Clamp(percent, 0, 1);
            return MathF.Pow(1 - percent, 2) * points[0] + 2 * (1 - percent) * percent * points[1] + MathF.Pow(percent, 2) * points[2];
        }
        public static Vector2 QuadraticCurveDerivative(Vector2[] points, float percent)
        {
            if (points.Length < 3) throw new Exception("Quadratic curve function requires 3 points as input.");
            percent = Math.Clamp(percent, 0, 1);
            return 2 * (1 - percent) * (points[1] - points[0]) + 2 * percent * (points[2] - points[1]);
        }

        public static Vector3 QuadraticCurve(Vector3 start, Vector3 mid, Vector3 end, float percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            return MathF.Pow(1 - percent, 2) * start + 2 * (1 - percent) * percent * mid + MathF.Pow(percent, 2) * end;
        }
        public static Vector3 QuadraticCurveDerivative(Vector3 start, Vector3 mid, Vector3 end, float percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            return 2 * (1 - percent) * (mid - start) + 2 * percent * (end - mid);
        }
        public static Vector3 QuadraticCurve(Vector3[] points, float percent)
        {
            if (points.Length < 3) throw new Exception("Quadratic curve function requires 3 points as input.");
            percent = Math.Clamp(percent, 0, 1);
            return MathF.Pow(1 - percent, 2) * points[0] + 2 * (1 - percent) * percent * points[1] + MathF.Pow(percent, 2) * points[2];
        }
        public static Vector3 QuadraticCurveDerivative(Vector3[] points, float percent)
        {
            if (points.Length < 3) throw new Exception("Quadratic curve function requires 3 points as input.");
            percent = Math.Clamp(percent, 0, 1);
            return 2 * (1 - percent) * (points[1] - points[0]) + 2 * percent * (points[2] - points[1]);
        }

        public static Vector2 CubicCurve(Vector2 start, Vector2 mid1, Vector2 mid2, Vector2 end, float percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            return MathF.Pow(1 - percent, 3) * start + 3 * MathF.Pow(1 - percent, 2) * percent * mid1 +
                3 * MathF.Pow(percent, 2) * (1 - percent) * mid2 + MathF.Pow(percent, 3) * end;
        }
        public static Vector2 CubicCurveDerivative(Vector2 start, Vector2 mid1, Vector2 mid2, Vector2 end, float percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            return 3 * MathF.Pow(1 - percent, 2) * (mid1 - start) + 6 * (1 - percent) * percent * (mid2 - mid1) + 3 * MathF.Pow(percent, 2) * (end - mid2);
        }
        public static Vector2 CubicCurve(Vector2[] points, float percent)
        {
            if (points.Length < 4) throw new Exception("Cubic curve function requires 4 points as input.");
            percent = Math.Clamp(percent, 0, 1);
            return MathF.Pow(1 - percent, 3) * points[0] + 3 * MathF.Pow(1 - percent, 2) * percent * points[1] +
                3 * MathF.Pow(percent, 2) * (1 - percent) * points[2] + MathF.Pow(percent, 3) * points[3];
        }
        public static Vector2 CubicCurveDerivative(Vector2[] points, float percent)
        {
            if (points.Length < 4) throw new Exception("Cubic curve function requires 4 points as input.");
            percent = Math.Clamp(percent, 0, 1);
            return 3 * MathF.Pow(1 - percent, 2) * (points[1] - points[0]) + 6 * (1 - percent) * percent * (points[2] - points[1]) + 3 * MathF.Pow(percent, 2) * (points[3] - points[2]);
        }

        public static Vector3 CubicCurve(Vector3 start, Vector3 mid1, Vector3 mid2, Vector3 end, float percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            return MathF.Pow(1 - percent, 3) * start + 3 * MathF.Pow(1 - percent, 2) * percent * mid1 +
                3 * MathF.Pow(percent, 2) * (1 - percent) * mid2 + MathF.Pow(percent, 3) * end;
        }
        public static Vector3 CubicCurveDerivative(Vector3 start, Vector3 mid1, Vector3 mid2, Vector3 end, float percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            return 3 * MathF.Pow(1 - percent, 2) * (mid1 - start) + 6 * (1 - percent) * percent * (mid2 - mid1) + 3 * MathF.Pow(percent, 2) * (end - mid2);
        }
        public static Vector3 CubicCurve(Vector3[] points, float percent)
        {
            if (points.Length < 4) throw new Exception("Cubic curve function requires 4 points as input.");
            percent = Math.Clamp(percent, 0, 1);
            return MathF.Pow(1 - percent, 3) * points[0] + 3 * MathF.Pow(1 - percent, 2) * percent * points[1] +
                3 * MathF.Pow(percent, 2) * (1 - percent) * points[2] + MathF.Pow(percent, 3) * points[3];
        }
        public static Vector3 CubicCurveDerivative(Vector3[] points, float percent)
        {
            if (points.Length < 4) throw new Exception("Cubic curve function requires 4 points as input.");
            percent = Math.Clamp(percent, 0, 1);
            return 3 * MathF.Pow(1 - percent, 2) * (points[1] - points[0]) + 6 * (1 - percent) * percent * (points[2] - points[1]) + 3 * MathF.Pow(percent, 2) * (points[3] - points[2]);
        }

        #endregion

        public static void ModifyPixels(this Texture2D sprite, Action<Color[], int, int, int> pixelMethod)
        {
            Color[] c = new Color[sprite.Width * sprite.Height];
            sprite.GetData(c);

            for (int i = 0; i < c.Length; i++)
            {
                int x = i % sprite.Width;
                int y = i / sprite.Width;

                pixelMethod.Invoke(c, i, x, y);
            }

            sprite.SetData(c);
        }

        public static int IndexWhere<T>(this IList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i])) return i;
            }
            return -1;
        }

        public static (int x, int y) IndexWhere<T>(this T[,] list, Predicate<T> predicate)
        {
            for (int x = 0; x < list.GetLength(0); x++)
            {
                for (int y = 0; y < list.GetLength(1); y++) if (predicate(list[x, y])) return (x, y);
            }
            return (-1, -1);
        }

        public static Rectangle RectangleGroup(List<Rectangle> recs)
        {
            Rectangle result = recs[0];

            for (int i = 0; i < recs.Count; i++) result = Rectangle.Union(result, recs[i]);

            return result;
        }
    }
}
