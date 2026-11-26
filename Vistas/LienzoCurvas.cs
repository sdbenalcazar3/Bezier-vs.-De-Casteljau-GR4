using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using ComparadorBezierDeCasteljau.Modelos;
using ComparadorBezierDeCasteljau.Utilidades;

namespace ComparadorBezierDeCasteljau.Vistas
{
    public class LienzoCurvas : Control
    {
        private const int MargenIzquierdo = 52;
        private const int MargenInferior = 36;
        private const int MargenDerecho = 20;
        private const int MargenSuperior = 20;
        private const float RadioDeteccionPx = 10f;
        private const double LimiteSeguroGdi = 1e6;

        private EstadoEscena _escena;
        private int _indiceArrastre = -1;
        private int _indiceHover = -1;

        public event Action<Punto2D> PuntoAgregadoEnLienzo;
        public event Action<int, Punto2D> PuntoMovidoEnLienzo;
        public event Action<int> PuntoEliminadoEnLienzo;
        public event Action<Punto2D> CursorMovidoEnLienzo;

        public LienzoCurvas()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = TemaVisual.FondoLienzo;
            Cursor = Cursors.Cross;
        }

        public double AnchoUtilMundo
        {
            get { return Math.Max(100.0, Width - MargenIzquierdo - MargenDerecho); }
        }

        public double AltoUtilMundo
        {
            get { return Math.Max(100.0, Height - MargenSuperior - MargenInferior); }
        }

        public void EstablecerEscena(EstadoEscena escena)
        {
            _escena = escena;
            Invalidate();
        }

        public PointF MundoAPantalla(Punto2D punto)
        {
            float px = (float)(MargenIzquierdo + punto.X);
            float py = (float)((Height - MargenInferior) - punto.Y);
            return new PointF(px, py);
        }

        public Punto2D PantallaAMundo(Point puntoPantalla)
        {
            double x = Math.Round((double)(puntoPantalla.X - MargenIzquierdo), 2);
            double y = Math.Round((double)((Height - MargenInferior) - puntoPantalla.Y), 2);
            return new Punto2D(x, y);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int indiceHit = BuscarPuntoCercano(e.Location);

            if (e.Button == MouseButtons.Left)
            {
                if (indiceHit >= 0)
                {
                    _indiceArrastre = indiceHit;
                    Cursor = Cursors.SizeAll;
                }
                else
                {
                    Punto2D nuevo = PantallaAMundo(e.Location);
                    PuntoAgregadoEnLienzo?.Invoke(nuevo);
                }
            }
            else if (e.Button == MouseButtons.Right && indiceHit >= 0)
            {
                PuntoEliminadoEnLienzo?.Invoke(indiceHit);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Punto2D mundo = PantallaAMundo(e.Location);
            CursorMovidoEnLienzo?.Invoke(mundo);

            if (_indiceArrastre >= 0 && e.Button == MouseButtons.Left)
            {
                PuntoMovidoEnLienzo?.Invoke(_indiceArrastre, mundo);
                return;
            }

            int nuevoHover = BuscarPuntoCercano(e.Location);
            if (nuevoHover != _indiceHover)
            {
                _indiceHover = nuevoHover;
                Cursor = _indiceHover >= 0 ? Cursors.Hand : Cursors.Cross;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_indiceArrastre >= 0)
            {
                _indiceArrastre = -1;
                Cursor = _indiceHover >= 0 ? Cursors.Hand : Cursors.Cross;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(TemaVisual.FondoLienzo);

            DibujarCuadriculaYEjes(g);

            if (_escena == null || _escena.PuntosControl.Count == 0)
            {
                DibujarMensajeGuiaInicial(g);
                return;
            }

            if (_escena.MostrarPoligonoControl)
            {
                DibujarPoligonoControl(g);
            }

            if (_escena.MostrarCurvaBernstein)
            {
                DibujarCurvaConDrawLine(g, _escena.CurvaBernstein, TemaVisual.CurvaBernstein, 3.0f, null);
            }

            if (_escena.MostrarCurvaDeCasteljau)
            {
                DibujarCurvaConDrawLine(g, _escena.CurvaDeCasteljau, TemaVisual.CurvaDeCasteljau, 2.2f, new float[] { 5f, 3.5f });
            }

            if (_escena.MostrarInterpolacionesDeCasteljau && _escena.PuntosControl.Count >= 2)
            {
                DibujarInterpolacionesDeCasteljau(g);
            }

            DibujarNodosControl(g);
        }

        private void DibujarCuadriculaYEjes(Graphics g)
        {
            int origenX = MargenIzquierdo;
            int origenY = Height - MargenInferior;
            int paso = 50;

            using (Pen lapizCuadricula = new Pen(TemaVisual.Cuadricula, 1f))
            using (Pen lapizEje = new Pen(TemaVisual.Ejes, 1.5f))
            using (Brush pincelTexto = new SolidBrush(TemaVisual.TextoSecundario))
            {
                // Líneas verticales de cuadrícula
                for (int x = origenX; x < Width - MargenDerecho; x += paso)
                {
                    g.DrawLine(lapizCuadricula, x, MargenSuperior, x, origenY);
                    int valorX = x - origenX;
                    if (valorX > 0 && valorX % 100 == 0)
                    {
                        g.DrawLine(lapizEje, x, origenY - 3, x, origenY + 3);
                        g.DrawString(valorX.ToString(CultureInfo.InvariantCulture), TemaVisual.FuenteEtiquetaLienzo, pincelTexto, x - 12, origenY + 6);
                    }
                }

                // Líneas horizontales de cuadrícula
                for (int y = origenY; y > MargenSuperior; y -= paso)
                {
                    g.DrawLine(lapizCuadricula, origenX, y, Width - MargenDerecho, y);
                    int valorY = origenY - y;
                    if (valorY > 0 && valorY % 100 == 0)
                    {
                        g.DrawLine(lapizEje, origenX - 3, y, origenX + 3, y);
                        g.DrawString(valorY.ToString(CultureInfo.InvariantCulture), TemaVisual.FuenteEtiquetaLienzo, pincelTexto, 8, y - 6);
                    }
                }

                // Ejes cartesianos X e Y mediante DrawLine
                g.DrawLine(lapizEje, origenX, origenY, Width - MargenDerecho, origenY);
                g.DrawLine(lapizEje, origenX, origenY, origenX, MargenSuperior);

                // Puntas de flecha de ejes con DrawLine
                int finX = Width - MargenDerecho;
                g.DrawLine(lapizEje, finX, origenY, finX - 6, origenY - 4);
                g.DrawLine(lapizEje, finX, origenY, finX - 6, origenY + 4);

                int finY = MargenSuperior;
                g.DrawLine(lapizEje, origenX, finY, origenX - 4, finY + 6);
                g.DrawLine(lapizEje, origenX, finY, origenX + 4, finY + 6);

                g.DrawString("0", TemaVisual.FuenteEtiquetaLienzo, pincelTexto, origenX - 14, origenY + 4);
                g.DrawString("X", TemaVisual.FuenteNegrita, pincelTexto, finX - 12, origenY + 8);
                g.DrawString("Y", TemaVisual.FuenteNegrita, pincelTexto, origenX - 20, finY - 4);
            }
        }

        private void DibujarPoligonoControl(Graphics g)
        {
            if (_escena.PuntosControl.Count < 2) return;

            using (Pen lapizPoligono = new Pen(TemaVisual.PoligonoControl, 1.3f))
            {
                lapizPoligono.DashPattern = new float[] { 4f, 4f };

                for (int i = 0; i < _escena.PuntosControl.Count - 1; i++)
                {
                    PointF a = MundoAPantalla(_escena.PuntosControl[i]);
                    PointF b = MundoAPantalla(_escena.PuntosControl[i + 1]);
                    g.DrawLine(lapizPoligono, a, b);
                }
            }
        }

        private void DibujarCurvaConDrawLine(
            Graphics g,
            Punto2D[] puntosCurva,
            Color color,
            float grosor,
            float[] patronGuion)
        {
            if (puntosCurva == null || puntosCurva.Length < 2) return;

            using (Pen lapiz = new Pen(color, grosor))
            {
                if (patronGuion != null)
                {
                    lapiz.DashPattern = patronGuion;
                }

                for (int i = 0; i < puntosCurva.Length - 1; i++)
                {
                    Punto2D p1 = puntosCurva[i];
                    Punto2D p2 = puntosCurva[i + 1];

                    if (!EsDibujableEnGdi(p1) || !EsDibujableEnGdi(p2))
                    {
                        continue;
                    }

                    PointF s1 = MundoAPantalla(p1);
                    PointF s2 = MundoAPantalla(p2);
                    g.DrawLine(lapiz, s1, s2);
                }
            }
        }

        private void DibujarInterpolacionesDeCasteljau(Graphics g)
        {
            if (_escena.NivelesDeCasteljau == null || _escena.NivelesDeCasteljau.Count <= 1) return;

            // Evita saturación visual extrema cuando el grado supera 30, mostrando solo el punto B(t)
            int totalNiveles = _escena.NivelesDeCasteljau.Count;
            bool mostrarSegmentosIntermedios = _escena.PuntosControl.Count <= 30;

            if (mostrarSegmentosIntermedios)
            {
                for (int r = 1; r < totalNiveles - 1; r++)
                {
                    NivelInterpolacion nivel = _escena.NivelesDeCasteljau[r];
                    Color colorNivel = TemaVisual.PaletaNivelesDeCasteljau[(r - 1) % TemaVisual.PaletaNivelesDeCasteljau.Length];

                    using (Pen lapizNivel = new Pen(colorNivel, 1.35f))
                    {
                        for (int i = 0; i < nivel.Puntos.Count - 1; i++)
                        {
                            PointF a = MundoAPantalla(nivel.Puntos[i]);
                            PointF b = MundoAPantalla(nivel.Puntos[i + 1]);
                            g.DrawLine(lapizNivel, a, b);
                        }

                        for (int i = 0; i < nivel.Puntos.Count; i++)
                        {
                            PointF centro = MundoAPantalla(nivel.Puntos[i]);
                            DibujarRomboConDrawLine(g, lapizNivel, centro, 3.5f);
                        }
                    }
                }
            }

            // Punto final B(t) destacado mediante rombo concéntrico y cruz usando DrawLine
            NivelInterpolacion nivelFinal = _escena.NivelesDeCasteljau[totalNiveles - 1];
            if (nivelFinal.Puntos.Count == 1 && EsDibujableEnGdi(nivelFinal.Puntos[0]))
            {
                PointF pt = MundoAPantalla(nivelFinal.Puntos[0]);
                using (Pen lapizFinal = new Pen(TemaVisual.PuntoEvaluadoT, 2.2f))
                using (Brush pincelTexto = new SolidBrush(TemaVisual.PuntoEvaluadoT))
                {
                    DibujarRomboConDrawLine(g, lapizFinal, pt, 7f);
                    DibujarRomboConDrawLine(g, lapizFinal, pt, 3.5f);
                    g.DrawLine(lapizFinal, pt.X - 10f, pt.Y, pt.X + 10f, pt.Y);
                    g.DrawLine(lapizFinal, pt.X, pt.Y - 10f, pt.X, pt.Y + 10f);

                    string etiquetaT = string.Format(
                        CultureInfo.InvariantCulture,
                        "B({0:F2})",
                        _escena.ParametroT);
                    g.DrawString(etiquetaT, TemaVisual.FuenteNegrita, pincelTexto, pt.X + 10f, pt.Y - 16f);
                }
            }
        }

        private void DibujarNodosControl(Graphics g)
        {
            int cantidad = _escena.PuntosControl.Count;
            bool mostrarTodasEtiquetas = cantidad <= 25;

            using (Pen lapizNormal = new Pen(TemaVisual.PuntoControl, 1.8f))
            using (Pen lapizActivo = new Pen(TemaVisual.PuntoControlActivo, 2.2f))
            using (Brush pincelEtiqueta = new SolidBrush(TemaVisual.TextoPrimario))
            {
                for (int i = 0; i < cantidad; i++)
                {
                    PointF c = MundoAPantalla(_escena.PuntosControl[i]);
                    bool activo = (i == _indiceHover || i == _indiceArrastre);
                    Pen lapiz = activo ? lapizActivo : lapizNormal;
                    float radio = activo ? 6f : 4.5f;

                    DibujarCuadradoConDrawLine(g, lapiz, c, radio);
                    DibujarCuadradoConDrawLine(g, lapiz, c, radio * 0.45f);

                    if (mostrarTodasEtiquetas || i == 0 || i == cantidad - 1 || i % 5 == 0)
                    {
                        g.DrawString("P" + i, TemaVisual.FuenteEtiquetaLienzo, pincelEtiqueta, c.X + 7f, c.Y - 14f);
                    }
                }
            }
        }

        private void DibujarMensajeGuiaInicial(Graphics g)
        {
            using (Brush pincel = new SolidBrush(TemaVisual.TextoSecundario))
            {
                string texto = "Clic izquierdo: agregar punto de control  |  Arrastrar: mover punto  |  Clic derecho sobre punto: eliminar";
                g.DrawString(texto, TemaVisual.FuenteBase, pincel, MargenIzquierdo + 16, MargenSuperior + 16);
            }
        }

        private static void DibujarCuadradoConDrawLine(Graphics g, Pen pen, PointF c, float r)
        {
            g.DrawLine(pen, c.X - r, c.Y - r, c.X + r, c.Y - r);
            g.DrawLine(pen, c.X + r, c.Y - r, c.X + r, c.Y + r);
            g.DrawLine(pen, c.X + r, c.Y + r, c.X - r, c.Y + r);
            g.DrawLine(pen, c.X - r, c.Y + r, c.X - r, c.Y - r);
        }

        private static void DibujarRomboConDrawLine(Graphics g, Pen pen, PointF c, float r)
        {
            g.DrawLine(pen, c.X, c.Y - r, c.X + r, c.Y);
            g.DrawLine(pen, c.X + r, c.Y, c.X, c.Y + r);
            g.DrawLine(pen, c.X, c.Y + r, c.X - r, c.Y);
            g.DrawLine(pen, c.X - r, c.Y, c.X, c.Y - r);
        }

        private static bool EsDibujableEnGdi(Punto2D punto)
        {
            return punto.EsValido
                && Math.Abs(punto.X) < LimiteSeguroGdi
                && Math.Abs(punto.Y) < LimiteSeguroGdi;
        }

        private int BuscarPuntoCercano(Point ubicacionPantalla)
        {
            if (_escena == null) return -1;

            float radioCuadrado = RadioDeteccionPx * RadioDeteccionPx;
            for (int i = 0; i < _escena.PuntosControl.Count; i++)
            {
                PointF p = MundoAPantalla(_escena.PuntosControl[i]);
                float dx = p.X - ubicacionPantalla.X;
                float dy = p.Y - ubicacionPantalla.Y;
                if ((dx * dx) + (dy * dy) <= radioCuadrado)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}