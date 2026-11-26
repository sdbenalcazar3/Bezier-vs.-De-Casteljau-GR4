using ComparadorBezierDeCasteljau.Vistas;
using ComparadorBezierDeCasteljau.Controladores;
using ComparadorBezierDeCasteljau.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComparadorBezierDeCasteljau
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormularioPrincipal vista = new FormularioPrincipal();
            EstadoEscena escena = new EstadoEscena();
            EvaluadorBernstein evaluadorBernstein = new EvaluadorBernstein();
            EvaluadorDeCasteljau evaluadorDeCasteljau = new EvaluadorDeCasteljau();
            ServicioBenchmark servicioBenchmark = new ServicioBenchmark(
                evaluadorBernstein,
                evaluadorDeCasteljau);

            new ControladorCurvas(
                vista,
                escena,
                evaluadorBernstein,
                evaluadorDeCasteljau,
                servicioBenchmark);
            new ControladorBenchmark(vista, escena, servicioBenchmark);

            Application.Run(vista);
        }
    }
}
