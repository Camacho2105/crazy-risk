using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class GameForm : Form
    {
        // Eventos (solo UI)
        public event Action? StartGameRequested;
        public event Action? EndTurnRequested;
        public event Action<int, int>? PlaceRequested;
        public event Action<int, int, int, int>? AttackRequested;
        public event Action<int, int, int>? FortifyRequested;
        public event Action? ExchangeCardsRequested;
        public event Action? ViewCardsRequested;
        public event Action? AutoPlaceRequested;
        public event Action? SwapSelectionRequested;
        public event Action? InitiativeRequested;
        public event Action<bool>? NeutralAutoChanged;
        public event Action<int>? TerritorySelected;

        private const int HUD_WIDTH = 420;

        private readonly SplitContainer _split;
        private readonly MapCanvas _canvas;

        // Estado UI
        private IReadOnlyList<PlayerView> _players = Array.Empty<PlayerView>();
        private IReadOnlyList<TerritoryView> _territories = Array.Empty<TerritoryView>();
        private int _currentPlayerId = -1;
        private Phase _phase = Phase.Lobby;
        private int _reinforcements = 0;
        private int _globalTradeCounter = 2;

        // Selecciones
        public int? SelectionA
        {
            get => _canvas.SelectionA;
            set 
            { 
                _canvas.SelectionA = value; 
                UpdateSelectionLabels(); 
                UpdateButtonsEnabled(); 
                _canvas.Invalidate(); 
            }
        }
        public int? SelectionB
        {
            get => _canvas.SelectionB;
            set 
            { 
                _canvas.SelectionB = value; 
                UpdateSelectionLabels(); 
                UpdateButtonsEnabled(); 
                _canvas.Invalidate(); 
            }
        }

        // HUD controls
        private readonly Label _lblPhase = new();
        private readonly Label _lblPlayer = new();
        private readonly Label _lblReinf = new();
        private readonly Label _lblSelA = new();
        private readonly Label _lblSelB = new();

        private readonly Button _btnStartGame = new() { Text = "Iniciar partida (reparto aleatorio)" };

        private readonly NumericUpDown _nudPlace = new() { Minimum = 1, Maximum = 999, Value = 1, Width = 120 };
        private readonly Button _btnPlace = new() { Text = "Colocar en A" };
        private readonly Button _btnAutoPlace = new() { Text = "Auto (dispersar)" };

        private readonly Button _btnInitiative = new() { Text = "Tirar iniciativa (quién ataca)" };
        private readonly Label _lblInitiative = new() { AutoSize = true, ForeColor = Color.DimGray };

        private readonly NumericUpDown _nudAtkDice = new() { Minimum = 1, Maximum = 3, Value = 3, Width = 60 };
        private readonly NumericUpDown _nudDefDice = new() { Minimum = 1, Maximum = 2, Value = 2, Width = 60 };
        private readonly Label _lblAtkLimit = new() { AutoSize = true, ForeColor = Color.DimGray };
        private readonly Label _lblDefLimit = new() { AutoSize = true, ForeColor = Color.DimGray };
        private readonly Button _btnAttack = new() { Text = "Atacar A → B" };

        private readonly NumericUpDown _nudMove = new() { Minimum = 1, Maximum = 999, Value = 1, Width = 120 };
        private readonly Button _btnFortify = new() { Text = "Mover A → B" };

        private readonly Label _lblCards = new();
        private readonly Label _lblGlobal = new();
        private readonly Button _btnExchange = new() { Text = "Intercambiar (trío)" };
        private readonly Button _btnViewCards = new() { Text = "Ver mis tarjetas" };

        private readonly Button _btnEndTurn = new() { Text = "Fin de turno" };
        private readonly Button _btnClearSel = new() { Text = "Limpiar selección A/B" };
        private readonly Button _btnSwapSel = new() { Text = "Intercambiar A ↔ B" };

        private readonly CheckBox _chkNeutralAuto = new() { Text = "Neutral automático", Checked = true };

        private readonly DicePanel _dicePanel = new();
        private readonly PlayerLegend _legend = new();

        public GameForm()
        {
            Text = "CrazyRisk – Partida (UI)";
            MinimumSize = new Size(1360, 880);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10f, FontStyle.Regular);

            KeyPreview = true;
            KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel2,
                SplitterWidth = 6
            };
            Controls.Add(_split);

            _canvas = new MapCanvas { Dock = DockStyle.Fill };

            // Click izquierdo: Seleccionar A
            _canvas.TerritoryClicked += id =>
            {
                Console.WriteLine($"🖱️ Territorio clickeado (IZQ): {id}");
                SelectionA = id;
                SelectionB = null;
                TerritorySelected?.Invoke(id);
                UpdateButtonsEnabled();
            };

            // Click derecho: Seleccionar B
            _canvas.TerritoryClickedRight += id =>
            {
                Console.WriteLine($"🖱️ Territorio clickeado (DER): {id}");
                
                if (!SelectionA.HasValue)
                {
                    SelectionA = id;
                }
                else
                {
                    SelectionB = id;
                }
                
                TerritorySelected?.Invoke(id);
                UpdateButtonsEnabled();
            };

            _split.Panel1.Controls.Add(_canvas);
            var hud = BuildHud(HUD_WIDTH);
            _split.Panel2.Controls.Add(hud);

            // Eventos (solo UI)
            _btnStartGame.Click += (s, e) => StartGameRequested?.Invoke();
            _btnEndTurn.Click += (s, e) => EndTurnRequested?.Invoke();
            _btnPlace.Click += (s, e) => { if (SelectionA is int a) PlaceRequested?.Invoke(a, (int)_nudPlace.Value); };
            _btnAutoPlace.Click += (s, e) => AutoPlaceRequested?.Invoke();
            _btnInitiative.Click += (s, e) => { ShowInitiativeRolling(); InitiativeRequested?.Invoke(); };
            _btnAttack.Click += (s, e) =>
            {
                if (SelectionA is int from && SelectionB is int to)
                {
                    _dicePanel.StartRolling();
                    AttackRequested?.Invoke(from, to, (int)_nudAtkDice.Value, (int)_nudDefDice.Value);
                }
                else MessageBox.Show(this, "Selecciona origen (A) y destino (B).", "Ataque",
                                     MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            _btnFortify.Click += (s, e) =>
            {
                if (SelectionA is int from && SelectionB is int to)
                    FortifyRequested?.Invoke(from, to, (int)_nudMove.Value);
            };
            _btnClearSel.Click += (s, e) => { SelectionA = null; SelectionB = null; };
            _btnSwapSel.Click += (s, e) => { (SelectionA, SelectionB) = (SelectionB, SelectionA); SwapSelectionRequested?.Invoke(); };
            _btnExchange.Click += (s, e) => ExchangeCardsRequested?.Invoke();
            _btnViewCards.Click += (s, e) => ViewCardsRequested?.Invoke();
            _chkNeutralAuto.CheckedChanged += (s, e) => NeutralAutoChanged?.Invoke(_chkNeutralAuto.Checked);

            // Mapa base (UI)
            var layout = MapGenerator.GenerateRisk42WithBridges();
            _canvas.LoadShapes(layout.Shapes, layout.Bridges);

            Shown += (_, __) => ApplySplitLayout();
            Resize += (_, __) => ApplySplitLayout();

            _legend.BindPlayers(_players);
            UpdateCardsLabel();
            UpdateLabels();
            UpdateSelectionLabels();
            UpdateButtonsEnabled();
        }

        // ====== API pública (visual)
        public void ShowInitiativeRolling() => _dicePanel.StartRolling();
        public void ShowInitiativeResult(int attackerPlayerId, int atkDie, int defDie)
        {
            var who = _players.FirstOrDefault(p => p.Id == attackerPlayerId)?.Alias ?? $"Jugador {attackerPlayerId}";
            _lblInitiative.Text = $"Ataca: {who}  ({atkDie} vs {defDie})";
            _dicePanel.ShowFinal(new[] { atkDie }, new[] { defDie });
        }
        public void ShowDiceRolling() => _dicePanel.StartRolling();
        public void ShowDiceResult(int[] atk, int[] def) => _dicePanel.ShowFinal(atk ?? Array.Empty<int>(), def ?? Array.Empty<int>());

        // ====== API para refrescar UI
        public void SetPlayers(IReadOnlyList<PlayerView> players)
        {
            _players = players ?? Array.Empty<PlayerView>();
            _legend.BindPlayers(_players);
            UpdateCardsLabel();
            UpdateLabels();
        }
        public void SetTerritories(IReadOnlyList<TerritoryView> territories)
        {
            _territories = territories ?? Array.Empty<TerritoryView>();
            _canvas.BindState(_players, _territories, _currentPlayerId, _phase);
            UpdateDiceLimits();
        }
        public void SetPhase(Phase phase)
        {
            _phase = phase; UpdateLabels();
            _canvas.BindState(_players, _territories, _currentPlayerId, _phase);
            UpdateButtonsEnabled();
        }
        public void SetCurrentPlayer(int playerId)
        {
            _currentPlayerId = playerId; UpdateLabels(); UpdateCardsLabel();
            _canvas.BindState(_players, _territories, _currentPlayerId, _phase);
            UpdateButtonsEnabled();
        }
        public void SetReinforcements(int remaining)
        {
            Console.WriteLine($"🔄 SetReinforcements llamado: {_reinforcements} → {remaining}");
            _reinforcements = remaining;
            UpdateLabels();
            UpdateButtonsEnabled();
        }
        public void SetGlobalTradeCounter(int value) { _globalTradeCounter = value; UpdateCardsLabel(); }

        // ====== HUD ======
        private Panel BuildHud(int HUD_WIDTH)
        {
            var hud = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10), BackColor = Color.White };
            var stack = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Padding = new Padding(0), Margin = new Padding(0) };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            hud.Controls.Add(stack);

            GroupBox NewBox(string text) => new GroupBox { Text = text, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12), Margin = new Padding(0, 8, 0, 8), Font = new Font(Font, FontStyle.Bold) };

            var boxState = NewBox("Estado");
            var tblState = NewTable(2);
            _lblPlayer.AutoSize = true; _lblPlayer.Font = new Font(Font, FontStyle.Bold);
            _lblPhase.AutoSize = true; _lblReinf.AutoSize = true;
            tblState.Controls.Add(_lblPlayer, 0, 0); tblState.SetColumnSpan(_lblPlayer, 2);
            tblState.Controls.Add(new Label { Text = "Fase:", AutoSize = true, Margin = new Padding(0, 6, 6, 0) }, 0, 1);
            tblState.Controls.Add(_lblPhase, 1, 1);
            tblState.Controls.Add(new Label { Text = "Refuerzos:", AutoSize = true, Margin = new Padding(0, 6, 6, 0) }, 0, 2);
            tblState.Controls.Add(_lblReinf, 1, 2);
            boxState.Controls.Add(tblState);
            stack.Controls.Add(boxState);

            var boxGame = NewBox("Partida");
            ApplyButtonStyle(_btnStartGame, primary: true);
            boxGame.Controls.Add(ButtonsColumn(_btnStartGame));
            stack.Controls.Add(boxGame);

            var boxSel = NewBox("Selección");
            var tblSel = NewTable(2);
            _lblSelA.AutoSize = true; _lblSelB.AutoSize = true;
            tblSel.Controls.Add(new Label { Text = "A (origen):", AutoSize = true }, 0, 0);
            tblSel.Controls.Add(_lblSelA, 1, 0);
            tblSel.Controls.Add(new Label { Text = "B (destino):", AutoSize = true, Margin = new Padding(0, 6, 0, 0) }, 0, 1);
            tblSel.Controls.Add(_lblSelB, 1, 1);
            boxSel.Controls.Add(tblSel);
            boxSel.Controls.Add(ButtonsColumn(_btnClearSel, _btnSwapSel));
            stack.Controls.Add(boxSel);

            var boxReinforce = NewBox("Refuerzos");
            var tblR = NewTable(2);
            tblR.Controls.Add(new Label { Text = "Cantidad", AutoSize = true }, 0, 0);
            tblR.Controls.Add(_nudPlace, 1, 0);
            ApplyButtonStyle(_btnPlace);
            ApplyButtonStyle(_btnAutoPlace);
            boxReinforce.Controls.Add(tblR);
            boxReinforce.Controls.Add(ButtonsColumn(_btnPlace, _btnAutoPlace));
            stack.Controls.Add(boxReinforce);

            var boxIni = NewBox("Iniciativa (quién ataca)");
            ApplyButtonStyle(_btnInitiative);
            var tblIni = NewTable(1);
            tblIni.Controls.Add(_lblInitiative, 0, 0);
            boxIni.Controls.Add(tblIni);
            boxIni.Controls.Add(ButtonsColumn(_btnInitiative));
            stack.Controls.Add(boxIni);

            var boxAtk = NewBox("Ataques (dados)");
            var tblA = NewTable(6);
            tblA.Controls.Add(new Label { Text = "ATK (1–3):", AutoSize = true }, 0, 0);
            tblA.Controls.Add(_nudAtkDice, 1, 0);
            tblA.Controls.Add(_lblAtkLimit, 2, 0);
            tblA.Controls.Add(new Label { Text = "DEF (1–2):", AutoSize = true }, 3, 0);
            tblA.Controls.Add(_nudDefDice, 4, 0);
            tblA.Controls.Add(_lblDefLimit, 5, 0);
            ApplyButtonStyle(_btnAttack);
            boxAtk.Controls.Add(tblA);
            boxAtk.Controls.Add(ButtonsColumn(_btnAttack));
            stack.Controls.Add(boxAtk);

            var boxPlan = NewBox("Planeación");
            var tblP = NewTable(2);
            tblP.Controls.Add(new Label { Text = "Cantidad a mover", AutoSize = true }, 0, 0);
            tblP.Controls.Add(_nudMove, 1, 0);
            ApplyButtonStyle(_btnFortify);
            boxPlan.Controls.Add(tblP);
            boxPlan.Controls.Add(ButtonsColumn(_btnFortify));
            stack.Controls.Add(boxPlan);

            var boxCards = NewBox("Tarjetas / Contador global");
            var tblC = NewTable(2);
            _lblCards.AutoSize = true; _lblGlobal.AutoSize = true;
            tblC.Controls.Add(new Label { Text = "Mis tarjetas:", AutoSize = true }, 0, 0);
            tblC.Controls.Add(_lblCards, 1, 0);
            tblC.Controls.Add(new Label { Text = "Contador global:", AutoSize = true }, 0, 1);
            tblC.Controls.Add(_lblGlobal, 1, 1);
            ApplyButtonStyle(_btnExchange);
            ApplyButtonStyle(_btnViewCards);
            boxCards.Controls.Add(tblC);
            boxCards.Controls.Add(ButtonsColumn(_btnExchange, _btnViewCards));
            stack.Controls.Add(boxCards);

            var boxTurn = NewBox("Turno");
            ApplyButtonStyle(_btnEndTurn, primary: true);
            boxTurn.Controls.Add(ButtonsColumn(_btnEndTurn));
            stack.Controls.Add(boxTurn);

            var boxNeutral = NewBox("Neutral");
            boxNeutral.Controls.Add(_chkNeutralAuto);
            stack.Controls.Add(boxNeutral);

            var boxDice = NewBox("Dados (visual)");
            _dicePanel.Width = HUD_WIDTH - 36;
            _dicePanel.Margin = new Padding(8);
            boxDice.Controls.Add(_dicePanel);
            stack.Controls.Add(boxDice);

            var boxLegend = NewBox("Jugadores");
            _legend.Width = HUD_WIDTH - 36;
            _legend.Margin = new Padding(8);
            boxLegend.Controls.Add(_legend);
            stack.Controls.Add(boxLegend);

            return hud;
        }

        private static TableLayoutPanel NewTable(int cols)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = cols, Margin = new Padding(6), Padding = new Padding(0) };
            for (int i = 0; i < cols; i++)
                t.ColumnStyles.Add(new ColumnStyle(cols == 1 ? SizeType.Percent : SizeType.AutoSize, cols == 1 ? 100f : 0f));
            return t;
        }
        private static TableLayoutPanel ButtonsColumn(params Button[] buttons)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Margin = new Padding(6, 8, 6, 6), Padding = new Padding(0) };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            foreach (var b in buttons) { ApplyButtonStyle(b); AddButtonRow(t, b); }
            return t;
        }
        private static void AddButtonRow(TableLayoutPanel panel, Button btn)
        {
            int r = panel.RowCount;
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            btn.Dock = DockStyle.Fill;
            panel.Controls.Add(btn, 0, r);
            panel.RowCount = r + 1;
        }
        private static void ApplyButtonStyle(Button b, bool primary = false)
        {
            b.AutoSize = false; b.Height = 44; b.Margin = new Padding(0, 6, 0, 0);
            b.Font = new Font("Segoe UI", 10.5f, primary ? FontStyle.Bold : FontStyle.Regular);
            b.FlatStyle = FlatStyle.Standard; b.UseVisualStyleBackColor = true; b.AutoEllipsis = true;
        }

        private void ApplySplitLayout()
        {
            if (!_split.IsHandleCreated || _split.Width <= 0) return;
            try
            {
                int total = _split.Width, splitter = _split.SplitterWidth;
                int minP2 = Math.Min(HUD_WIDTH, Math.Max(0, total - 100));
                int minP1 = Math.Min(600, Math.Max(0, total - minP2 - splitter));
                _split.Panel1MinSize = minP1;
                _split.Panel2MinSize = minP2;

                int desired = Math.Max(minP1, total - minP2 - splitter);
                int max = Math.Max(0, total - _split.Panel2MinSize - splitter);
                desired = Math.Min(desired, max);
                desired = Math.Max(desired, minP1);
                _split.SplitterDistance = desired;
            }
            catch { }
        }

        private void UpdateDiceLimits()
        {
            var terrA = _territories.FirstOrDefault(t => t.Id == SelectionA);
            var terrB = _territories.FirstOrDefault(t => t.Id == SelectionB);
            
            int troopsA = terrA?.Troops ?? 0;
            int troopsB = terrB?.Troops ?? 0;

            // Para ataque: máximo de dados = mínimo(3, tropasA - 1)
            int atkMax = (troopsA >= 2) ? Math.Min(3, troopsA - 1) : 0;
            
            // Para defensa: máximo de dados = mínimo(2, tropasB)
            int defMax = (troopsB >= 1) ? Math.Min(2, troopsB) : 0;

            _lblAtkLimit.Text = (atkMax > 0) ? $"máx {atkMax}" : "—";
            _lblDefLimit.Text = (defMax > 0) ? $"máx {defMax}" : "—";

            _nudAtkDice.Enabled = atkMax > 0;
            _nudDefDice.Enabled = defMax > 0;

            _nudAtkDice.Maximum = Math.Max(1, atkMax);
            _nudDefDice.Maximum = Math.Max(1, defMax);

            if (_nudAtkDice.Value > _nudAtkDice.Maximum) _nudAtkDice.Value = _nudAtkDice.Maximum;
            if (_nudDefDice.Value > _nudDefDice.Maximum) _nudDefDice.Value = _nudDefDice.Maximum;

            // Solo habilitar ataque si es fase de ataques y los territorios son válidos
            bool canAttack = (_phase == Phase.Attacks) &&
                            SelectionA.HasValue && 
                            SelectionB.HasValue &&
                            terrA?.OwnerId == _currentPlayerId &&
                            terrB?.OwnerId != _currentPlayerId &&
                            atkMax > 0 && 
                            defMax > 0;
            
            _btnAttack.Enabled = canAttack;
        }

        private void UpdateButtonsEnabled()
        {
            Console.WriteLine($"\n🔍 DEBUG UpdateButtonsEnabled:");
            Console.WriteLine($"  _phase = {_phase}");
            Console.WriteLine($"  SelectionA = {SelectionA}");
            Console.WriteLine($"  SelectionB = {SelectionB}");
            Console.WriteLine($"  _reinforcements = {_reinforcements}");
            Console.WriteLine($"  _currentPlayerId = {_currentPlayerId}");
            
            // Obtener información de los territorios seleccionados
            var terrA = _territories.FirstOrDefault(t => t.Id == SelectionA);
            var terrB = _territories.FirstOrDefault(t => t.Id == SelectionB);
            
            Console.WriteLine($"  TerrA: {terrA?.Name} (Owner: {terrA?.OwnerId})");
            Console.WriteLine($"  TerrB: {terrB?.Name} (Owner: {terrB?.OwnerId})");
            
            _btnStartGame.Enabled = (_phase == Phase.Lobby || _phase == Phase.Setup);
            
            // Botón Colocar
            bool placeEnabled = (_phase == Phase.Reinforcements) && 
                            SelectionA.HasValue && 
                            _reinforcements > 0 &&
                            terrA?.OwnerId == _currentPlayerId;
            Console.WriteLine($"  _btnPlace.Enabled = {placeEnabled}");
            _btnPlace.Enabled = placeEnabled;
            
            // Botón Auto-colocar
            _btnAutoPlace.Enabled = (_phase == Phase.Reinforcements) && _reinforcements > 0;
            Console.WriteLine($"  _btnAutoPlace.Enabled = {_btnAutoPlace.Enabled}");
            
            // Botón Atacar
            bool attackEnabled = (_phase == Phase.Attacks) && 
                                SelectionA.HasValue && 
                                SelectionB.HasValue &&
                                terrA?.OwnerId == _currentPlayerId &&
                                terrB?.OwnerId != _currentPlayerId &&
                                terrA?.OwnerId != terrB?.OwnerId;
            _btnAttack.Enabled = attackEnabled;
            Console.WriteLine($"  _btnAttack.Enabled = {attackEnabled}");
            
            // Botón Mover (Fortificar)
            bool fortifyEnabled = (_phase == Phase.Fortify) && 
                                SelectionA.HasValue && 
                                SelectionB.HasValue &&
                                terrA?.OwnerId == _currentPlayerId &&
                                terrB?.OwnerId == _currentPlayerId &&
                                terrA?.Id != terrB?.Id;
            _btnFortify.Enabled = fortifyEnabled;
            Console.WriteLine($"  _btnFortify.Enabled = {fortifyEnabled}");
            
            _btnEndTurn.Enabled = (_phase != Phase.Lobby && _phase != Phase.GameOver);
            _btnInitiative.Enabled = (_phase == Phase.Attacks);
            
            UpdateDiceLimits();
        }

        private void UpdateLabels()
        {
            var cur = _players.FirstOrDefault(p => p.Id == _currentPlayerId);
            _lblPlayer.Text = cur is null ? "En lobby / sin turno" : $"Turno de: {cur.Alias}";
            _lblPhase.Text = $"{_phase}";
            _lblReinf.Text = $"{_reinforcements}";
        }
        private void UpdateCardsLabel()
        {
            var cur = _players.FirstOrDefault(p => p.Id == _currentPlayerId);
            _lblCards.Text = cur is null ? "—" : $"{cur.CardsCount} (máx. 5)";
            _lblGlobal.Text = _globalTradeCounter.ToString();
        }
        private void UpdateSelectionLabels()
        {
            _lblSelA.Text = SelectionA?.ToString() ?? "—";
            _lblSelB.Text = SelectionB?.ToString() ?? "—";
            UpdateDiceLimits();
        }
    }

    // ====== Leyenda de jugadores
    public class PlayerLegend : Panel
    {
        private IReadOnlyList<PlayerView> _players = Array.Empty<PlayerView>();
        public PlayerLegend() { AutoSize = true; Padding = new Padding(4); }
        public void BindPlayers(IReadOnlyList<PlayerView> players) { _players = players ?? Array.Empty<PlayerView>(); Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int y = 6;
            foreach (var p in _players)
            {
                var rect = new Rectangle(6, y, 18, 18);
                using var br = new SolidBrush(p.Color);
                e.Graphics.FillRectangle(br, rect);
                e.Graphics.DrawRectangle(Pens.Black, rect);
                e.Graphics.DrawString(p.Alias, Font, Brushes.Black, 30, y + 1);
                y += 24;
            }
            Height = Math.Max(30, y + 6);
            Width = Math.Max(Width, 220);
        }
    }

    // ====== Canvas del mapa
    public class MapCanvas : Control
    {
        private readonly List<TerritoryShape> _shapes = new();
        private IReadOnlyList<PlayerView> _players = Array.Empty<PlayerView>();
        private IReadOnlyList<TerritoryView> _territories = Array.Empty<TerritoryView>();
        private int _currentPlayerId = -1;
        private Phase _phase = Phase.Lobby;

        private Dictionary<int, GraphicsPathInfo> _paths = new();
        private readonly ToolTip _tt = new();
        private int? _hoverId = null;
        private float _scale = 1f;
        private List<Bridge> _bridges = new();

        public int? SelectionA { get; set; }
        public int? SelectionB { get; set; }
        public event Action<int>? TerritoryClicked;
        public event Action<int>? TerritoryClickedRight;

        public MapCanvas()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(220, 232, 245);
            Padding = new Padding(12);
            Resize += (_, __) => RebuildPaths();
            MouseDown += MapCanvas_MouseDown;
            MouseMove += MapCanvas_MouseMove;

            _tt.InitialDelay = 120; _tt.ReshowDelay = 100; _tt.AutoPopDelay = 2000; _tt.ShowAlways = true;
        }
        private void MapCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            var id = HitTest(e.Location);
            if (!id.HasValue) return;
            
            if (e.Button == MouseButtons.Left)
            {
                // Click izquierdo = Selección A
                TerritoryClicked?.Invoke(id.Value);
                Console.WriteLine($"🖱️ Click IZQ en territorio {id.Value} → SelectionA");
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Click derecho = Selección B (directamente)
                TerritoryClickedRight?.Invoke(id.Value);
                Console.WriteLine($"🖱️ Click DER en territorio {id.Value} → SelectionB");
            }
        }
        public void LoadShapes(IEnumerable<TerritoryShape> shapes, IEnumerable<Bridge> bridges)
        {
            _shapes.Clear(); _shapes.AddRange(shapes);
            _bridges = bridges?.ToList() ?? new();
            RebuildPaths(); Invalidate();
        }

        public void BindState(IReadOnlyList<PlayerView> players, IReadOnlyList<TerritoryView> territories, int currentPlayerId, Phase phase)
        {
            _players = players ?? Array.Empty<PlayerView>();
            _territories = territories ?? Array.Empty<TerritoryView>();
            _currentPlayerId = currentPlayerId;
            _phase = phase;
            Invalidate();
        }

        private void MapCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            var id = HitTest(e.Location);
            if (id != _hoverId)
            {
                _hoverId = id;
                if (id.HasValue)
                {
                    var t = _territories.FirstOrDefault(x => x.Id == id.Value);
                    if (t != null)
                    {
                        string owner = t.OwnerId == 0 ? "Neutral" : (_players.FirstOrDefault(p => p.Id == t.OwnerId)?.Alias ?? "?");
                        _tt.Show($"{t.Name} | Tropas: {t.Troops} | Dueño: {owner}", this, e.Location + new Size(16, 16), 1600);
                    }
                }
                else _tt.Hide(this);
            }
        }

        private int? HitTest(Point p)
        {
            foreach (var kv in _paths)
                if (kv.Value.Path.IsVisible(p))
                    return kv.Key;
            return null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var ocean = new SolidBrush(Color.FromArgb(205, 220, 235)))
                e.Graphics.FillRectangle(ocean, ClientRectangle);
            if (_paths.Count == 0) return;

            DrawBridges(e.Graphics);

            foreach (var shp in _shapes)
            {
                var info = _paths[shp.Id];
                var ownerColor = OwnerColor(shp.Id);
                using var fill = new SolidBrush(ownerColor);
                float penW = MathF.Max(1.6f, shp.BorderWidth * _scale);
                using var pen = new Pen(Color.Black, penW);
                e.Graphics.FillPath(fill, info.Path);
                e.Graphics.DrawPath(pen, info.Path);

                if (SelectionA == shp.Id) { using var penSel = new Pen(Color.DodgerBlue, penW + 2.0f); e.Graphics.DrawPath(penSel, info.Path); }
                if (SelectionB == shp.Id) { using var penSel = new Pen(Color.OrangeRed, penW + 2.0f); e.Graphics.DrawPath(penSel, info.Path); }

                DrawIdCentered(e.Graphics, shp.Id, info.Bounds);
                DrawTroopBadge(e.Graphics, shp.Id, info.Bounds);
            }
        }

        private void DrawBridges(Graphics g)
        {
            if (_bridges.Count == 0) return;
            foreach (var b in _bridges)
            {
                if (!_paths.TryGetValue(b.FromId, out var a) || !_paths.TryGetValue(b.ToId, out var c)) continue;
                var p1 = a.Centroid; var p2 = c.Centroid;

                using var halo = new Pen(Color.FromArgb(230, 255, 255, 255), MathF.Max(6f, 6f * _scale))
                { DashStyle = DashStyle.Solid, EndCap = LineCap.Round, StartCap = LineCap.Round };
                g.DrawLine(halo, p1, p2);

                using var pen = new Pen(Color.FromArgb(200, 40, 40, 40), MathF.Max(3f, 3f * _scale))
                { DashStyle = DashStyle.Dash, EndCap = LineCap.Round, StartCap = LineCap.Round };
                g.DrawLine(pen, p1, p2);

                float r = MathF.Max(3.5f, 4.5f * _scale);
                using var node = new SolidBrush(Color.FromArgb(230, 40, 40, 40));
                g.FillEllipse(node, p1.X - r, p1.Y - r, 2 * r, 2 * r);
                g.FillEllipse(node, p2.X - r, p2.Y - r, 2 * r, 2 * r);
            }
        }

        private Color OwnerColor(int territoryId)
        {
            var tv = _territories.FirstOrDefault(t => t.Id == territoryId);
            if (tv is null) return Color.Gainsboro;

            // Buscar jugador por OwnerId
            var player = _players.FirstOrDefault(p => p.Id == tv.OwnerId);

            // Si no hay jugador o no tiene color asignado, usa gris claro
            if (player == null)
                return Color.Gainsboro;

            // Devuelve el color real del jugador (incluyendo Neutral gris)
            return player.Color;
        }


        private static Color ContinentColor(string c) => c switch
        {
            "NA" => Color.FromArgb(190, 220, 255),
            "SA" => Color.FromArgb(255, 210, 190),
            "AF" => Color.FromArgb(255, 240, 180),
            "EU" => Color.FromArgb(210, 200, 255),
            "AS" => Color.FromArgb(200, 255, 210),
            "OC" => Color.FromArgb(200, 240, 240),
            _ => Color.LightGray
        };

        private void DrawIdCentered(Graphics g, int territoryId, RectangleF bounds)
        {
            string text = territoryId.ToString();
            float w = bounds.Width;
            float fontSize = MathF.Max(10f, MathF.Min(18f, w * 0.22f));
            using var fnt = new Font(Font.FontFamily, fontSize, FontStyle.Bold);
            var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var halo = new SolidBrush(Color.FromArgb(235, 235, 235));
            var r = new RectangleF(bounds.X, bounds.Y - 1, bounds.Width, bounds.Height);
            g.DrawString(text, fnt, halo, r, fmt);
            g.DrawString(text, fnt, Brushes.Black, bounds, fmt);
        }

        private void DrawTroopBadge(Graphics g, int territoryId, RectangleF bounds)
        {
            int troops = _territories.FirstOrDefault(t => t.Id == territoryId)?.Troops ?? 0;
            if (troops <= 0) return;
            string text = troops.ToString();

            float d = Math.Min(bounds.Width, bounds.Height);
            float r = MathF.Max(12f, MathF.Min(22f, d * 0.28f));

            float cx = bounds.Right - r * 0.9f;
            float cy = bounds.Bottom - r * 0.9f;
            var rect = new RectangleF(cx - r, cy - r, 2 * r, 2 * r);

            using var br = new SolidBrush(Color.White);
            using var pen = new Pen(Color.Black, MathF.Max(2f, _scale * 1.6f));
            g.FillEllipse(br, rect); g.DrawEllipse(pen, rect);
            var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var fnt = new Font(Font.FontFamily, MathF.Max(10f, r * 0.7f), FontStyle.Bold);
            g.DrawString(text, fnt, Brushes.Black, rect, fmt);
        }

        private void RebuildPaths()
        {
            _paths = new Dictionary<int, GraphicsPathInfo>();
            if (_shapes.Count == 0) return;
            var bounds = GetContentBounds();
            var inner = new Rectangle(
                ClientRectangle.Left + Padding.Left,
                ClientRectangle.Top + Padding.Top,
                Math.Max(0, ClientRectangle.Width - Padding.Horizontal),
                Math.Max(0, ClientRectangle.Height - Padding.Vertical));
            if (inner.Width <= 0 || inner.Height <= 0) return;

            _scale = Math.Min(inner.Width / bounds.Width, inner.Height / bounds.Height);
            float offX = inner.Left + (inner.Width - bounds.Width * _scale) / 2f - bounds.Left * _scale;
            float offY = inner.Top + (inner.Height - bounds.Height * _scale) / 2f - bounds.Top * _scale;

            foreach (var shp in _shapes)
            {
                var gp = new GraphicsPath();
                var pts = shp.Points.Select(pt => new PointF(offX + pt.X * _scale, offY + pt.Y * _scale)).ToArray();
                gp.AddPolygon(pts);
                var rect = gp.GetBounds();
                var centroid = ComputeCentroid(pts);
                _paths[shp.Id] = new GraphicsPathInfo(gp, centroid, rect);
            }
        }

        private RectangleF GetContentBounds()
        {
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var shp in _shapes)
                foreach (var p in shp.Points)
                { minX = Math.Min(minX, p.X); minY = Math.Min(minY, p.Y); maxX = Math.Max(maxX, p.X); maxY = Math.Max(maxY, p.Y); }
            if (minX == float.MaxValue) return new RectangleF(0, 0, 1600, 900);
            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }

        private static PointF ComputeCentroid(PointF[] pts)
        {
            double cx = 0, cy = 0, a = 0;
            for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
            {
                double cross = pts[j].X * pts[i].Y - pts[i].X * pts[j].Y;
                a += cross; cx += (pts[j].X + pts[i].X) * cross; cy += (pts[j].Y + pts[i].Y) * cross;
            }
            if (Math.Abs(a) < 1e-6) return new PointF(pts.Average(p => p.X), pts.Average(p => p.Y));
            a *= 0.5; cx /= (6 * a); cy /= (6 * a);
            return new PointF((float)cx, (float)cy);
        }
    }

    // ====== Generador de mapa (42 territorios con puentes entre continentes)
    public static class MapGenerator
    {
        public static MapLayout GenerateRisk42WithBridges()
        {
            const float R = 36f;
            const float GAP = 8f;
            float stepX = 1.5f * R + GAP;
            float stepY = (float)(Math.Sqrt(3) * R) + GAP;

            int id = 1;
            var shapes = new List<TerritoryShape>();

            var na = Cluster(new PointF(120, 110), R, stepX, stepY, "NA", new[] { 2, 2, 2 }, ref id, shapes);
            var sa = Cluster(new PointF(360, 460), R, stepX, stepY, "SA", new[] { 2, 2 }, ref id, shapes);
            var eu = Cluster(new PointF(700, 170), R, stepX, stepY, "EU", new[] { 3, 4, 3 }, ref id, shapes);
            var af = Cluster(new PointF(700, 430), R, stepX, stepY, "AF", new[] { 2, 2, 2 }, ref id, shapes);
            var @as = Cluster(new PointF(1040, 210), R, stepX, stepY, "AS", new[] { 4, 5, 4 }, ref id, shapes);
            var oc = Cluster(new PointF(1320, 740), R, stepX, stepY, "OC", new[] { 2, 1 }, ref id, shapes);

            var bridges = new List<Bridge>
            {
                new(na.SouthId, sa.NorthId), // NA–SA
                new(na.EastId, eu.WestId),   // NA–EU
                // (NA–AS eliminado)
                new(eu.SouthId, af.NorthId), // EU–AF
                new(eu.EastId,  @as.WestId), // EU–AS
                new(sa.EastId,  af.WestId),  // SA–AF
                new(af.EastId,  @as.WestId), // AF–AS
                new(@as.SouthId, oc.NorthId) // AS–OC
            };

            return new MapLayout(shapes, bridges);
        }

        private record ClusterExtrema(int NorthId, int SouthId, int WestId, int EastId);

        private static ClusterExtrema Cluster(PointF topLeft, float r, float stepX, float stepY, string cont, int[] rows, ref int id, List<TerritoryShape> outList)
        {
            int maxRow = rows.Length == 0 ? 0 : rows.Max();
            var local = new List<TerritoryShape>();

            for (int row = 0; row < rows.Length; row++)
            {
                int cols = rows[row];
                float rowWidth = cols * stepX;
                float left = topLeft.X + (maxRow * stepX - rowWidth) / 2f + ((row % 2 == 1) ? 0.75f * r : 0f);
                float y = topLeft.Y + row * stepY;

                for (int c = 0; c < cols; c++)
                {
                    float cx = left + c * stepX;
                    float cy = y;
                    var pts = Hex(cx, cy, r);
                    var shape = new TerritoryShape { Id = id, Name = $"{cont}-{id}", Continent = cont, Points = pts };
                    outList.Add(shape);
                    local.Add(shape);
                    id++;
                }
            }

            int north = local.OrderBy(s => s.Points.Average(p => p.Y)).First().Id;
            int south = local.OrderByDescending(s => s.Points.Average(p => p.Y)).First().Id;
            int west = local.OrderBy(s => s.Points.Average(p => p.X)).First().Id;
            int east = local.OrderByDescending(s => s.Points.Average(p => p.X)).First().Id;

            return new ClusterExtrema(north, south, west, east);
        }

        private static List<PointF> Hex(float cx, float cy, float r)
        {
            var pts = new List<PointF>(6);
            for (int i = 0; i < 6; i++)
            {
                float ang = (float)(Math.PI / 3 * i);
                pts.Add(new PointF(cx + (float)Math.Cos(ang) * r, cy + (float)Math.Sin(ang) * r));
            }
            return pts;
        }
    }

    // ====== Panel de dados (visual)
    public class DicePanel : Panel
    {
        private int[] _atk = Array.Empty<int>();
        private int[] _def = Array.Empty<int>();
        private readonly System.Windows.Forms.Timer _rollTimer = new();
        private readonly Random _rng = new();
        private bool _rolling;
        public DicePanel()
        {
            DoubleBuffered = true; Height = 110; Width = 330;
            _rollTimer.Interval = 70;
            _rollTimer.Tick += (_, __) => { if (_rolling) { _atk = FakeRoll(3); _def = FakeRoll(2); Invalidate(); } };
        }
        private int[] FakeRoll(int n) { var arr = new int[Math.Max(0, n)]; for (int i = 0; i < arr.Length; i++) arr[i] = 1 + _rng.Next(6); return arr; }
        public void StartRolling() { _rolling = true; _rollTimer.Start(); }
        public void ShowFinal(int[] atk, int[] def) { _rolling = false; _rollTimer.Stop(); _atk = atk ?? Array.Empty<int>(); _def = def ?? Array.Empty<int>(); Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawRow(e.Graphics, 26, "ATK", _atk); DrawRow(e.Graphics, 76, "DEF", _def);
        }
        private static void DrawRow(Graphics g, int y, string label, int[] vals)
        {
            g.DrawString(label, SystemFonts.DefaultFont, Brushes.Black, 0, y - 12);
            int x = 58; for (int i = 0; i < vals.Length; i++)
            {
                var r = new Rectangle(x, y - 18, 36, 36);
                g.FillRectangle(Brushes.White, r); g.DrawRectangle(Pens.Black, r);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var fnt = new Font(SystemFonts.DefaultFont.FontFamily, 13, FontStyle.Bold);
                g.DrawString(vals[i].ToString(), fnt, Brushes.Black, r, fmt);
                x += 44;
            }
        }
    }
}
