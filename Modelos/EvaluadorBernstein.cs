using System;
using System.Collections.Generic;

namespace ComparadorBezierDeCasteljau.Modelos
{
    public class EvaluadorBernstein : IEvaluadorCurva
    {
        public string Nombre
        {
            get { return "Bernstein (Directo)"; }
        }

        public Punto2D EvaluarPunto(IReadOnlyList<Punto2D> puntosControl, double t)
        {
            ValidarPuntos(puntosControl);

            int cantidad = puntosControl.Count;
            if (cantidad == 1)
            {
                return puntosControl[0];
            }

            int n = cantidad - 1;
            double[] factoriales = ConstruirTablaFactoriales(n);
            return EvaluarConFactoriales(puntosControl, factoriales, n, t);
        }

        public Punto2D[] GenerarCurva(IReadOnlyList<Punto2D> puntosControl, int muestras)
        {
            ValidarPuntos(puntosControl);
            if (muestras < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(muestras), "Debe haber al menos 2 muestras.");
            }

            Punto2D[] resultado = new Punto2D[muestras];
            int cantidad = puntosControl.Count;

            if (cantidad == 1)
            {
                for (int k = 0; k < muestras; k++)
                {
                    resultado[k] = puntosControl[0];
                }
                return resultado;
            }

            int n = cantidad - 1;
            double[] factoriales = ConstruirTablaFactoriales(n);
            double paso = 1.0 / (muestras - 1);

            for (int k = 0; k < muestras; k++)
            {
                double t = (k == muestras - 1) ? 1.0 : k * paso;
                resultado[k] = EvaluarConFactoriales(puntosControl, factoriales, n, t);
            }

            return resultado;
        }

        private static Punto2D EvaluarConFactoriales(
            IReadOnlyList<Punto2D> puntosControl,
            double[] factoriales,
            int n,
            double t)
        {
            double unoMenosT = 1.0 - t;
            double factN = factoriales[n];
            double sumaX = 0.0;
            double sumaY = 0.0;

            for (int i = 0; i <= n; i++)
            {
                double coefBinomial = factN / (factoriales[i] * factoriales[n - i]);
                double polinomioBernstein = coefBinomial * Math.Pow(t, i) * Math.Pow(unoMenosT, n - i);

                sumaX += polinomioBernstein * puntosControl[i].X;
                sumaY += polinomioBernstein * puntosControl[i].Y;
            }

            return new Punto2D(sumaX, sumaY);
        }

        private static double[] ConstruirTablaFactoriales(int n)
        {
            double[] tabla = new double[n + 1];
            tabla[0] = 1.0;

            for (int i = 1; i <= n; i++)
            {
                tabla[i] = tabla[i - 1] * i;
            }

            return tabla;
        }

        private static void ValidarPuntos(IReadOnlyList<Punto2D> puntosControl)
        {
            if (puntosControl == null || puntosControl.Count == 0)
            {
                throw new ArgumentException("Se requiere al menos un punto de control.", nameof(puntosControl));
            }
        }
    }
}