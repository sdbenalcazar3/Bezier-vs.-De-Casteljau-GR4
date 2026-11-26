using System;
using System.Collections.Generic;
using ComparadorBezierDeCasteljau.Modelos;
using ComparadorBezierDeCasteljau.Vistas;

namespace ComparadorBezierDeCasteljau.Controladores
{
    public class ControladorCurvas
    {
        private readonly IVistaPrincipal _vista;
        private readonly EstadoEscena _escena;
        private readonly EvaluadorBernstein _evaluadorBernstein;
        private readonly EvaluadorDeCasteljau _evaluadorDeCasteljau;
        private readonly ServicioBenchmark _servicioBenchmark;

        public ControladorCurvas(
            IVistaPrincipal vista,
            EstadoEscena escena,
            EvaluadorBernstein evaluadorBernstein,
            EvaluadorDeCasteljau evaluadorDeCasteljau,
            ServicioBenchmark servicioBenchmark)
        {
            _vista = vista ?? throw new ArgumentNullException(nameof(vista));
            _escena = escena ?? throw new ArgumentNullException(nameof(escena));
            _evaluadorBernstein = evaluadorBernstein ?? throw new ArgumentNullException(nameof(evaluadorBernstein));
            _evaluadorDeCasteljau = evaluadorDeCasteljau ?? throw new ArgumentNullException(nameof(evaluadorDeCasteljau));
            _servicioBenchmark = servicioBenchmark ?? throw new ArgumentNullException(nameof(servicioBenchmark));

            SuscribirEventosVista();
            CargarPresetCubicoValidacion();
        }

        private void SuscribirEventosVista()
        {
            _vista.PuntoAgregadoSolicitado += AgregarPunto;
            _vista.PuntoModificadoSolicitado += ModificarPunto;
            _vista.PuntoEliminadoSolicitado += EliminarPunto;
            _vista.LimpiarPuntosSolicitado += LimpiarPuntos;
            _vista.PresetCubicoSolicitado += CargarPresetCubicoValidacion;
            _vista.GenerarPuntosAleatoriosSolicitado += GenerarPuntosSinteticos;
            _vista.MuestrasCambiadas += CambiarMuestras;
            _vista.ParametroTCambiado += CambiarParametroT;
            _vista.VisibilidadCapasCambiada += CambiarVisibilidadCapas;
            _vista.CursorLienzoMovido += p => _vista.ActualizarBarraEstado(p, _escena);
        }

        public void CargarPresetCubicoValidacion()
        {
            // Curva cúbica ideal para replicar en la validación manual del informe
            _escena.PuntosControl.Clear();
            _escena.PuntosControl.Add(new Punto2D(60, 80));
            _escena.PuntosControl.Add(new Punto2D(180, 380));
            _escena.PuntosControl.Add(new Punto2D(460, 380));
            _escena.PuntosControl.Add(new Punto2D(580, 80));

            RecalcularYActualizarVista(true);
        }

        private void AgregarPunto(Punto2D punto)
        {
            _escena.PuntosControl.Add(punto);
            RecalcularYActualizarVista(true);
        }

        private void ModificarPunto(int indice, Punto2D nuevoPunto)
        {
            if (indice < 0 || indice >= _escena.PuntosControl.Count) return;

            _escena.PuntosControl[indice] = nuevoPunto;
            RecalcularYActualizarVista(true);
        }

        private void EliminarPunto(int indice)
        {
            if (indice < 0 || indice >= _escena.PuntosControl.Count) return;

            _escena.PuntosControl.RemoveAt(indice);
            RecalcularYActualizarVista(true);
        }

        private void LimpiarPuntos()
        {
            _escena.PuntosControl.Clear();
            RecalcularYActualizarVista(true);
        }

        private void GenerarPuntosSinteticos(int cantidad)
        {
            List<Punto2D> nuevos = _servicioBenchmark.GenerarPuntosControlSinteticos(
                cantidad,
                _vista.AnchoUtilLienzo,
                _vista.AltoUtilLienzo);

            _escena.PuntosControl.Clear();
            _escena.PuntosControl.AddRange(nuevos);
            RecalcularYActualizarVista(true);
        }

        private void CambiarMuestras(int muestras)
        {
            _escena.Muestras = muestras;
            RecalcularYActualizarVista(false);
        }

        private void CambiarParametroT(double t)
        {
            _escena.ParametroT = Math.Max(0.0, Math.Min(1.0, t));
            if (_escena.PuntosControl.Count >= 2)
            {
                _escena.NivelesDeCasteljau = _evaluadorDeCasteljau.CalcularNivelesInterpolacion(
                    _escena.PuntosControl,
                    _escena.ParametroT);
            }
            _vista.RenderizarEscena(_escena);
        }

        private void CambiarVisibilidadCapas(bool poligono, bool bernstein, bool deCasteljau, bool interpolaciones)
        {
            _escena.MostrarPoligonoControl = poligono;
            _escena.MostrarCurvaBernstein = bernstein;
            _escena.MostrarCurvaDeCasteljau = deCasteljau;
            _escena.MostrarInterpolacionesDeCasteljau = interpolaciones;
            _vista.RenderizarEscena(_escena);
        }

        private void RecalcularYActualizarVista(bool actualizarTabla)
        {
            if (_escena.PuntosControl.Count >= 2)
            {
                _escena.CurvaBernstein = _evaluadorBernstein.GenerarCurva(_escena.PuntosControl, _escena.Muestras);
                _escena.CurvaDeCasteljau = _evaluadorDeCasteljau.GenerarCurva(_escena.PuntosControl, _escena.Muestras);
                _escena.NivelesDeCasteljau = _evaluadorDeCasteljau.CalcularNivelesInterpolacion(
                    _escena.PuntosControl,
                    _escena.ParametroT);
                _escena.UltimasMetricasVisuales = MetricasDiferencia.Calcular(
                    _escena.CurvaBernstein,
                    _escena.CurvaDeCasteljau);
            }
            else
            {
                _escena.CurvaBernstein = new Punto2D[0];
                _escena.CurvaDeCasteljau = new Punto2D[0];
                _escena.NivelesDeCasteljau = new List<NivelInterpolacion>();
                _escena.UltimasMetricasVisuales = new MetricasDiferencia { EstadoEstabilidad = "Agrega al menos 2 puntos" };
            }

            if (actualizarTabla)
            {
                _vista.ActualizarTablaPuntos(_escena.PuntosControl);
            }

            _vista.RenderizarEscena(_escena);
        }
    }
}