using System;

namespace ComparadorBezierDeCasteljau.Modelos
{
    public class ResultadoBenchmark
    {
        public DateTime FechaHora { get; set; }
        public int CantidadPuntosControl { get; set; }
        public int Grado { get; set; }
        public int Muestras { get; set; }
        public int Repeticiones { get; set; }

        public double TiempoPromedioBernsteinMs { get; set; }
        public double TiempoMinBernsteinMs { get; set; }
        public double TiempoMaxBernsteinMs { get; set; }

        public double TiempoPromedioDeCasteljauMs { get; set; }
        public double TiempoMinDeCasteljauMs { get; set; }
        public double TiempoMaxDeCasteljauMs { get; set; }

        public double RelacionTiempoDcSobreB
        {
            get
            {
                if (TiempoPromedioBernsteinMs <= 0.0) return 0.0;
                return TiempoPromedioDeCasteljauMs / TiempoPromedioBernsteinMs;
            }
        }

        public MetricasDiferencia Diferencias { get; set; }
    }
}