using System.Collections.Generic;

namespace ComparadorBezierDeCasteljau.Modelos
{
    public class EstadoEscena
    {
        public List<Punto2D> PuntosControl { get; }
        public Punto2D[] CurvaBernstein { get; set; }
        public Punto2D[] CurvaDeCasteljau { get; set; }
        public IReadOnlyList<NivelInterpolacion> NivelesDeCasteljau { get; set; }
        public MetricasDiferencia UltimasMetricasVisuales { get; set; }

        public int Muestras { get; set; }
        public double ParametroT { get; set; }

        public bool MostrarPoligonoControl { get; set; }
        public bool MostrarCurvaBernstein { get; set; }
        public bool MostrarCurvaDeCasteljau { get; set; }
        public bool MostrarInterpolacionesDeCasteljau { get; set; }

        public int IndicePuntoSeleccionado { get; set; }

        public EstadoEscena()
        {
            PuntosControl = new List<Punto2D>();
            CurvaBernstein = new Punto2D[0];
            CurvaDeCasteljau = new Punto2D[0];
            NivelesDeCasteljau = new List<NivelInterpolacion>();
            UltimasMetricasVisuales = new MetricasDiferencia { EstadoEstabilidad = "Sin datos" };

            Muestras = 100;
            ParametroT = 0.5;
            MostrarPoligonoControl = true;
            MostrarCurvaBernstein = true;
            MostrarCurvaDeCasteljau = true;
            MostrarInterpolacionesDeCasteljau = true;
            IndicePuntoSeleccionado = -1;
        }

        public int Grado
        {
            get { return PuntosControl.Count > 0 ? PuntosControl.Count - 1 : 0; }
        }
    }
}