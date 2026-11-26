using System.Collections.Generic;

namespace ComparadorBezierDeCasteljau.Modelos
{
    public interface IEvaluadorCurva
    {
        string Nombre { get; }

        Punto2D EvaluarPunto(IReadOnlyList<Punto2D> puntosControl, double t);

        Punto2D[] GenerarCurva(IReadOnlyList<Punto2D> puntosControl, int muestras);
    }
}