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

            void OpenGameWithController(string alias, int port, bool esServidor, string ipServidor = null)
            {
                try
                {
                    Console.WriteLine($"Intentando abrir juego: {alias}, Servidor={esServidor}");
                    
                    var game = new GameForm();
                    game.FormClosed += (_, __) => 
                    {
                        Console.WriteLine("Cerrando aplicación desde GameForm");
                        start.Close();
                        Application.Exit();
                    };

                    // Crear mapa y partida
                    var mapa = CreadorMapa.CrearMapaMundial();
                    var partida = new Partida(mapa);

                    // Crear jugadores
                    var jugador1 = new Jugador(alias, "#EA5545", 40);
                    var jugador2 = new Jugador("Oponente", "#3488F5", 40);
                    var neutral = new EjercitoNeutral("Neutral", "#A0A0A0", 40);

                    partida.AddJugador(jugador1);
                    partida.AddJugador(jugador2);
                    partida.AddJugador(neutral);

                    // Preparar partida
                    partida.PrepararPartida();

                    // Crear GameController
                    var controller = new GameController(game, partida, esServidor, alias);

                    // Iniciar red
                    if (esServidor)
                    {
                        Console.WriteLine($"Iniciando servidor en puerto {port}...");
                        controller.IniciarRed(puerto: port);
                        MessageBox.Show($"Servidor iniciado en puerto {port}\nEsperando conexiones...", 
                                    "Servidor Activo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(ipServidor))
                        {
                            MessageBox.Show("Se requiere IP del servidor", "Error");
                            return;
                        }
                        
                        Console.WriteLine($"Conectando a {ipServidor}:{port}...");
                        controller.IniciarRed(puerto: port, ipServidor: ipServidor);
                    }

                    // Forzar actualización UI
                    controller.ForzarActualizacionUI();
                    
                    start.Hide();
                    game.Show();
                    Console.WriteLine("GameForm mostrado con controlador");
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
                OpenGameWithController(e.Alias, e.Port, esServidor: true);
            };

            start.JoinRequested += (s, e) => 
            {
                Console.WriteLine($"Evento JoinRequested: {e.Alias} -> {e.Host}:{e.Port}");
                OpenGameWithController(e.Alias, e.Port, esServidor: false, ipServidor: e.Host);
            };

            Console.WriteLine("🚀 Iniciando Application.Run...");
            Application.Run(start);
            Console.WriteLine("🏁 Application.Run terminado");
        }
    }
}