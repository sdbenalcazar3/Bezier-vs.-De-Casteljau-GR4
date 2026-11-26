using System;
using System.Collections.Generic;

namespace ComparadorBezierDeCasteljau.Modelos
{
    public class EvaluadorDeCasteljau : IEvaluadorCurva
    {
        public string Nombre
        {
            get { return "De Casteljau"; }
        }

        public Punto2D EvaluarPunto(IReadOnlyList<Punto2D> puntosControl, double t)
        {
            ValidarPuntos(puntosControl);

            int cantidad = puntosControl.Count;
            if (cantidad == 1)
            {
                return puntosControl[0];
            }

            Punto2D[] buffer = new Punto2D[cantidad];
            return EvaluarConBuffer(puntosControl, buffer, t);
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

            // Reutiliza un único arreglo para evitar presión al GC durante el benchmark
            Punto2D[] buffer = new Punto2D[cantidad];
            double paso = 1.0 / (muestras - 1);

            for (int k = 0; k < muestras; k++)
            {
                double t = (k == muestras - 1) ? 1.0 : k * paso;
                resultado[k] = EvaluarConBuffer(puntosControl, buffer, t);
            }

            return resultado;
        }

        public IReadOnlyList<NivelInterpolacion> CalcularNivelesInterpolacion(
            IReadOnlyList<Punto2D> puntosControl,
            double t)
        {
            List<NivelInterpolacion> niveles = new List<NivelInterpolacion>();
            if (puntosControl == null || puntosControl.Count == 0)
            {
                return niveles;
            }

            Punto2D[] actual = new Punto2D[puntosControl.Count];
            for (int i = 0; i < puntosControl.Count; i++)
            {
                actual[i] = puntosControl[i];
            }

            niveles.Add(new NivelInterpolacion(0, actual));

            int n = puntosControl.Count - 1;
            for (int r = 1; r <= n; r++)
            {
                Punto2D[] siguiente = new Punto2D[n - r + 1];
                for (int i = 0; i < siguiente.Length; i++)
                {
                    siguiente[i] = Punto2D.Interpolar(actual[i], actual[i + 1], t);
                }

                niveles.Add(new NivelInterpolacion(r, siguiente));
                actual = siguiente;
            }

            return niveles;
        }

        private static Punto2D EvaluarConBuffer(
            IReadOnlyList<Punto2D> puntosControl,
            Punto2D[] buffer,
            double t)
        {
            int cantidad = puntosControl.Count;
            for (int i = 0; i < cantidad; i++)
            {
                buffer[i] = puntosControl[i];
            }

            int n = cantidad - 1;
            for (int r = 1; r <= n; r++)
            {
                int limite = n - r;
                for (int i = 0; i <= limite; i++)
                {
                    buffer[i] = Punto2D.Interpolar(buffer[i], buffer[i + 1], t);
                }
            }

            return buffer[0];
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