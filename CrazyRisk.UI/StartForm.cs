using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class StartForm : Form
    {
        public event EventHandler<HostRequestArgs>? HostRequested;
        public event EventHandler<JoinRequestArgs>? JoinRequested;

        private readonly RadioButton _rbHost = new() { Text = "Hostear (Servidor)", Checked = true };
        private readonly RadioButton _rbJoin = new() { Text = "Unirse (Cliente)" };
        private readonly TextBox _txtAlias = new() { Width = 280 };
        private readonly TextBox _txtHost = new() { Width = 200, Text = "192.168.0.28" };
        private readonly NumericUpDown _nudPort = new() { Minimum = 1024, Maximum = 65535, Value = 9000, Width = 110 };
        private readonly Button _btnConnect = new() { Text = "Conectar", AutoSize = true };

        public StartForm()
        {
            Text = "CrazyRisk – Inicio (UI)";
            MinimumSize = new Size(600, 380);
            AutoScaleMode = AutoScaleMode.Dpi;

            KeyPreview = true;
            KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), RowCount = 6 };
            for (int i = 0; i < 6; i++) root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            var title = new Label { Text = "CrazyRisk", AutoSize = true };
            title.Font = new Font(title.Font.FontFamily, 24, FontStyle.Bold);
            root.Controls.Add(title);

            _txtAlias.Text = Environment.UserName;
            var pAlias = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            pAlias.Controls.Add(new Label { Text = "Alias:", AutoSize = true, Margin = new Padding(0, 8, 8, 0) });
            pAlias.Controls.Add(_txtAlias);
            root.Controls.Add(pAlias);

            var role = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            role.Controls.Add(_rbHost);
            role.Controls.Add(_rbJoin);
            root.Controls.Add(role);

            var net = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            net.Controls.Add(new Label { Text = "Host/IP:", AutoSize = true, Margin = new Padding(0, 8, 8, 0) });
            net.Controls.Add(_txtHost);
            net.Controls.Add(new Label { Text = "Puerto:", AutoSize = true, Margin = new Padding(16, 8, 8, 0) });
            net.Controls.Add(_nudPort);
            root.Controls.Add(net);

            _btnConnect.Click += BtnConnect_Click;
            root.Controls.Add(_btnConnect);

            root.Controls.Add(new Label
            {
                AutoSize = true,
                MaximumSize = new Size(520, 0),
            });
        }

        private void BtnConnect_Click(object? sender, EventArgs e)
        {
            string alias = string.IsNullOrWhiteSpace(_txtAlias.Text) ? "Jugador" : _txtAlias.Text.Trim();
            string host = _txtHost.Text.Trim();
            int port = (int)_nudPort.Value;

            if (_rbHost.Checked) HostRequested?.Invoke(this, new HostRequestArgs(alias, port));
            else JoinRequested?.Invoke(this, new JoinRequestArgs(alias, host, port));
        }
    }
}
