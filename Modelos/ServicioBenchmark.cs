using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ComparadorBezierDeCasteljau.Modelos
{
    public class ServicioBenchmark
    {
        private readonly IEvaluadorCurva _evaluadorBernstein;
        private readonly IEvaluadorCurva _evaluadorDeCasteljau;

        public ServicioBenchmark(
            IEvaluadorCurva evaluadorBernstein,
            IEvaluadorCurva evaluadorDeCasteljau)
        {
            _evaluadorBernstein = evaluadorBernstein ?? throw new ArgumentNullException(nameof(evaluadorBernstein));
            _evaluadorDeCasteljau = evaluadorDeCasteljau ?? throw new ArgumentNullException(nameof(evaluadorDeCasteljau));
        }

        public ResultadoBenchmark EjecutarPrueba(
            IReadOnlyList<Punto2D> puntosControl,
            int muestras,
            int repeticiones)
        {
            if (puntosControl == null || puntosControl.Count < 2)
            {
                throw new ArgumentException("Se requieren al menos 2 puntos de control para ejecutar la prueba.");
            }
            if (muestras < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(muestras), "Debe usar al menos 2 muestras.");
            }
            if (repeticiones < 10)
            {
                throw new ArgumentOutOfRangeException(nameof(repeticiones), "Cada prueba debe ejecutarse al menos 10 veces.");
            }

            // Copia plana e inmutable para aislar completamente la ejecución de la UI
            Punto2D[] puntosCopia = new Punto2D[puntosControl.Count];
            for (int i = 0; i < puntosControl.Count; i++)
            {
                puntosCopia[i] = puntosControl[i];
            }

            // Calentamiento (warm-up) del compilador JIT antes de cronometrar
            _evaluadorBernstein.GenerarCurva(puntosCopia, Math.Min(muestras, 20));
            _evaluadorDeCasteljau.GenerarCurva(puntosCopia, Math.Min(muestras, 20));

            double sumaB = 0.0, minB = double.MaxValue, maxB = 0.0;
            double sumaDc = 0.0, minDc = double.MaxValue, maxDc = 0.0;

            Punto2D[] ultimaCurvaB = null;
            Punto2D[] ultimaCurvaDc = null;
            Stopwatch cronometro = new Stopwatch();

            // Medición exclusiva del cómputo matemático (cero operaciones gráficas)
            for (int r = 0; r < repeticiones; r++)
            {
                cronometro.Restart();
                ultimaCurvaB = _evaluadorBernstein.GenerarCurva(puntosCopia, muestras);
                cronometro.Stop();
                double msB = cronometro.Elapsed.TotalMilliseconds;

                sumaB += msB;
                if (msB < minB) minB = msB;
                if (msB > maxB) maxB = msB;

                cronometro.Restart();
                ultimaCurvaDc = _evaluadorDeCasteljau.GenerarCurva(puntosCopia, muestras);
                cronometro.Stop();
                double msDc = cronometro.Elapsed.TotalMilliseconds;

                sumaDc += msDc;
                if (msDc < minDc) minDc = msDc;
                if (msDc > maxDc) maxDc = msDc;
            }

            MetricasDiferencia diferencias = MetricasDiferencia.Calcular(ultimaCurvaB, ultimaCurvaDc);

            return new ResultadoBenchmark
            {
                FechaHora = DateTime.Now,
                CantidadPuntosControl = puntosCopia.Length,
                Grado = puntosCopia.Length - 1,
                Muestras = muestras,
                Repeticiones = repeticiones,
                TiempoPromedioBernsteinMs = sumaB / repeticiones,
                TiempoMinBernsteinMs = minB,
                TiempoMaxBernsteinMs = maxB,
                TiempoPromedioDeCasteljauMs = sumaDc / repeticiones,
                TiempoMinDeCasteljauMs = minDc,
                TiempoMaxDeCasteljauMs = maxDc,
                Diferencias = diferencias
            };
        }

        public List<Punto2D> GenerarPuntosControlSinteticos(int cantidad, double anchoArea, double altoArea)
        {
            List<Punto2D> puntos = new List<Punto2D>(cantidad);
            double margenX = 50.0;
            double margenY = 60.0;
            double anchoUtil = Math.Max(200.0, anchoArea - (2.0 * margenX));
            double altoUtil = Math.Max(180.0, altoArea - (2.0 * margenY));

            for (int i = 0; i < cantidad; i++)
            {
                double fraccion = cantidad > 1 ? (double)i / (cantidad - 1) : 0.5;
                double x = margenX + (fraccion * anchoUtil);
                double onda = Math.Sin(fraccion * Math.PI * 3.0) * 0.42 + 0.5;
                double y = margenY + (onda * altoUtil);
                puntos.Add(new Punto2D(Math.Round(x, 2), Math.Round(y, 2)));
            }

            return puntos;
        }
    }
}