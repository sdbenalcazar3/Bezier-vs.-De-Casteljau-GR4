using System;

namespace ComparadorBezierDeCasteljau.Modelos
{
    public class MetricasDiferencia
    {
        public double ErrorMaximo { get; set; }
        public double ErrorPromedio { get; set; }
        public double Rmse { get; set; }
        public int PuntosInvalidosBernstein { get; set; }
        public int PuntosInvalidosDeCasteljau { get; set; }
        public int PuntosComparadosValidos { get; set; }
        public string EstadoEstabilidad { get; set; }

        public static MetricasDiferencia Calcular(Punto2D[] curvaBernstein, Punto2D[] curvaDeCasteljau)
        {
            MetricasDiferencia metricas = new MetricasDiferencia();
            if (curvaBernstein == null || curvaDeCasteljau == null)
            {
                metricas.EstadoEstabilidad = "Sin datos";
                return metricas;
            }

            int total = Math.Min(curvaBernstein.Length, curvaDeCasteljau.Length);
            double sumaErrores = 0.0;
            double sumaCuadrados = 0.0;
            double errorMax = 0.0;
            int validos = 0;

            for (int i = 0; i < total; i++)
            {
                bool bValido = curvaBernstein[i].EsValido;
                bool dcValido = curvaDeCasteljau[i].EsValido;

                if (!bValido) metricas.PuntosInvalidosBernstein++;
                if (!dcValido) metricas.PuntosInvalidosDeCasteljau++;

                if (bValido && dcValido)
                {
                    double dist = curvaBernstein[i].DistanciaA(curvaDeCasteljau[i]);
                    if (dist > errorMax) errorMax = dist;
                    sumaErrores += dist;
                    sumaCuadrados += dist * dist;
                    validos++;
                }
            }

            metricas.PuntosComparadosValidos = validos;
            metricas.ErrorMaximo = errorMax;
            metricas.ErrorPromedio = validos > 0 ? sumaErrores / validos : double.NaN;
            metricas.Rmse = validos > 0 ? Math.Sqrt(sumaCuadrados / validos) : double.NaN;

            if (metricas.PuntosInvalidosBernstein > 0 || metricas.PuntosInvalidosDeCasteljau > 0)
            {
                metricas.EstadoEstabilidad = "Inestable (Overflow / NaN)";
            }
            else if (metricas.ErrorMaximo > 1e-6)
            {
                metricas.EstadoEstabilidad = "Divergencia numérica";
            }
            else
            {
                metricas.EstadoEstabilidad = "Estable";
            }

            return metricas;
        }
    }
}