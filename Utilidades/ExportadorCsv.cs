using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ComparadorBezierDeCasteljau.Modelos;

namespace ComparadorBezierDeCasteljau.Utilidades
{
    public static class ExportadorCsv
    {
        public static void ExportarResultados(string rutaArchivo, IEnumerable<ResultadoBenchmark> resultados)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(
                "FechaHora,PuntosControl,Grado,Muestras,Repeticiones," +
                "BernsteinPromMs,BernsteinMinMs,BernsteinMaxMs," +
                "DeCasteljauPromMs,DeCasteljauMinMs,DeCasteljauMaxMs," +
                "RelacionDC_B,ErrorMax,ErrorProm,RMSE,InvalidosBernstein,InvalidosDeCasteljau,EstadoEstabilidad");

            CultureInfo inv = CultureInfo.InvariantCulture;

            foreach (ResultadoBenchmark r in resultados)
            {
                MetricasDiferencia d = r.Diferencias ?? new MetricasDiferencia();
                sb.AppendFormat(inv,
                    "{0:yyyy-MM-dd HH:mm:ss},{1},{2},{3},{4},{5:F6},{6:F6},{7:F6},{8:F6},{9:F6},{10:F6},{11:F4},{12:E4},{13:E4},{14:E4},{15},{16},\"{17}\"",
                    r.FechaHora,
                    r.CantidadPuntosControl,
                    r.Grado,
                    r.Muestras,
                    r.Repeticiones,
                    r.TiempoPromedioBernsteinMs,
                    r.TiempoMinBernsteinMs,
                    r.TiempoMaxBernsteinMs,
                    r.TiempoPromedioDeCasteljauMs,
                    r.TiempoMinDeCasteljauMs,
                    r.TiempoMaxDeCasteljauMs,
                    r.RelacionTiempoDcSobreB,
                    d.ErrorMaximo,
                    d.ErrorPromedio,
                    d.Rmse,
                    d.PuntosInvalidosBernstein,
                    d.PuntosInvalidosDeCasteljau,
                    d.EstadoEstabilidad ?? string.Empty);
                sb.AppendLine();
            }

            // UTF-8 con BOM para correcta visualización de acentos en Excel
            File.WriteAllText(rutaArchivo, sb.ToString(), new UTF8Encoding(true));
        }
    }
}