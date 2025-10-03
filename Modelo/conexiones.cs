using System;
using System.Linq;
using System.Windows.Forms;
using CrazyRisk.Modelo;
using CrazyRisk.Comunicacion;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    /// Controlador principal que coordina UI, Lógica y Red
    public class GameController
    {
        private readonly GameForm _ui;
        private readonly Partida _partida;
        private Servidor _servidor;
        private Cliente _cliente;
        
        private readonly bool _esServidor;
        private readonly string _aliasLocal;
        private Jugador _jugadorLocal;
        private bool _esNeutralAutomatico = true;

        public GameController(GameForm ui, Partida partida, bool esServidor, string aliasLocal)
        {
            _ui = ui;
            _partida = partida;
            _esServidor = esServidor;
            _aliasLocal = aliasLocal;

            if (_partida != null)
            {
                _partida.OnEventoJuego += OnEventoLogica;
                Console.WriteLine("✅ Suscrito a eventos de partida");
            }

            ConectarEventosUI();
            ConectarEventosLogica();
            // 🆕 Forzar primera actualización
            ForzarActualizacionUI();
            try
            {
                // Si ya hay jugadores en la partida, identificar el jugador local por alias
                var arr = _partida?.Jugadores?.ToArray();
                if (arr != null && arr.Length > 0)
                {
                    _jugadorLocal = arr.FirstOrDefault(j => string.Equals(j.Alias, _aliasLocal, StringComparison.OrdinalIgnoreCase));
                    if (_jugadorLocal == null)
                    {
                        // fallback: toma el que NO sea "Cliente" ni "Neutral"
                        _jugadorLocal = arr.FirstOrDefault(j => !string.Equals(j.Alias, "Cliente", StringComparison.OrdinalIgnoreCase)
                                                            && !string.Equals(j.Alias, "Neutral", StringComparison.OrdinalIgnoreCase));
                    }
                    Console.WriteLine($"🆔 Jugador local detectado en controller: {_jugadorLocal?.Alias ?? "NULL"}");
                }
            }
            catch { /* ignore */ }

        }
        public void SolicitarEstadoInicial()
        {
            if (!_esServidor && _cliente != null)
            {
                _cliente.Enviar("SOLICITAR_ESTADO");
                Console.WriteLine("Solicitando estado inicial del servidor...");
            }
        }
        #region Conexión de Eventos UI → Lógica
        
        private void ConectarEventosUI()
        {
            // En el constructor de GameController o en ActualizarUI
            _ui.StartGameRequested += OnStartGameRequested;
            _ui.EndTurnRequested += OnEndTurnRequested;
            _ui.PlaceRequested += OnPlaceRequested;
            _ui.AttackRequested += OnAttackRequested;
            _ui.FortifyRequested += OnFortifyRequested;
            _ui.ExchangeCardsRequested += OnExchangeCardsRequested;
            _ui.ViewCardsRequested += OnViewCardsRequested;
            _ui.AutoPlaceRequested += OnAutoPlaceRequested;
            _ui.InitiativeRequested += OnInitiativeRequested;
            _ui.NeutralAutoChanged += OnNeutralAutoChanged;
        }

        private void OnStartGameRequested()
        {
            try
            {
                _partida.PrepararPartida();
                ActualizarUI();
                MostrarMensaje("Partida iniciada. Coloca tus tropas iniciales.", "Inicio");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al iniciar partida: {ex.Message}");
            }
        }

        
        private void OnPlaceRequested(int territorioId, int cantidad)
        {
            try
            {
                Console.WriteLine($"\n🎯 DEBUG OnPlaceRequested:");
                Console.WriteLine($"  TerritoryId: {territorioId}");
                Console.WriteLine($"  Cantidad: {cantidad}");
                Console.WriteLine($"  EsServidor: {_esServidor}");
                
                if (_esServidor)
                {
                    // ✅ SERVIDOR: Ejecutar localmente
                    var jugador = _partida.GetJugadorActual();
                    Console.WriteLine($"  Jugador actual: {jugador?.Alias ?? "NULL"}");
                    Console.WriteLine($"  Tropas disponibles: {jugador?.TropasDisponibles ?? 0}");
                    
                    string nombreTerr = MapeoTerritorios.ObtenerNombreTerritorio(territorioId);
                    Console.WriteLine($"  Nombre territorio: {nombreTerr}");
                    
                    var territorio = _partida.Mapa.ObtenerTerritorioPorNombre(nombreTerr);
                    
                    if (territorio == null)
                    {
                        Console.WriteLine($"  ❌ Territorio no encontrado: {nombreTerr}");
                        MostrarError($"Territorio no encontrado: {nombreTerr}");
                        return;
                    }
                    
                    Console.WriteLine($"  Territorio encontrado: {territorio.Nombre}");
                    Console.WriteLine($"  Dueño del territorio: {territorio.Dueno?.Alias ?? "NULL"}");
                    Console.WriteLine($"  Tropas actuales: {territorio.Tropas}");

                    if (territorio.Dueno != jugador)
                    {
                        Console.WriteLine($"  ❌ El territorio NO es tuyo");
                        MostrarError($"El territorio {territorio.Nombre} no es tuyo. Dueño: {territorio.Dueno?.Alias ?? "Nadie"}");
                        return;
                    }

                    int colocadas = 0;
                    for (int i = 0; i < cantidad; i++)
                    {
                        if (_partida.ColocarTropa(jugador, territorio))
                        {
                            colocadas++;
                            Console.WriteLine($"  ✅ Tropa {i+1}/{cantidad} colocada");
                        }
                        else
                        {
                            Console.WriteLine($"  ❌ No se pudo colocar tropa {i+1}/{cantidad}");
                            break;
                        }
                    }

                    if (colocadas > 0)
                    {
                        ActualizarUI();
                        MostrarMensaje($"Colocadas {colocadas} tropas en {territorio.Nombre}", "Refuerzos");
                        Console.WriteLine($"  ✅ Total colocadas: {colocadas}");
                        
                        // El servidor difunde automáticamente por el evento OnEventoJuego
                    }
                    else
                    {
                        MostrarError("No se pudieron colocar tropas.");
                        Console.WriteLine($"  ❌ NO se colocó ninguna tropa");
                    }
                }
                else
                {
                    // ✅ CLIENTE: Solo enviar comando al servidor
                    if (_cliente != null)
                    {
                        _cliente.Enviar($"TROPA:{territorioId},{cantidad}");
                        Console.WriteLine($"  📡 Comando enviado al servidor: TROPA:{territorioId},{cantidad}");
                    }
                    else
                    {
                        MostrarError("No hay conexión con el servidor.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ EXCEPCIÓN: {ex.Message}");
                MostrarError($"Error al colocar tropas: {ex.Message}");
            }
        }

        private void OnAutoPlaceRequested()
        {
            try
            {
                var jugador = _partida.GetJugadorActual();
                var territorios = jugador.GetTerritorios();
                
                if (territorios.Length == 0 || jugador.TropasDisponibles <= 0)
                {
                    MostrarError("No hay tropas disponibles o territorios para colocar.");
                    return;
                }

                var rnd = new Random();
                int tropasRestantes = jugador.TropasDisponibles;
                
                while (tropasRestantes > 0)
                {
                    var territorio = territorios[rnd.Next(territorios.Length)];
                    if (_partida.ColocarTropa(jugador, territorio))
                        tropasRestantes--;
                    else
                        break;
                }

                ActualizarUI();
                MostrarMensaje("Tropas colocadas automáticamente", "Auto-colocación");
                
                // ✅ ENVIAR COMANDO A RED (con todas las tropas colocadas)
                if (!_esServidor && _cliente != null)
                {
                    _cliente.Enviar($"AUTOPLACE");
                    Console.WriteLine($"  📡 Comando enviado: AUTOPLACE");
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error en colocación automática: {ex.Message}");
            }
        }

        private void OnAttackRequested(int origenId, int destinoId, int dadosAtk, int dadosDef)
        {
            try
            {
                Console.WriteLine($"\n⚔️ DEBUG OnAttackRequested:");
                Console.WriteLine($"  Origen: {origenId}, Destino: {destinoId}");
                Console.WriteLine($"  Dados Atk: {dadosAtk}, Dados Def: {dadosDef}");
                Console.WriteLine($"  EsServidor: {_esServidor}");
                
                if (_esServidor)
                {
                    // Ejecutar localmente
                    string nombreOrigen = MapeoTerritorios.ObtenerNombreTerritorio(origenId);
                    string nombreDestino = MapeoTerritorios.ObtenerNombreTerritorio(destinoId);

                    Console.WriteLine($"  Nombre Origen: {nombreOrigen}");
                    Console.WriteLine($"  Nombre Destino: {nombreDestino}");

                    var origen = _partida.Mapa.ObtenerTerritorioPorNombre(nombreOrigen);
                    var destino = _partida.Mapa.ObtenerTerritorioPorNombre(nombreDestino);

                    if (origen == null || destino == null)
                    {
                        Console.WriteLine($"  ❌ Territorios no encontrados");
                        MostrarError("Territorios no válidos para ataque");
                        return;
                    }

                    Console.WriteLine($"  Origen encontrado: {origen.Nombre} (Dueño: {origen.Dueno?.Alias})");
                    Console.WriteLine($"  Destino encontrado: {destino.Nombre} (Dueño: {destino.Dueno?.Alias})");
                    Console.WriteLine($"  Tropas Origen: {origen.Tropas}, Tropas Destino: {destino.Tropas}");

                    bool conquistado = _partida.RealizarAtaque(origen, destino, dadosAtk);
                    Console.WriteLine($"  ✅ Ataque ejecutado. Conquistado: {conquistado}");

                    // Actualizar UI localmente
                    ActualizarUI();

                    if (conquistado)
                    {
                        MostrarMensaje($"¡{origen.Nombre} conquistó {destino.Nombre}!", "Victoria");
                        
                        var ganador = _partida.VerificarVictoria();
                        if (ganador != null)
                        {
                            _ui.SetPhase(Phase.GameOver);
                            MostrarMensaje($"¡{ganador.Alias} ha ganado la partida!", "¡Victoria Total!");
                        }
                    }

                    // Difundir cambios a todos los clientes
                    _servidor?.Difundir($"ACTUALIZAR_TERRITORIO:{origen.Nombre},{origen.Dueno.Alias},{origen.Tropas}");
                    _servidor?.Difundir($"ACTUALIZAR_TERRITORIO:{destino.Nombre},{destino.Dueno.Alias},{destino.Tropas}");
                }
                else
                {
                    // Cliente: solo enviar comando al servidor
                    if (_cliente != null)
                    {
                        _cliente.Enviar($"ATACAR:{origenId},{destinoId},{dadosAtk}");
                        Console.WriteLine($"📡 Comando enviado al servidor: ATACAR:{origenId},{destinoId},{dadosAtk}");
                    }
                    else
                    {
                        MostrarError("No hay conexión con el servidor.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ataque: {ex.Message}");
                MostrarError($"Error en ataque: {ex.Message}");
            }
        }

        private void OnFortifyRequested(int origenId, int destinoId, int cantidad)
        {
            try
            {
                Console.WriteLine($"\n🚚 DEBUG OnFortifyRequested:");
                Console.WriteLine($"  Origen: {origenId}, Destino: {destinoId}");
                Console.WriteLine($"  Cantidad: {cantidad}");
                Console.WriteLine($"  EsServidor: {_esServidor}");
                
                if (_esServidor)
                {
                    var jugador = _partida.GetJugadorActual();
                    string nombreOrigen = MapeoTerritorios.ObtenerNombreTerritorio(origenId);
                    string nombreDestino = MapeoTerritorios.ObtenerNombreTerritorio(destinoId);

                    Console.WriteLine($"  Jugador: {jugador.Alias}");
                    Console.WriteLine($"  Nombre Origen: {nombreOrigen}");
                    Console.WriteLine($"  Nombre Destino: {nombreDestino}");

                    var origen = _partida.Mapa.ObtenerTerritorioPorNombre(nombreOrigen);
                    var destino = _partida.Mapa.ObtenerTerritorioPorNombre(nombreDestino);

                    if (origen == null || destino == null)
                    {
                        Console.WriteLine($"  ❌ Territorios no encontrados");
                        MostrarError("Territorios no válidos para movimiento");
                        return;
                    }

                    Console.WriteLine($"  Origen: {origen.Nombre} (Dueño: {origen.Dueno?.Alias}, Tropas: {origen.Tropas})");
                    Console.WriteLine($"  Destino: {destino.Nombre} (Dueño: {destino.Dueno?.Alias}, Tropas: {destino.Tropas})");

                    _partida.MoverTropas(jugador, origen, destino, cantidad);
                    Console.WriteLine($"  ✅ Movimiento ejecutado");

                    ActualizarUI();
                    MostrarMensaje($"Movidas {cantidad} tropas de {origen.Nombre} a {destino.Nombre}", "Fortificación");

                    // Difundir cambios a todos los clientes
                    _servidor?.Difundir($"ACTUALIZAR_TERRITORIO:{origen.Nombre},{origen.Dueno.Alias},{origen.Tropas}");
                    _servidor?.Difundir($"ACTUALIZAR_TERRITORIO:{destino.Nombre},{destino.Dueno.Alias},{destino.Tropas}");
                }
                else
                {
                    // Cliente: solo enviar comando al servidor
                    if (_cliente != null)
                    {
                        _cliente.Enviar($"FORTIFY:{origenId},{destinoId},{cantidad}");
                        Console.WriteLine($"📡 Comando enviado al servidor: FORTIFY:{origenId},{destinoId},{cantidad}");
                    }
                    else
                    {
                        MostrarError("No hay conexión con el servidor.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al mover tropas: {ex.Message}");
                MostrarError($"Error al mover tropas: {ex.Message}");
            }
        }

        private void OnEndTurnRequested()
        {
            try
            {
                if (_esServidor)
                {
                    var actual = _partida.GetJugadorActual();

                    if (_partida.RequiereIntercambioForzoso(actual))
                    {
                        MostrarError($"{actual.Alias} debe intercambiar cartas antes de terminar el turno.");
                        return;
                    }

                    switch (_partida.Estado)
                    {
                        case EstadoJuego.Refuerzos:
                            if (actual == _jugadorLocal) 
                            {
                                // 🔹 Servidor en Refuerzos → ahora pasa turno al Cliente en Refuerzos
                                _partida.AvanzarTurno();
                                _partida.CambiarEstado(EstadoJuego.Refuerzos);
                            }
                            else 
                            {
                                // 🔹 Cliente en Refuerzos → vuelve al Servidor en Ataques
                                _partida.AvanzarTurno();
                                _partida.CambiarEstado(EstadoJuego.Ataques);
                            }
                            break;

                        case EstadoJuego.Ataques:
                            // 🔹 Ataques → Planeación (mismo jugador)
                            _partida.CambiarEstado(EstadoJuego.Planeacion);
                            break;

                        case EstadoJuego.Planeacion:
                            if (actual == _jugadorLocal) 
                            {
                                // 🔹 Servidor terminó Planeación → pasa al Cliente en Ataques
                                _partida.AvanzarTurno();
                                _partida.CambiarEstado(EstadoJuego.Ataques);
                            }
                            else 
                            {
                                // 🔹 Cliente terminó Planeación → vuelve al Servidor en Refuerzos
                                _partida.AvanzarTurno();
                                _partida.CambiarEstado(EstadoJuego.Refuerzos);
                            }
                            break;
                    }

                    ActualizarUI();
                }
                else
                {
                    _cliente?.Enviar("ENDTURN");
                    Console.WriteLine($"📡 Enviado: ENDTURN");
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error al avanzar turno: {ex.Message}");
            }
        }



        private void OnExchangeCardsRequested()
        {
            try
            {
                var jugador = _partida.GetJugadorActual();
                var cartas = jugador.GetCartas();

                if (cartas.Length < 3)
                {
                    MostrarError("Necesitas al menos 3 cartas para intercambiar.");
                    return;
                }

                Carta[] trio = BuscarTrioValido(cartas);
                
                if (trio == null)
                {
                    MostrarError("No tienes un trío válido (3 iguales o 1 de cada tipo).");
                    return;
                }

                int tropasRecibidas = _partida.CanjearCartas(jugador, trio);
                ActualizarUI();
                MostrarMensaje($"Intercambiaste cartas por {tropasRecibidas} tropas", "Intercambio");
                
                // ✅ ENVIAR COMANDO A RED
                if (!_esServidor && _cliente != null)
                {
                    string tiposTrio = string.Join(",", trio.Select(c => c.Tipo));
                    _cliente.Enviar($"EXCHANGE:{tiposTrio}");
                    Console.WriteLine($"  📡 Comando enviado: EXCHANGE:{tiposTrio}");
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error al intercambiar cartas: {ex.Message}");
            }
        }

        private Carta[] BuscarTrioValido(Carta[] cartas)
        {
            // Intentar 3 iguales
            var infanteria = cartas.Where(c => c is CartaInfanteria).ToArray();
            var caballeria = cartas.Where(c => c is CartaCaballeria).ToArray();
            var artilleria = cartas.Where(c => c is CartaArtilleria).ToArray();

            if (infanteria.Length >= 3) return infanteria.Take(3).ToArray();
            if (caballeria.Length >= 3) return caballeria.Take(3).ToArray();
            if (artilleria.Length >= 3) return artilleria.Take(3).ToArray();

            // Intentar 1 de cada tipo
            if (infanteria.Length >= 1 && caballeria.Length >= 1 && artilleria.Length >= 1)
            {
                return new Carta[] { infanteria[0], caballeria[0], artilleria[0] };
            }

            return null;
        }

        private void OnViewCardsRequested()
        {
            var jugador = _partida.GetJugadorActual();
            var cartas = jugador.GetCartas();

            if (cartas.Length == 0)
            {
                MostrarMensaje("No tienes cartas.", "Mis Cartas");
                return;
            }

            string mensaje = "Tus cartas:\n";
            var grupos = cartas.GroupBy(c => c.Tipo);
            foreach (var grupo in grupos)
            {
                mensaje += $"• {grupo.Key}: {grupo.Count()}\n";
            }

            MostrarMensaje(mensaje, "Mis Cartas");
        }

        private void OnInitiativeRequested()
        {
            // Determinar quién ataca primero con dados
            var rnd = new Random();
            int dado1 = rnd.Next(1, 7);
            int dado2 = rnd.Next(1, 7);

            var jugadores = _partida.Jugadores.ToArray();
            if (jugadores.Length < 2) return;

            int atacante = dado1 >= dado2 ? 0 : 1;
            _ui.ShowInitiativeResult(atacante + 1, dado1, dado2);
        }

        private void OnNeutralAutoChanged(bool activado)
        {
            _esNeutralAutomatico = activado;
        }

        #endregion

        #region Conexión de Eventos Lógica → UI

        private void ConectarEventosLogica()
        {
            _partida.OnEventoJuego += OnEventoLogica;
        }

        private void OnEventoLogica(EventoJuego evento)
        {
            // Actualizar UI según el tipo de evento
            switch (evento.Tipo)
            {
               case nameof(TipoEvento.COMBATE):
                    if (evento.Datos is ResultadoCombate combate)
                    {
                        // Mostrar animación de dados primero
                        _ui.ShowDiceRolling();

                        // Esperar 1 segundo y luego mostrar resultado final
                        Task.Delay(1000).ContinueWith(_ =>
                        {
                            _ui.Invoke(new Action(() =>
                            {
                                _ui.ShowDiceResult(combate.DadosAtacante, combate.DadosDefensor);
                            }));
                        });
                    }
                    break;


                case nameof(TipoEvento.TURNO_CAMBIADO):
                    ActualizarUI();
                    break;

                case nameof(TipoEvento.ESTADO_CAMBIADO):
                    ActualizarUI();
                    break;

                case nameof(TipoEvento.REFUERZOS_CALCULADOS):
                    ActualizarUI();
                    break;
            }

            // Log del evento
            Console.WriteLine($"[{evento.Tipo}] {evento.Mensaje}");
        }

        #endregion

        #region Actualización de UI

        private void ActualizarUI()
        {
            try
            {
                if (_partida == null) return;
                
                Console.WriteLine("🔄 Actualizando UI desde GameController...");
                
                // Convertir jugadores de lógica a UI
                var jugadores = _partida.Jugadores.ToArray();
                var playerViews = MapeoTerritorios.ConvertirJugadoresAPlayerView(jugadores);
                _ui.SetPlayers(playerViews);
                
                // Convertir territorios de lógica a UI  
                var territorios = MapeoTerritorios.ConvertirMapaATerritorios(_partida.Mapa);
                _ui.SetTerritories(territorios);
                
                // 🔍 DEBUG: Verificar IDs
                Console.WriteLine("\n🔍 DEBUG - Jugadores:");
                foreach (var p in playerViews)
                {
                    Console.WriteLine($"  ID={p.Id}, Alias={p.Alias}, Color={p.Color}");
                }

                Console.WriteLine("\n🔍 DEBUG - Primeros 5 territorios:");
                foreach (var t in territorios.Take(5))
                {
                    Console.WriteLine($"  T{t.Id} '{t.Name}' → OwnerId={t.OwnerId}");
                }
                // Actualizar jugador actual
                var jugadorActual = _partida.GetJugadorActual();
                if (jugadorActual != null)
                {
                    int playerId = playerViews.FirstOrDefault(p => p.Alias == jugadorActual.Alias)?.Id ?? 1;
                    _ui.SetCurrentPlayer(playerId);
                    
                    Console.WriteLine($"🎯 Actualizando refuerzos: {jugadorActual.TropasDisponibles}");
                    _ui.SetReinforcements(jugadorActual.TropasDisponibles);
                }
                
                // Actualizar fase
                Phase phase = _partida.Estado switch
                {
                    EstadoJuego.Preparacion => Phase.Setup,
                    EstadoJuego.Refuerzos => Phase.Reinforcements,
                    EstadoJuego.Ataques => Phase.Attacks,
                    EstadoJuego.Planeacion => Phase.Fortify,
                    EstadoJuego.Finalizado => Phase.GameOver,
                    _ => Phase.Lobby
                };
                _ui.SetPhase(phase);
                
                Console.WriteLine("✅ UI actualizada correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ActualizarUI: {ex.Message}");
                // Fallback: usar datos de demo
                UsarDatosDemo();
            }
        }

        private void UsarDatosDemo()
        {
            // Datos de demo como fallback
            var players = new List<PlayerView>
            {
                PlayerView.Neutral(),
                new(1, _aliasLocal, Color.Blue, 0),
                new(2, "Rival", Color.Red, 0)
            };
            _ui.SetPlayers(players);
        }
        public void ForzarActualizacionUI()
        {
            try
            {
                Console.WriteLine("🔄 GameController: Forzando actualización de UI...");
                
                if (_partida == null) 
                {
                    Console.WriteLine("❌ Partida es null");
                    return;
                }

                // Convertir jugadores
                var jugadores = _partida.Jugadores.ToArray();
                var playerViews = MapeoTerritorios.ConvertirJugadoresAPlayerView(jugadores);
                _ui.SetPlayers(playerViews);
                Console.WriteLine($"✅ {playerViews.Count} jugadores convertidos");

                // Convertir territorios
                var territorios = MapeoTerritorios.ConvertirMapaATerritorios(_partida.Mapa);
                _ui.SetTerritories(territorios);
                Console.WriteLine($"✅ {territorios.Count} territorios convertidos");

                // Actualizar jugador actual
                var jugadorActual = _partida.GetJugadorActual();
                if (jugadorActual != null)
                {
                    var playerView = playerViews.FirstOrDefault(p => p.Alias == jugadorActual.Alias);
                    if (playerView != null)
                    {
                        _ui.SetCurrentPlayer(playerView.Id);
                        _ui.SetReinforcements(jugadorActual.TropasDisponibles);
                        Console.WriteLine($"✅ Jugador actual: {jugadorActual.Alias} (ID: {playerView.Id})");
                    }
                }

                // Actualizar fase
                _ui.SetPhase(Phase.Reinforcements);
                Console.WriteLine("✅ Fase establecida a Refuerzos");

                Console.WriteLine("✅ UI actualizada forzadamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error forzando UI: {ex.Message}");
            }
        }
        

        #endregion

        #region Utilidades

        private void MostrarMensaje(string mensaje, string titulo)
        {
            MessageBox.Show(_ui, mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MostrarError(string mensaje)
        {
            MessageBox.Show(_ui, mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion

        #region Inicialización

        public void InicializarJugadores(string aliasJugador1, string colorJugador1, 
                                         string aliasJugador2, string colorJugador2)
        {
            var jugador1 = new Jugador(aliasJugador1, colorJugador1, 0);
            var jugador2 = new Jugador(aliasJugador2, colorJugador2, 0);
            var neutral = new EjercitoNeutral("Neutral", "#A0A0A0", 0);

            _partida.AddJugador(jugador1);
            _partida.AddJugador(jugador2);
            _partida.AddJugador(neutral);

            // Identificar jugador local
            _jugadorLocal = aliasJugador1 == _aliasLocal ? jugador1 : jugador2;

            ActualizarUI();
        }

        public void IniciarRed(int puerto, string ipServidor = null)
        {
            if (_esServidor)
            {
                _servidor = new Servidor(puerto, _partida);
                
                _partida.OnEventoJuego += (evento) =>
                {
                    string mensaje = $"{evento.Tipo}:{evento.Mensaje}";
                    _servidor?.Difundir(mensaje);
                };

                Task.Run(() => _servidor.Iniciar());
                
                Console.WriteLine($"Servidor iniciado en puerto {puerto}");
            }
            else
            {
                if (string.IsNullOrEmpty(ipServidor))
                    throw new ArgumentException("Se requiere IP del servidor.");

                _cliente = new Cliente();
                
                _cliente.OnMensajeRecibido += (msg) =>
                {
                    // Procesar en hilo de UI
                    try
                    {
                        if (_ui.InvokeRequired)
                        {
                            _ui.Invoke(new Action(() => ProcesarMensajeRed(msg)));
                        }
                        else
                        {
                            ProcesarMensajeRed(msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error invocando procesamiento: {ex.Message}");
                    }
                };

                // Conectar en hilo separado
                Task.Run(() => _cliente.Conectar(ipServidor, puerto));
                
                Console.WriteLine($"Cliente conectando a {ipServidor}:{puerto}");
            }
        }

        private void ProcesarMensajeRed(string mensaje)
        {
            try
            {
                Console.WriteLine($"[RED] Procesando: {mensaje}");
                
                if (mensaje.StartsWith("JUGADOR:"))
                {
                    // JUGADOR:Alias,Color,Tropas,CantTerritorios
                    var partes = mensaje.Substring(8).Split(',');
                    string alias = partes[0];
                    string color = partes[1];
                    int tropas = int.Parse(partes[2]);
                    
                    // Verificar si el jugador ya existe
                    var jugadorExistente = _partida.Jugadores.ToArray()
                        .FirstOrDefault(j => j.Alias == alias);
                    
                    if (jugadorExistente == null)
                    {
                        // Crear nuevo jugador
                        Jugador nuevoJugador;
                        if (alias.ToLower() == "neutral")
                            nuevoJugador = new EjercitoNeutral(alias, color, tropas);
                        else
                            nuevoJugador = new Jugador(alias, color, tropas);
                        
                        _partida.AddJugador(nuevoJugador);
                        Console.WriteLine($"  Jugador creado: {alias}");
                    }
                }
                else if (mensaje.StartsWith("TERRITORIO:"))
                {
                    // TERRITORIO:Nombre,Dueño,Tropas
                    var partes = mensaje.Substring(11).Split(',');
                    string nombreTerr = partes[0];
                    string dueno = partes[1];
                    int tropas = int.Parse(partes[2]);
                    
                    var territorio = _partida.Mapa.ObtenerTerritorioPorNombre(nombreTerr);
                    if (territorio != null)
                    {
                        // Asignar dueño
                        var jugadorDueno = _partida.Jugadores.ToArray()
                            .FirstOrDefault(j => j.Alias == dueno);
                        
                        if (jugadorDueno != null)
                        {
                            if (territorio.Dueno != jugadorDueno)
                            {
                                territorio.Dueno?.RemoveTerritorio(territorio);
                                jugadorDueno.AddTerritorio(territorio);
                            }
                        }
                        
                        // Asignar tropas
                        while (territorio.Tropas < tropas)
                            territorio.AddTropas(1);
                        
                        while (territorio.Tropas > tropas)
                            territorio.RemoveTropas(1);
                    }
                }
                else if (mensaje.StartsWith("TURNO_ACTUAL:"))
                {
                    var alias = mensaje.Substring(13);
                    Console.WriteLine($"  Turno actual: {alias}");
                }
                else if (mensaje.StartsWith("FASE_ACTUAL:"))
                {
                    var fase = mensaje.Substring(12);
                    Console.WriteLine($"  Fase actual: {fase}");
                }
                else if (mensaje.StartsWith("ESTADO_COMPLETO"))
                {
                    Console.WriteLine("  Estado inicial recibido completamente");
                    ActualizarUI();
                    _ui.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Conectado al servidor. Sincronización completada.", "Cliente");
                    }));
                }
                else if (mensaje.StartsWith("ACTUALIZAR_TERRITORIO:"))
                {
                    // ACTUALIZAR_TERRITORIO:Nombre,Dueño,Tropas
                    var partes = mensaje.Substring(22).Split(',');
                    string nombreTerr = partes[0];
                    string dueno = partes[1];
                    int tropas = int.Parse(partes[2]);
                    
                    var territorio = _partida.Mapa.ObtenerTerritorioPorNombre(nombreTerr);
                    if (territorio != null)
                    {
                        var jugadorDueno = _partida.Jugadores.ToArray()
                            .FirstOrDefault(j => j.Alias == dueno);
                        
                        if (jugadorDueno != null && territorio.Dueno != jugadorDueno)
                        {
                            territorio.Dueno?.RemoveTerritorio(territorio);
                            jugadorDueno.AddTerritorio(territorio);
                        }
                        
                        int diff = tropas - territorio.Tropas;
                        if (diff > 0)
                            territorio.AddTropas(diff);
                        else if (diff < 0)
                            territorio.RemoveTropas(-diff);
                    }
                    
                    ActualizarUI();
                }
                else if (mensaje.StartsWith("ACTUALIZAR_REFUERZOS:"))
                {
                    var partes = mensaje.Substring(21).Split(',');
                    string alias = partes[0];
                    int tropas = int.Parse(partes[1]);
                    
                    var jugador = _partida.Jugadores.ToArray()
                        .FirstOrDefault(j => j.Alias == alias);
                    
                    if (jugador != null)
                    {
                        int actual = jugador.TropasDisponibles;
                        if (tropas > actual)
                            jugador.AddTropasDisponibles(tropas - actual);
                        else if (tropas < actual)
                            jugador.RemoveTropasDisponibles(actual - tropas);
                    }
                    
                    ActualizarUI();
                }
                else if (mensaje.StartsWith("CAMBIO_TURNO:"))
                {
                    var partes = mensaje.Substring(13).Split(',');
                    ActualizarUI();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando mensaje: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }


        #endregion
    }
}