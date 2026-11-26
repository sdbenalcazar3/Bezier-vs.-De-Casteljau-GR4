using System.Drawing;

namespace ComparadorBezierDeCasteljau.Utilidades
{
    public static class TemaVisual
    {
        public static readonly Color FondoVentana = Color.FromArgb(244, 246, 250);
        public static readonly Color FondoPanel = Color.White;
        public static readonly Color FondoLienzo = Color.FromArgb(252, 253, 255);
        public static readonly Color BordeSuave = Color.FromArgb(218, 224, 234);

        public static readonly Color Cuadricula = Color.FromArgb(236, 240, 246);
        public static readonly Color Ejes = Color.FromArgb(120, 132, 150);
        public static readonly Color TextoPrimario = Color.FromArgb(30, 41, 59);
        public static readonly Color TextoSecundario = Color.FromArgb(100, 116, 139);

        public static readonly Color PoligonoControl = Color.FromArgb(148, 163, 184);
        public static readonly Color PuntoControl = Color.FromArgb(30, 41, 59);
        public static readonly Color PuntoControlActivo = Color.FromArgb(14, 165, 233);

        // Colores de alto contraste para visualización simultánea
        public static readonly Color CurvaBernstein = Color.FromArgb(37, 99, 235);     // Azul sólido
        public static readonly Color CurvaDeCasteljau = Color.FromArgb(234, 88, 12);   // Naranja discontinuo
        public static readonly Color PuntoEvaluadoT = Color.FromArgb(220, 38, 38);     // Rojo intenso

        public static readonly Color EstadoOk = Color.FromArgb(22, 163, 74);
        public static readonly Color EstadoAlerta = Color.FromArgb(217, 119, 6);
        public static readonly Color EstadoError = Color.FromArgb(220, 38, 38);

        public static readonly Color[] PaletaNivelesDeCasteljau = new Color[]
        {
            Color.FromArgb(16, 185, 129),  // Nivel 1: Esmeralda
            Color.FromArgb(139, 92, 246),  // Nivel 2: Violeta
            Color.FromArgb(236, 72, 153),  // Nivel 3: Magenta
            Color.FromArgb(245, 158, 11),  // Nivel 4: Ámbar
            Color.FromArgb(6, 182, 212)    // Nivel 5: Cian
        };

        public static readonly Font FuenteBase = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font FuenteNegrita = new Font("Segoe UI", 9F, FontStyle.Bold);
        public static readonly Font FuenteTitulo = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        public static readonly Font FuenteEtiquetaLienzo = new Font("Consolas", 8F, FontStyle.Regular);
        public static readonly Font FuenteKpi = new Font("Segoe UI", 11F, FontStyle.Bold);
    }
}