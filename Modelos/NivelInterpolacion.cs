using System.Collections.Generic;

namespace ComparadorBezierDeCasteljau.Modelos
{
    public class NivelInterpolacion
    {
        public int Nivel { get; }
        public IReadOnlyList<Punto2D> Puntos { get; }

        public NivelInterpolacion(int nivel, IReadOnlyList<Punto2D> puntos)
        {
            Nivel = nivel;
            Puntos = puntos;
        }
    }
}