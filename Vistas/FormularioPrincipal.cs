using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using ComparadorBezierDeCasteljau.Modelos;
using ComparadorBezierDeCasteljau.Utilidades;

namespace ComparadorBezierDeCasteljau.Vistas
{
    public class FormularioPrincipal : Form, IVistaPrincipal
    {
        private LienzoCurvas _lienzo;
        private TabControl _pestanasDerecha;

        // Controles Pestaña 1: Curva y Control
        private NumericUpDown _numMuestras;
        private TrackBar _barraT;
        private Label _lblValorT;
        private Button _btnAnimarT;
        private Timer _temporizadorAnimacion;
        private CheckBox _chkPoligono;
        private CheckBox _chkBernstein;
        private CheckBox _chkDeCasteljau;
        private CheckBox _chkInterpolaciones;
        private NumericUpDown _numPuntosGenerar;
        private Button _btnGenerarPuntos;
        private Button _btnPresetCubico;
        private Button _btnLimpiarPuntos;
        private DataGridView _tablaPuntos;
        private Button _btnAgregarFilaPunto;
        private Button _btnEliminarFilaPunto;

        // Controles Pestaña 2: Benchmark
        private NumericUpDown _numRepeticiones;
        private Button _btnEjecutarActual;
        private Button _btnEjecutarMatriz;
        private Button _btnExportarCsv;
        private Button _btnLimpiarHistorial;
        private Label _lblKpiBernstein;
        private Label _lblKpiDeCasteljau;
        private Label _lblKpiErrorMax;
        private Label _lblKpiRmse;
        private Label _lblKpiEstabilidad;
        private Label _lblProgresoBenchmark;
        private ProgressBar _barraProgreso;
        private DataGridView _tablaHistorial;

        // Barra de estado
        private StatusStrip _barraEstado;
        private ToolStripStatusLabel _lblEstadoCursor;
        private ToolStripStatusLabel _lblEstadoGrado;
        private ToolStripStatusLabel _lblEstadoPuntoT;
        private ToolStripStatusLabel _lblEstadoDiferencia;

        private bool _sincronizandoTabla;
        private double _direccionAnimacion = 1.0;

        public event Action<Punto2D> PuntoAgregadoSolicitado;
        public event Action<int, Punto2D> PuntoModificadoSolicitado;
        public event Action<int> PuntoEliminadoSolicitado;
        public event Action LimpiarPuntosSolicitado;
        public event Action PresetCubicoSolicitado;
        public event Action<int> GenerarPuntosAleatoriosSolicitado;
        public event Action<int> MuestrasCambiadas;
        public event Action<double> ParametroTCambiado;
        public event Action<bool, bool, bool, bool> VisibilidadCapasCambiada;
        public event Action<Punto2D> CursorLienzoMovido;
        public event Action<int> EjecutarBenchmarkActualSolicitado;
        public event Action<int> EjecutarMatrizExperimentalSolicitado;
        public event Action ExportarCsvSolicitado;
        public event Action LimpiarHistorialBenchmarkSolicitado;

        public double AnchoUtilLienzo
        {
            get { return _lienzo.AnchoUtilMundo; }
        }

        public double AltoUtilLienzo
        {
            get { return _lienzo.AltoUtilMundo; }
        }

        public FormularioPrincipal()
        {
            InicializarComponentes();
            ConectarEventos();
        }

        private void InicializarComponentes()
        {
            Text = "Evaluador Comparativo: Bézier (Bernstein) vs. De Casteljau";
            Size = new Size(1340, 820);
            MinimumSize = new Size(1080, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = TemaVisual.FondoVentana;
            Font = TemaVisual.FuenteBase;

            // Barra de estado inferior
            _barraEstado = new StatusStrip { BackColor = Color.White, SizingGrip = false };
            _lblEstadoCursor = new ToolStripStatusLabel("Cursor: (0.00, 0.00)") { BorderSides = ToolStripStatusLabelBorderSides.Right };
            _lblEstadoGrado = new ToolStripStatusLabel("Grado: 0 | Muestras: 100") { BorderSides = ToolStripStatusLabelBorderSides.Right };
            _lblEstadoPuntoT = new ToolStripStatusLabel("B(t): --") { BorderSides = ToolStripStatusLabelBorderSides.Right };
            _lblEstadoDiferencia = new ToolStripStatusLabel("Δ máx: 0.00e+00");
            _barraEstado.Items.AddRange(new ToolStripItem[]
            {
                _lblEstadoCursor, _lblEstadoGrado, _lblEstadoPuntoT, _lblEstadoDiferencia
            });

            // Panel lateral derecho
            Panel panelDerecho = new Panel
            {
                Dock = DockStyle.Right,
                Width = 445,
                Padding = new Padding(8),
                BackColor = TemaVisual.FondoVentana
            };

            _pestanasDerecha = new TabControl { Dock = DockStyle.Fill };
            TabPage tabCurva = CrearPestanaCurvaYControl();
            TabPage tabBenchmark = CrearPestanaBenchmark();
            _pestanasDerecha.TabPages.Add(tabCurva);
            _pestanasDerecha.TabPages.Add(tabBenchmark);
            panelDerecho.Controls.Add(_pestanasDerecha);

            Splitter divisor = new Splitter
            {
                Dock = DockStyle.Right,
                Width = 4,
                BackColor = TemaVisual.BordeSuave
            };

            // Panel central (leyenda superior + lienzo)
            Panel panelCentral = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 8, 4, 8),
                BackColor = TemaVisual.FondoVentana
            };

            Panel barraLeyenda = CrearBarraLeyenda();
            Panel contenedorLienzo = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = TemaVisual.FondoLienzo
            };

            _lienzo = new LienzoCurvas { Dock = DockStyle.Fill };
            contenedorLienzo.Controls.Add(_lienzo);

            panelCentral.Controls.Add(contenedorLienzo);
            panelCentral.Controls.Add(barraLeyenda);

            // El orden de agregado con Dock importa en WinForms
            Controls.Add(panelCentral);
            Controls.Add(divisor);
            Controls.Add(panelDerecho);
            Controls.Add(_barraEstado);

            _temporizadorAnimacion = new Timer { Interval = 35 };
        }

        private Panel CrearBarraLeyenda()
        {
            Panel barra = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.White,
                Padding = new Padding(10, 8, 10, 4)
            };

            FlowLayoutPanel flujo = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            flujo.Controls.Add(CrearEtiquetaLeyenda("━━ Bernstein (Directo)", TemaVisual.CurvaBernstein));
            flujo.Controls.Add(CrearEtiquetaLeyenda("╌╌ De Casteljau", TemaVisual.CurvaDeCasteljau));
            flujo.Controls.Add(CrearEtiquetaLeyenda("◆ Punto B(t)", TemaVisual.PuntoEvaluadoT));
            flujo.Controls.Add(CrearEtiquetaLeyenda("┄┄ Polígono", TemaVisual.PoligonoControl));
            flujo.Controls.Add(new Label
            {
                Text = "  |  Clic izq: agregar · Arrastrar: mover · Clic der: eliminar",
                AutoSize = true,
                ForeColor = TemaVisual.TextoSecundario,
                Padding = new Padding(4, 2, 0, 0)
            });

            barra.Controls.Add(flujo);
            return barra;
        }

        private static Label CrearEtiquetaLeyenda(string texto, Color color)
        {
            return new Label
            {
                Text = texto,
                ForeColor = color,
                Font = TemaVisual.FuenteNegrita,
                AutoSize = true,
                Margin = new Padding(0, 2, 12, 0)
            };
        }

        private TabPage CrearPestanaCurvaYControl()
        {
            TabPage tab = new TabPage("Curva y Control")
            {
                BackColor = TemaVisual.FondoPanel,
                Padding = new Padding(10)
            };

            // 1. Grupo Parámetros de Evaluación
            GroupBox grpParametros = new GroupBox
            {
                Text = "Parámetros de evaluación",
                Dock = DockStyle.Top,
                Height = 132,
                Padding = new Padding(10, 6, 10, 6)
            };

            Label lblMuestras = new Label { Text = "Muestras:", Location = new Point(12, 26), AutoSize = true };
            _numMuestras = new NumericUpDown
            {
                Location = new Point(92, 23),
                Width = 105,
                Minimum = 10,
                Maximum = 50000,
                Increment = 50,
                Value = 100
            };

            _lblValorT = new Label
            {
                Text = "Parámetro t = 0.50",
                Location = new Point(12, 60),
                AutoSize = true,
                Font = TemaVisual.FuenteNegrita
            };

            _btnAnimarT = new Button
            {
                Text = "Animar t",
                Location = new Point(290, 54),
                Size = new Size(100, 26),
                FlatStyle = FlatStyle.System
            };

            _barraT = new TrackBar
            {
                Location = new Point(8, 84),
                Width = 385,
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                TickFrequency = 10
            };

            grpParametros.Controls.AddRange(new Control[] { lblMuestras, _numMuestras, _lblValorT, _btnAnimarT, _barraT });

            // 2. Grupo Visibilidad
            GroupBox grpCapas = new GroupBox
            {
                Text = "Visualización simultánea",
                Dock = DockStyle.Top,
                Height = 76,
                Padding = new Padding(10, 6, 10, 6)
            };

            _chkBernstein = new CheckBox { Text = "Curva Bernstein", Location = new Point(12, 22), AutoSize = true, Checked = true, ForeColor = TemaVisual.CurvaBernstein };
            _chkDeCasteljau = new CheckBox { Text = "Curva De Casteljau", Location = new Point(195, 22), AutoSize = true, Checked = true, ForeColor = TemaVisual.CurvaDeCasteljau };
            _chkPoligono = new CheckBox { Text = "Polígono de control", Location = new Point(12, 46), AutoSize = true, Checked = true };
            _chkInterpolaciones = new CheckBox { Text = "Interpolaciones De Casteljau", Location = new Point(195, 46), AutoSize = true, Checked = true };
            grpCapas.Controls.AddRange(new Control[] { _chkBernstein, _chkDeCasteljau, _chkPoligono, _chkInterpolaciones });

            // 3. Grupo Presets y Generador de Grados
            GroupBox grpPresets = new GroupBox
            {
                Text = "Escenarios rápidos y grados",
                Dock = DockStyle.Top,
                Height = 96,
                Padding = new Padding(10, 6, 10, 6)
            };

            _btnPresetCubico = new Button { Text = "Cúbica validación (4 pts)", Location = new Point(12, 22), Size = new Size(185, 28) };
            _btnLimpiarPuntos = new Button { Text = "Limpiar lienzo", Location = new Point(206, 22), Size = new Size(184, 28) };

            Label lblN = new Label { Text = "Generar N puntos:", Location = new Point(12, 62), AutoSize = true };
            _numPuntosGenerar = new NumericUpDown
            {
                Location = new Point(126, 59),
                Width = 85,
                Minimum = 2,
                Maximum = 250,
                Value = 20
            };
            _btnGenerarPuntos = new Button { Text = "Generar curva", Location = new Point(220, 57), Size = new Size(170, 27) };

            grpPresets.Controls.AddRange(new Control[]
            {
                _btnPresetCubico, _btnLimpiarPuntos, lblN, _numPuntosGenerar, _btnGenerarPuntos
            });

            // 4. Grupo Tabla editable de puntos de control
            GroupBox grpTabla = new GroupBox
            {
                Text = "Puntos de control (coordenadas cartesianas)",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            Panel panelBotonesTabla = new Panel { Dock = DockStyle.Bottom, Height = 36 };
            _btnAgregarFilaPunto = new Button { Text = "+ Agregar punto", Location = new Point(0, 6), Size = new Size(135, 26) };
            _btnEliminarFilaPunto = new Button { Text = "- Eliminar seleccionado", Location = new Point(144, 6), Size = new Size(165, 26) };
            panelBotonesTabla.Controls.AddRange(new Control[] { _btnAgregarFilaPunto, _btnEliminarFilaPunto });

            _tablaPuntos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            _tablaPuntos.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColPunto", HeaderText = "Punto", ReadOnly = true, FillWeight = 24 });
            _tablaPuntos.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColX", HeaderText = "X", FillWeight = 38 });
            _tablaPuntos.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColY", HeaderText = "Y", FillWeight = 38 });

            grpTabla.Controls.Add(_tablaPuntos);
            grpTabla.Controls.Add(panelBotonesTabla);

            tab.Controls.Add(grpTabla);
            tab.Controls.Add(grpPresets);
            tab.Controls.Add(grpCapas);
            tab.Controls.Add(grpParametros);

            return tab;
        }

        private TabPage CrearPestanaBenchmark()
        {
            TabPage tab = new TabPage("Benchmark y Comparación")
            {
                BackColor = TemaVisual.FondoPanel,
                Padding = new Padding(10)
            };

            // 1. Configuración y acciones
            GroupBox grpConfig = new GroupBox
            {
                Text = "Ejecución experimental (Stopwatch sin renderizado)",
                Dock = DockStyle.Top,
                Height = 136,
                Padding = new Padding(10, 6, 10, 6)
            };

            Label lblRep = new Label { Text = "Repeticiones (mín. 10):", Location = new Point(12, 26), AutoSize = true };
            _numRepeticiones = new NumericUpDown
            {
                Location = new Point(165, 23),
                Width = 90,
                Minimum = 10,
                Maximum = 1000,
                Value = 20
            };

            _btnEjecutarActual = new Button { Text = "Ejecutar prueba actual", Location = new Point(12, 56), Size = new Size(185, 30) };
            _btnEjecutarMatriz = new Button { Text = "Ejecutar matriz (Grados × Muestras)", Location = new Point(204, 56), Size = new Size(192, 30) };

            _barraProgreso = new ProgressBar
            {
                Location = new Point(12, 96),
                Size = new Size(384, 12),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            _lblProgresoBenchmark = new Label
            {
                Text = "Listo para medir rendimiento y estabilidad.",
                Location = new Point(12, 112),
                AutoSize = true,
                ForeColor = TemaVisual.TextoSecundario
            };

            grpConfig.Controls.AddRange(new Control[]
            {
                lblRep, _numRepeticiones, _btnEjecutarActual, _btnEjecutarMatriz, _barraProgreso, _lblProgresoBenchmark
            });

            // 2. Resumen KPI
            GroupBox grpKpi = new GroupBox
            {
                Text = "Último resultado",
                Dock = DockStyle.Top,
                Height = 140,
                Padding = new Padding(10, 6, 10, 6)
            };

            _lblKpiBernstein = new Label { Text = "Bernstein promedio: -- ms", Location = new Point(12, 22), AutoSize = true, Font = TemaVisual.FuenteNegrita, ForeColor = TemaVisual.CurvaBernstein };
            _lblKpiDeCasteljau = new Label { Text = "De Casteljau promedio: -- ms", Location = new Point(12, 44), AutoSize = true, Font = TemaVisual.FuenteNegrita, ForeColor = TemaVisual.CurvaDeCasteljau };
            _lblKpiErrorMax = new Label { Text = "Error máximo (Δ máx): --", Location = new Point(12, 68), AutoSize = true };
            _lblKpiRmse = new Label { Text = "RMSE: --", Location = new Point(12, 90), AutoSize = true };
            _lblKpiEstabilidad = new Label { Text = "Estabilidad: --", Location = new Point(12, 112), AutoSize = true, Font = TemaVisual.FuenteNegrita };

            grpKpi.Controls.AddRange(new Control[]
            {
                _lblKpiBernstein, _lblKpiDeCasteljau, _lblKpiErrorMax, _lblKpiRmse, _lblKpiEstabilidad
            });

            // 3. Historial y exportación
            GroupBox grpHistorial = new GroupBox
            {
                Text = "Historial de pruebas (para informe)",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            Panel panelAccionesCsv = new Panel { Dock = DockStyle.Bottom, Height = 36 };
            _btnExportarCsv = new Button { Text = "Exportar CSV...", Location = new Point(0, 6), Size = new Size(140, 26) };
            _btnLimpiarHistorial = new Button { Text = "Limpiar historial", Location = new Point(148, 6), Size = new Size(135, 26) };
            panelAccionesCsv.Controls.AddRange(new Control[] { _btnExportarCsv, _btnLimpiarHistorial });

            _tablaHistorial = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
            };

            _tablaHistorial.Columns.Add("Grado", "Grado");
            _tablaHistorial.Columns.Add("Muestras", "Muestras");
            _tablaHistorial.Columns.Add("Reps", "Reps");
            _tablaHistorial.Columns.Add("BernsteinMs", "Bernstein (ms)");
            _tablaHistorial.Columns.Add("DeCasteljauMs", "De Casteljau (ms)");
            _tablaHistorial.Columns.Add("Ratio", "DC / B");
            _tablaHistorial.Columns.Add("ErrorMax", "Δ máx");
            _tablaHistorial.Columns.Add("Estabilidad", "Estabilidad");

            grpHistorial.Controls.Add(_tablaHistorial);
            grpHistorial.Controls.Add(panelAccionesCsv);

            tab.Controls.Add(grpHistorial);
            tab.Controls.Add(grpKpi);
            tab.Controls.Add(grpConfig);

            return tab;
        }

        private void ConectarEventos()
        {
            _lienzo.PuntoAgregadoEnLienzo += p => PuntoAgregadoSolicitado?.Invoke(p);
            _lienzo.PuntoMovidoEnLienzo += (idx, p) => PuntoModificadoSolicitado?.Invoke(idx, p);
            _lienzo.PuntoEliminadoEnLienzo += idx => PuntoEliminadoSolicitado?.Invoke(idx);
            _lienzo.CursorMovidoEnLienzo += p => CursorLienzoMovido?.Invoke(p);

            _numMuestras.ValueChanged += (s, e) => MuestrasCambiadas?.Invoke((int)_numMuestras.Value);

            _barraT.Scroll += (s, e) =>
            {
                double t = _barraT.Value / 100.0;
                _lblValorT.Text = string.Format(CultureInfo.InvariantCulture, "Parámetro t = {0:F2}", t);
                ParametroTCambiado?.Invoke(t);
            };

            _btnAnimarT.Click += (s, e) =>
            {
                if (_temporizadorAnimacion.Enabled)
                {
                    _temporizadorAnimacion.Stop();
                    _btnAnimarT.Text = "Animar t";
                }
                else
                {
                    _temporizadorAnimacion.Start();
                    _btnAnimarT.Text = "Pausar";
                }
            };

            _temporizadorAnimacion.Tick += (s, e) =>
            {
                int siguiente = _barraT.Value + (int)(2 * _direccionAnimacion);
                if (siguiente >= 100)
                {
                    siguiente = 100;
                    _direccionAnimacion = -1.0;
                }
                else if (siguiente <= 0)
                {
                    siguiente = 0;
                    _direccionAnimacion = 1.0;
                }

                _barraT.Value = siguiente;
                double t = siguiente / 100.0;
                _lblValorT.Text = string.Format(CultureInfo.InvariantCulture, "Parámetro t = {0:F2}", t);
                ParametroTCambiado?.Invoke(t);
            };

            EventHandler alCambiarCapa = (s, e) =>
            {
                VisibilidadCapasCambiada?.Invoke(
                    _chkPoligono.Checked,
                    _chkBernstein.Checked,
                    _chkDeCasteljau.Checked,
                    _chkInterpolaciones.Checked);
            };

            _chkPoligono.CheckedChanged += alCambiarCapa;
            _chkBernstein.CheckedChanged += alCambiarCapa;
            _chkDeCasteljau.CheckedChanged += alCambiarCapa;
            _chkInterpolaciones.CheckedChanged += alCambiarCapa;

            _btnPresetCubico.Click += (s, e) => PresetCubicoSolicitado?.Invoke();
            _btnLimpiarPuntos.Click += (s, e) => LimpiarPuntosSolicitado?.Invoke();
            _btnGenerarPuntos.Click += (s, e) => GenerarPuntosAleatoriosSolicitado?.Invoke((int)_numPuntosGenerar.Value);

            _btnAgregarFilaPunto.Click += (s, e) =>
            {
                double x = Math.Round(AnchoUtilLienzo * 0.5, 2);
                double y = Math.Round(AltoUtilLienzo * 0.5, 2);
                PuntoAgregadoSolicitado?.Invoke(new Punto2D(x, y));
            };

            _btnEliminarFilaPunto.Click += (s, e) =>
            {
                if (_tablaPuntos.SelectedRows.Count > 0)
                {
                    int idx = _tablaPuntos.SelectedRows[0].Index;
                    PuntoEliminadoSolicitado?.Invoke(idx);
                }
            };

            _tablaPuntos.CellEndEdit += (s, e) =>
            {
                if (_sincronizandoTabla || e.RowIndex < 0) return;

                string textoX = Convert.ToString(_tablaPuntos.Rows[e.RowIndex].Cells["ColX"].Value);
                string textoY = Convert.ToString(_tablaPuntos.Rows[e.RowIndex].Cells["ColY"].Value);

                double x, y;
                if (IntentarParsearNumero(textoX, out x) && IntentarParsearNumero(textoY, out y))
                {
                    PuntoModificadoSolicitado?.Invoke(e.RowIndex, new Punto2D(x, y));
                }
                else
                {
                    MostrarMensaje("Valor inválido", "Ingresa valores numéricos válidos para las coordenadas X e Y.", true);
                    // Fuerza resincronización desde el controlador mediante evento no destructivo
                    ParametroTCambiado?.Invoke(_barraT.Value / 100.0);
                }
            };

            _btnEjecutarActual.Click += (s, e) => EjecutarBenchmarkActualSolicitado?.Invoke((int)_numRepeticiones.Value);
            _btnEjecutarMatriz.Click += (s, e) => EjecutarMatrizExperimentalSolicitado?.Invoke((int)_numRepeticiones.Value);
            _btnExportarCsv.Click += (s, e) => ExportarCsvSolicitado?.Invoke();
            _btnLimpiarHistorial.Click += (s, e) => LimpiarHistorialBenchmarkSolicitado?.Invoke();
        }

        private static bool IntentarParsearNumero(string texto, out double valor)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                valor = 0.0;
                return false;
            }

            string normalizado = texto.Trim().Replace(',', '.');
            return double.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out valor);
        }

        public void RenderizarEscena(EstadoEscena escena)
        {
            _lienzo.EstablecerEscena(escena);
            ActualizarBarraEstado(new Punto2D(0, 0), escena);
        }

        public void ActualizarTablaPuntos(IReadOnlyList<Punto2D> puntos)
        {
            _sincronizandoTabla = true;
            try
            {
                int indiceSeleccionado = _tablaPuntos.SelectedRows.Count > 0
                    ? _tablaPuntos.SelectedRows[0].Index
                    : -1;

                _tablaPuntos.Rows.Clear();
                for (int i = 0; i < puntos.Count; i++)
                {
                    _tablaPuntos.Rows.Add(
                        "P" + i,
                        puntos[i].X.ToString("F2", CultureInfo.InvariantCulture),
                        puntos[i].Y.ToString("F2", CultureInfo.InvariantCulture));
                }

                if (indiceSeleccionado >= 0 && indiceSeleccionado < _tablaPuntos.Rows.Count)
                {
                    _tablaPuntos.Rows[indiceSeleccionado].Selected = true;
                }
            }
            finally
            {
                _sincronizandoTabla = false;
            }
        }

        public void ActualizarBarraEstado(Punto2D cursorMundo, EstadoEscena escena)
        {
            _lblEstadoCursor.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Cursor: ({0:F1}, {1:F1})",
                cursorMundo.X,
                cursorMundo.Y);

            if (escena == null) return;

            _lblEstadoGrado.Text = string.Format(
                "Puntos: {0} | Grado n = {1} | Muestras: {2}",
                escena.PuntosControl.Count,
                escena.Grado,
                escena.Muestras);

            if (escena.NivelesDeCasteljau != null && escena.NivelesDeCasteljau.Count > 0)
            {
                NivelInterpolacion ultimo = escena.NivelesDeCasteljau[escena.NivelesDeCasteljau.Count - 1];
                if (ultimo.Puntos.Count == 1)
                {
                    _lblEstadoPuntoT.Text = string.Format(
                        CultureInfo.InvariantCulture,
                        "B({0:F2}) = {1}",
                        escena.ParametroT,
                        ultimo.Puntos[0]);
                }
            }
            else
            {
                _lblEstadoPuntoT.Text = "B(t): --";
            }

            MetricasDiferencia m = escena.UltimasMetricasVisuales;
            if (m != null)
            {
                _lblEstadoDiferencia.Text = m.PuntosInvalidosBernstein > 0
                    ? string.Format("Bernstein NaN/Inf: {0} pts ({1})", m.PuntosInvalidosBernstein, m.EstadoEstabilidad)
                    : string.Format(CultureInfo.InvariantCulture, "Δ máx: {0:E3} | {1}", m.ErrorMaximo, m.EstadoEstabilidad);
            }
        }

        public void MostrarResultadoBenchmark(ResultadoBenchmark r)
        {
            if (r == null) return;

            CultureInfo inv = CultureInfo.InvariantCulture;
            _lblKpiBernstein.Text = string.Format(
                inv,
                "Bernstein promedio: {0:F4} ms  (mín {1:F4} / máx {2:F4})",
                r.TiempoPromedioBernsteinMs,
                r.TiempoMinBernsteinMs,
                r.TiempoMaxBernsteinMs);

            _lblKpiDeCasteljau.Text = string.Format(
                inv,
                "De Casteljau promedio: {0:F4} ms  (mín {1:F4} / máx {2:F4})",
                r.TiempoPromedioDeCasteljauMs,
                r.TiempoMinDeCasteljauMs,
                r.TiempoMaxDeCasteljauMs);

            MetricasDiferencia d = r.Diferencias;
            _lblKpiErrorMax.Text = string.Format(
                inv,
                "Error máximo (Δ máx): {0:E4}  |  Promedio: {1:E4}",
                d.ErrorMaximo,
                d.ErrorPromedio);

            _lblKpiRmse.Text = string.Format(
                inv,
                "RMSE: {0:E4}  |  Inválidos (B/DC): {1}/{2}",
                d.Rmse,
                d.PuntosInvalidosBernstein,
                d.PuntosInvalidosDeCasteljau);

            _lblKpiEstabilidad.Text = "Estabilidad: " + d.EstadoEstabilidad;
            if (d.PuntosInvalidosBernstein > 0 || d.PuntosInvalidosDeCasteljau > 0)
            {
                _lblKpiEstabilidad.ForeColor = TemaVisual.EstadoError;
            }
            else if (d.ErrorMaximo > 1e-6)
            {
                _lblKpiEstabilidad.ForeColor = TemaVisual.EstadoAlerta;
            }
            else
            {
                _lblKpiEstabilidad.ForeColor = TemaVisual.EstadoOk;
            }
        }

        public void ActualizarHistorialBenchmark(IReadOnlyList<ResultadoBenchmark> historial)
        {
            _tablaHistorial.Rows.Clear();
            CultureInfo inv = CultureInfo.InvariantCulture;

            foreach (ResultadoBenchmark r in historial)
            {
                _tablaHistorial.Rows.Add(
                    r.Grado,
                    r.Muestras,
                    r.Repeticiones,
                    r.TiempoPromedioBernsteinMs.ToString("F4", inv),
                    r.TiempoPromedioDeCasteljauMs.ToString("F4", inv),
                    r.RelacionTiempoDcSobreB.ToString("F2", inv) + "x",
                    r.Diferencias.PuntosInvalidosBernstein > 0
                        ? "Overflow (" + r.Diferencias.PuntosInvalidosBernstein + " NaN)"
                        : r.Diferencias.ErrorMaximo.ToString("E3", inv),
                    r.Diferencias.EstadoEstabilidad);
            }
        }

        public void EstablecerEstadoEjecucionBenchmark(bool ejecutando, string mensajeProgreso)
        {
            _btnEjecutarActual.Enabled = !ejecutando;
            _btnEjecutarMatriz.Enabled = !ejecutando;
            _numRepeticiones.Enabled = !ejecutando;
            _barraProgreso.Visible = ejecutando;
            _lblProgresoBenchmark.Text = mensajeProgreso;
        }

        public void MostrarMensaje(string titulo, string mensaje, bool esError)
        {
            MessageBox.Show(
                this,
                mensaje,
                titulo,
                MessageBoxButtons.OK,
                esError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        public string SolicitarRutaGuardadoCsv()
        {
            using (SaveFileDialog dialogo = new SaveFileDialog())
            {
                dialogo.Title = "Exportar resultados de benchmark";
                dialogo.Filter = "Archivo CSV (*.csv)|*.csv";
                dialogo.FileName = "benchmark_bezier_vs_decasteljau.csv";
                return dialogo.ShowDialog(this) == DialogResult.OK ? dialogo.FileName : null;
            }
        }
    }
}