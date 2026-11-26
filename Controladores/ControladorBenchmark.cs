using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComparadorBezierDeCasteljau.Modelos;
using ComparadorBezierDeCasteljau.Utilidades;
using ComparadorBezierDeCasteljau.Vistas;

namespace ComparadorBezierDeCasteljau.Controladores
{
	public class ControladorBenchmark
	{
		private readonly IVistaPrincipal _vista;
		private readonly EstadoEscena _escena;
		private readonly ServicioBenchmark _servicioBenchmark;
		private readonly List<ResultadoBenchmark> _historial;

		public ControladorBenchmark(
			IVistaPrincipal vista,
			EstadoEscena escena,
			ServicioBenchmark servicioBenchmark)
		{
			_vista = vista ?? throw new ArgumentNullException(nameof(vista));
			_escena = escena ?? throw new ArgumentNullException(nameof(escena));
			_servicioBenchmark = servicioBenchmark ?? throw new ArgumentNullException(nameof(servicioBenchmark));
			_historial = new List<ResultadoBenchmark>();

			SuscribirEventosVista();
		}

		private void SuscribirEventosVista()
		{
			_vista.EjecutarBenchmarkActualSolicitado += async reps => await EjecutarPruebaActualAsync(reps);
			_vista.EjecutarMatrizExperimentalSolicitado += async reps => await EjecutarMatrizExperimentalAsync(reps);
			_vista.ExportarCsvSolicitado += ExportarHistorialCsv;
			_vista.LimpiarHistorialBenchmarkSolicitado += LimpiarHistorial;
		}

		private async Task EjecutarPruebaActualAsync(int repeticiones)
		{
			if (_escena.PuntosControl.Count < 2)
			{
				_vista.MostrarMensaje("Puntos insuficientes", "Agrega al menos 2 puntos de control antes de ejecutar la prueba.", true);
				return;
			}

			// Captura inmutable de parámetros antes de pasar al hilo secundario
			List<Punto2D> copiaPuntos = new List<Punto2D>(_escena.PuntosControl);
			int muestras = _escena.Muestras;

			_vista.EstablecerEstadoEjecucionBenchmark(true, "Ejecutando benchmark aislado...");

			try
			{
				ResultadoBenchmark resultado = await Task.Run(() =>
					_servicioBenchmark.EjecutarPrueba(copiaPuntos, muestras, repeticiones));

				_historial.Add(resultado);
				_vista.MostrarResultadoBenchmark(resultado);
				_vista.ActualizarHistorialBenchmark(_historial);
				_vista.EstablecerEstadoEjecucionBenchmark(false, "Prueba completada (" + repeticiones + " repeticiones).");
			}
			catch (Exception ex)
			{
				_vista.EstablecerEstadoEjecucionBenchmark(false, "Error en la ejecución.");
				_vista.MostrarMensaje("Error de benchmark", ex.Message, true);
			}
		}

		private async Task EjecutarMatrizExperimentalAsync(int repeticiones)
		{
			// Matriz diseñada para evidenciar O(n) vs O(n²) y el overflow de Bernstein en n >= 171
			int[] cantidadesPuntos = new int[] { 4, 10, 25, 50, 100, 175 };
			int[] cantidadesMuestras = new int[] { 100, 1000 };
			double ancho = _vista.AnchoUtilLienzo;
			double alto = _vista.AltoUtilLienzo;

			_vista.EstablecerEstadoEjecucionBenchmark(true, "Ejecutando matriz experimental...");

			try
			{
				List<ResultadoBenchmark> resultadosLote = await Task.Run(() =>
				{
					List<ResultadoBenchmark> acumulados = new List<ResultadoBenchmark>();
					foreach (int cantPuntos in cantidadesPuntos)
					{
						List<Punto2D> puntos = _servicioBenchmark.GenerarPuntosControlSinteticos(cantPuntos, ancho, alto);
						foreach (int muestras in cantidadesMuestras)
						{
							ResultadoBenchmark res = _servicioBenchmark.EjecutarPrueba(puntos, muestras, repeticiones);
							acumulados.Add(res);
						}
					}
					return acumulados;
				});

				_historial.AddRange(resultadosLote);
				if (resultadosLote.Count > 0)
				{
					_vista.MostrarResultadoBenchmark(resultadosLote[resultadosLote.Count - 1]);
				}

				_vista.ActualizarHistorialBenchmark(_historial);
				_vista.EstablecerEstadoEjecucionBenchmark(
					false,
					"Matriz finalizada: " + resultadosLote.Count + " escenarios registrados.");
			}
			catch (Exception ex)
			{
				_vista.EstablecerEstadoEjecucionBenchmark(false, "Error en la matriz experimental.");
				_vista.MostrarMensaje("Error de benchmark", ex.Message, true);
			}
		}

		private void ExportarHistorialCsv()
		{
			if (_historial.Count == 0)
			{
				_vista.MostrarMensaje("Sin resultados", "Ejecuta al menos una prueba antes de exportar a CSV.", true);
				return;
			}

			string ruta = _vista.SolicitarRutaGuardadoCsv();
			if (string.IsNullOrEmpty(ruta)) return;

			try
			{
				ExportadorCsv.ExportarResultados(ruta, _historial);
				_vista.MostrarMensaje("Exportación exitosa", "Los resultados se guardaron correctamente en:\n" + ruta, false);
			}
			catch (Exception ex)
			{
				_vista.MostrarMensaje("Error al exportar", ex.Message, true);
			}
		}

		private void LimpiarHistorial()
		{
			_historial.Clear();
			_vista.ActualizarHistorialBenchmark(_historial);
			_vista.EstablecerEstadoEjecucionBenchmark(false, "Historial limpiado.");
		}
	}
}