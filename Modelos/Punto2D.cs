using System;
using System.Globalization;

namespace ComparadorBezierDeCasteljau.Modelos
{
    public struct Punto2D
    {
        public double X { get; }
        public double Y { get; }

        public Punto2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool EsValido
        {
            get
            {
                return !double.IsNaN(X) && !double.IsInfinity(X)
                    && !double.IsNaN(Y) && !double.IsInfinity(Y);
            }
        }

        public double DistanciaA(Punto2D otro)
        {
            double dx = X - otro.X;
            double dy = Y - otro.Y;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        public static Punto2D Interpolar(Punto2D a, Punto2D b, double t)
        {
            double unoMenosT = 1.0 - t;
            return new Punto2D(
                (unoMenosT * a.X) + (t * b.X),
                (unoMenosT * a.Y) + (t * b.Y));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0:F2}, {1:F2})", X, Y);
        }
    }
}