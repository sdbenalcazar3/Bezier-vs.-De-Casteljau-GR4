using System;
using System.Collections.Generic;
using ComparadorBezierDeCasteljau.Modelos;

namespace ComparadorBezierDeCasteljau.Vistas
{
    public interface IVistaPrincipal
    {
        // Eventos de interacción de curva y puntos
        event Action<Punto2D> PuntoAgregadoSolicitado;
        event Action<int, Punto2D> PuntoModificadoSolicitado;
        event Action<int> PuntoEliminadoSolicitado;
        event Action LimpiarPuntosSolicitado;
        event Action PresetCubicoSolicitado;
        event Action<int> GenerarPuntosAleatoriosSolicitado;

        // Eventos de parámetros visuales
        event Action<int> MuestrasCambiadas;
        event Action<double> ParametroTCambiado;
        event Action<bool, bool, bool, bool> VisibilidadCapasCambiada;
        event Action<Punto2D> CursorLienzoMovido;

        // Eventos de benchmark
        event Action<int> EjecutarBenchmarkActualSolicitado;
        event Action<int> EjecutarMatrizExperimentalSolicitado;
        event Action ExportarCsvSolicitado;
        event Action LimpiarHistorialBenchmarkSolicitado;

        // Propiedades de consulta de dimensiones del lienzo
        double AnchoUtilLienzo { get; }
        double AltoUtilLienzo { get; }

        // Métodos de actualización desde el controlador
        void RenderizarEscena(EstadoEscena escena);
        void ActualizarTablaPuntos(IReadOnlyList<Punto2D> puntos);
        void ActualizarBarraEstado(Punto2D cursorMundo, EstadoEscena escena);
        void MostrarResultadoBenchmark(ResultadoBenchmark resultado);
        void ActualizarHistorialBenchmark(IReadOnlyList<ResultadoBenchmark> historial);
        void EstablecerEstadoEjecucionBenchmark(bool ejecutando, string mensajeProgreso);
        void MostrarMensaje(string titulo, string mensaje, bool esError);
        string SolicitarRutaGuardadoCsv();
    }
}