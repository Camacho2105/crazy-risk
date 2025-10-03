using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using CrazyRisk.Modelo;
using WinFormsApp1;

namespace CrazyRisk.Comunicacion
{
    public class Servidor
    {
        private TcpListener listener;
        private bool enEjecucion = false;
        private List<TcpClient> clientes = new List<TcpClient>();
        private Partida partida;

        public Servidor(int puerto, Partida partida)
        {
            listener = new TcpListener(IPAddress.Any, puerto);
            this.partida = partida;

            // Suscribir eventos de la partida para avisar a clientes
            this.partida.OnEventoJuego += (evento) =>
            {
                Difundir($"[{evento.Tipo}] {evento.Mensaje}");
            };
        }

        public void Iniciar()
        {
            try
            {
                listener.Start();
                enEjecucion = true;
                Console.WriteLine($"Servidor escuchando en puerto {((IPEndPoint)listener.LocalEndpoint).Port}...");

                while (enEjecucion)
                {
                    TcpClient cliente = listener.AcceptTcpClient();
                    lock (clientes) clientes.Add(cliente);
                    Thread hiloCliente = new Thread(() => ManejarCliente(cliente));
                    hiloCliente.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el servidor: {ex.Message}");
            }
            finally
            {
                listener.Stop();
                Console.WriteLine("Servidor detenido.");
            }
        }
        
        private void ManejarCliente(TcpClient cliente)
        {
            try
            {
                Console.WriteLine("Cliente conectado!");
                NetworkStream stream = cliente.GetStream();

                // Mensaje de bienvenida
                Enviar(cliente, "Bienvenido a CrazyRisk!\nComandos: JOIN, TROPA, ATACAR, ENDTURN");

                // Enviar estado completo de la partida al cliente recién conectado
                EnviarEstadoCompleto(cliente);

                byte[] buffer = new byte[1024];
                while (cliente.Connected)
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0) break;

                    string mensaje = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                    Console.WriteLine($"[CLIENTE] {mensaje}");

                    ProcesarComando(cliente, mensaje);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error con cliente: {ex.Message}");
            }
            finally
            {
                cliente.Close();
                lock (clientes) clientes.Remove(cliente);
                Console.WriteLine("Cliente desconectado.");
            }
        }
        private void EnviarEstadoCompleto(TcpClient cliente)
        {
            try
            {
                Console.WriteLine("Enviando estado completo al cliente...");
                
                // 1. Enviar jugadores
                foreach (var j in partida.Jugadores.ToArray())
                {
                    string msg = $"JUGADOR:{j.Alias},{j.Color},{j.TropasDisponibles},{j.GetCantTerritorios()}";
                    Enviar(cliente, msg);
                    Console.WriteLine($"  Enviado: {msg}");
                }
                
                // 2. Enviar todos los territorios
                foreach (var t in partida.Mapa.GetTerritorios())
                {
                    string dueno = t.Dueno?.Alias ?? "Neutral";
                    string msg = $"TERRITORIO:{t.Nombre},{dueno},{t.Tropas}";
                    Enviar(cliente, msg);
                }
                
                // 3. Enviar turno actual
                var actual = partida.GetJugadorActual();
                string msgTurno = $"TURNO_ACTUAL:{actual.Alias}";
                Enviar(cliente, msgTurno);
                Console.WriteLine($"  Enviado: {msgTurno}");
                
                // 4. Enviar fase actual
                string msgFase = $"FASE_ACTUAL:{partida.Estado}";
                Enviar(cliente, msgFase);
                Console.WriteLine($"  Enviado: {msgFase}");
                
                // 5. Señal de fin de sincronización
                Enviar(cliente, "ESTADO_COMPLETO");
                Console.WriteLine("Estado completo enviado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando estado: {ex.Message}");
            }
        }

        // Procesar comandos enviados por cliente
        private void ProcesarComando(TcpClient cliente, string comando)
        {
            try
            {
                if (comando.StartsWith("SOLICITAR_ESTADO"))
                {
                    EnviarEstadoCompleto(cliente);
                    return;
                }
                if (comando.StartsWith("TROPA:"))
                {
                    var partes = comando.Substring(6).Split(',');
                    int territoryId = int.Parse(partes[0]);
                    int cantidad = int.Parse(partes[1]);

                    string nombreTerr = MapeoTerritorios.ObtenerNombreTerritorio(territoryId);
                    var territorio = partida.Mapa.ObtenerTerritorioPorNombre(nombreTerr);
                    var jugador = partida.GetJugadorActual();

                    if (territorio != null && jugador != null)
                    {
                        int colocadas = 0;
                        for (int i = 0; i < cantidad; i++)
                        {
                            if (partida.ColocarTropa(jugador, territorio))
                                colocadas++;
                            else
                                break;
                        }

                        // Difundir cambio a TODOS los clientes
                        if (colocadas > 0)
                        {
                            Difundir($"ACTUALIZAR_TERRITORIO:{territorio.Nombre},{territorio.Dueno.Alias},{territorio.Tropas}");
                            Difundir($"ACTUALIZAR_REFUERZOS:{jugador.Alias},{jugador.TropasDisponibles}");
                        }
                    }
                }
                else if (comando.StartsWith("ATACAR:"))
                {
                    var partes = comando.Substring(7).Split(',');
                    int origenId = int.Parse(partes[0]);
                    int destinoId = int.Parse(partes[1]);
                    int dadosAtk = int.Parse(partes[2]);

                    string nombreOrigen = MapeoTerritorios.ObtenerNombreTerritorio(origenId);
                    string nombreDestino = MapeoTerritorios.ObtenerNombreTerritorio(destinoId);

                    var origen = partida.Mapa.ObtenerTerritorioPorNombre(nombreOrigen);
                    var destino = partida.Mapa.ObtenerTerritorioPorNombre(nombreDestino);

                    if (origen != null && destino != null)
                    {
                        bool conquistado = partida.RealizarAtaque(origen, destino, dadosAtk);

                        // Difundir resultado
                        Difundir($"ACTUALIZAR_TERRITORIO:{origen.Nombre},{origen.Dueno.Alias},{origen.Tropas}");
                        Difundir($"ACTUALIZAR_TERRITORIO:{destino.Nombre},{destino.Dueno.Alias},{destino.Tropas}");

                        if (conquistado)
                        {
                            Difundir($"CONQUISTA:{origen.Nombre},{destino.Nombre}");
                        }
                    }
                }
                else if (comando.StartsWith("ENDTURN"))
                {
                    // === MÁQUINA DE ESTADOS SINCRONIZADA CON GameController ===
                    var actual = partida.GetJugadorActual();
                    bool esCliente = string.Equals(actual.Alias, "Cliente", StringComparison.OrdinalIgnoreCase);

                    switch (partida.Estado)
                    {
                        case EstadoJuego.Refuerzos:
                            if (!esCliente)
                            {
                                // Servidor en Refuerzos -> pasa turno al Cliente en Refuerzos
                                partida.AvanzarTurno();
                                partida.CambiarEstado(EstadoJuego.Refuerzos);
                            }
                            else
                            {
                                // Cliente en Refuerzos -> vuelve Servidor en Ataques
                                partida.AvanzarTurno();
                                partida.CambiarEstado(EstadoJuego.Ataques);
                            }
                            break;

                        case EstadoJuego.Ataques:
                            // Ataques -> Planeación (mismo jugador)
                            partida.CambiarEstado(EstadoJuego.Planeacion);
                            break;

                        case EstadoJuego.Planeacion:
                            if (!esCliente)
                            {
                                // Servidor termina Planeación -> pasa Cliente en Ataques
                                partida.AvanzarTurno();
                                partida.CambiarEstado(EstadoJuego.Ataques);
                            }
                            else
                            {
                                // Cliente termina Planeación -> vuelve Servidor en Refuerzos
                                partida.AvanzarTurno();
                                partida.CambiarEstado(EstadoJuego.Refuerzos);
                            }
                            break;
                    }

                    // Difundir nuevo turno y fase (para mantener clientes alineados)
                    var nuevo = partida.GetJugadorActual();
                    Difundir($"CAMBIO_TURNO:{nuevo.Alias},{nuevo.TropasDisponibles}");
                    Difundir($"FASE_ACTUAL:{partida.Estado}");
                }
                else if (comando.StartsWith("FORTIFY:"))
                {
                    var partes = comando.Substring(8).Split(',');
                    int origenId = int.Parse(partes[0]);
                    int destinoId = int.Parse(partes[1]);
                    int cantidad = int.Parse(partes[2]);

                    string nombreOrigen = MapeoTerritorios.ObtenerNombreTerritorio(origenId);
                    string nombreDestino = MapeoTerritorios.ObtenerNombreTerritorio(destinoId);

                    var origen = partida.Mapa.ObtenerTerritorioPorNombre(nombreOrigen);
                    var destino = partida.Mapa.ObtenerTerritorioPorNombre(nombreDestino);
                    var jugador = partida.GetJugadorActual();

                    if (origen != null && destino != null && jugador != null)
                    {
                        partida.MoverTropas(jugador, origen, destino, cantidad);

                        // Difundir cambios
                        Difundir($"ACTUALIZAR_TERRITORIO:{origen.Nombre},{origen.Dueno.Alias},{origen.Tropas}");
                        Difundir($"ACTUALIZAR_TERRITORIO:{destino.Nombre},{destino.Dueno.Alias},{destino.Tropas}");
                    }
                }
            }
            catch (Exception ex)
            {
                Enviar(cliente, $"Error: {ex.Message}");
                Console.WriteLine($"Error procesando comando: {ex.Message}");
            }
        }

        // Enviar mensaje a un cliente
        private void Enviar(TcpClient cliente, string mensaje)
        {
            try
            {
                if (cliente.Connected)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(mensaje + "\n");
                    cliente.GetStream().Write(buffer, 0, buffer.Length);
                }
            }
            catch { }
        }

        // Difundir mensaje a todos los clientes
        public void Difundir(string mensaje)
        {
            lock (clientes)
            {
                foreach (var c in clientes)
                {
                    Enviar(c, mensaje);
                }
            }
            Console.WriteLine($"[SERVER->TODOS] {mensaje}");
        }

        public void Detener()
        {
            enEjecucion = false;
            listener.Stop();
            lock (clientes)
            {
                foreach (var c in clientes) c.Close();
                clientes.Clear();
            }
        }
    }
}
