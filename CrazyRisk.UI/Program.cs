using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrazyRisk.Modelo;
using CrazyRisk.Comunicacion;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var start = new StartForm();
            Console.WriteLine("✅ Aplicación iniciada - StartForm creado");

            void OpenGameDemo(string alias, int port)
            {
                Console.WriteLine($"🎮 Abriendo MODO DEMO para: {alias}");

                var game = new GameForm();
                game.FormClosed += (_, __) =>
                {
                    Console.WriteLine("🚪 Cerrando aplicación desde GameForm");
                    start.Close();
                    Application.Exit();
                };

                // Tu código demo que funciona
                var players = new List<PlayerView>
                {
                    PlayerView.Neutral(),
                    new(1, alias, Color.FromArgb(52,136,245), 0),
                    new(2, "Rival", Color.FromArgb(234,85,69), 0)
                };
                game.SetPlayers(players);
                game.SetPhase(Phase.Reinforcements);
                game.SetCurrentPlayer(1);
                game.SetReinforcements(5);
                game.SetGlobalTradeCounter(2);

                var shapes = MapGenerator.GenerateRisk42WithBridges().Shapes;
                var terrs = shapes.Select((s, i) =>
                {
                    int owner = (i % 3 == 0) ? 0 : (i % 3 == 1 ? 1 : 2);
                    int troops = 1 + (i % 3);
                    return new TerritoryView(s.Id, $"T{s.Id}", s.Continent, owner, troops);
                }).OrderBy(t => t.Id).ToList();
                game.SetTerritories(terrs);

                Console.WriteLine("✅ Datos demo cargados en GameForm");

                start.Hide();
                game.Show();
                Console.WriteLine("✅ GameForm mostrado");
            }

            void OpenGameAsServer(string alias, int port)
            {
                try
                {
                    Console.WriteLine($"Abriendo como SERVIDOR: {alias}:{port}");
                    
                    var game = new GameForm();
                    game.FormClosed += (_, __) => { start.Close(); Application.Exit(); };

                    // Servidor crea la partida
                    var mapa = CreadorMapa.CrearMapaMundial();
                    var partida = new Partida(mapa);

                    var jugador1 = new Jugador(alias, "#EA5545", 0);
                    var jugador2 = new Jugador("Cliente", "#3488F5", 0);
                    var neutral = new EjercitoNeutral("Neutral", "#A0A0A0", 0);

                    partida.AddJugador(jugador1);
                    partida.AddJugador(jugador2);
                    partida.AddJugador(neutral);
                    partida.PrepararPartida();
                    partida.ColocarRefuerzosNeutral();

                    var controller = new GameController(game, partida, true, alias);
                    controller.IniciarRed(puerto: port);
                    controller.ForzarActualizacionUI();

                    start.Hide();
                    game.Show();
                    
                    MessageBox.Show($"Servidor activo en puerto {port}", "Servidor");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    MessageBox.Show($"Error: {ex.Message}", "Error");
                }
            }

            void OpenGameAsClient(string alias, string ipServidor, int port)
            {
                try
                {
                    Console.WriteLine($"Abriendo como CLIENTE: {alias} -> {ipServidor}:{port}");
                    
                    var game = new GameForm();
                    game.FormClosed += (_, __) => { start.Close(); Application.Exit(); };

                    // Cliente NO crea partida aún, espera datos del servidor
                    var mapa = CreadorMapa.CrearMapaMundial();
                    var partida = new Partida(mapa);

                    // Crear jugadores vacíos (se llenarán con datos del servidor)
                    var controller = new GameController(game, partida, false, alias);
                    
                    // Conectar al servidor
                    controller.IniciarRed(puerto: port, ipServidor: ipServidor);
                    
                    // Solicitar estado inicial
                    System.Threading.Thread.Sleep(500); // Esperar conexión
                    controller.SolicitarEstadoInicial();

                    start.Hide();
                    game.Show();
                    
                    Console.WriteLine("Cliente conectado, esperando datos del servidor...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    MessageBox.Show($"Error: {ex.Message}", "Error");
                }
            }

            // CONFIGURAR EVENTOS
            start.HostRequested += (s, e) => 
            {
                Console.WriteLine($"Evento HostRequested: {e.Alias}:{e.Port}");
                OpenGameAsServer(e.Alias, e.Port);
            };

            start.JoinRequested += (s, e) => 
            {
                Console.WriteLine($"Evento JoinRequested: {e.Alias} -> {e.Host}:{e.Port}");
                OpenGameAsClient(e.Alias, e.Host, e.Port);
            };

            Console.WriteLine("🚀 Iniciando Application.Run...");
            Application.Run(start);
            Console.WriteLine("🏁 Application.Run terminado");
        }
    }
}