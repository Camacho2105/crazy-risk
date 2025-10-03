using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CrazyRisk.Comunicacion
{
    public class Cliente
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool conectado = false;

        public event Action<string> OnMensajeRecibido;

        public void Conectar(string ip, int puerto)
        {
            try
            {
                Console.WriteLine("Cliente intentando conectar...");
                client = new TcpClient(ip, puerto);
                stream = client.GetStream();
                conectado = true;
                Console.WriteLine("Conectado al servidor.");

                // Iniciar hilo para escuchar mensajes
                Thread hiloEscucha = new Thread(EscucharServidor);
                hiloEscucha.IsBackground = true; // Importante: hilo en background
                hiloEscucha.Start();
                
                Console.WriteLine("Hilo de escucha iniciado");
                
                // ✅ NO bloquear aquí, mantener conexión abierta
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar: {ex.Message}");
                conectado = false;
            }
        }

        private void EscucharServidor()
        {
            try
            {
                byte[] buffer = new byte[4096]; // Aumentar buffer
                while (conectado && client != null && client.Connected)
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0)
                    {
                        Console.WriteLine("Servidor cerró la conexión");
                        break;
                    }
                    
                    string mensaje = Encoding.UTF8.GetString(buffer, 0, bytes);
                    
                    // Manejar múltiples mensajes si vienen juntos
                    string[] mensajes = mensaje.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var msg in mensajes)
                    {
                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            Console.WriteLine($"[Servidor] {msg}");
                            OnMensajeRecibido?.Invoke(msg.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en hilo de escucha: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Hilo de escucha terminado");
            }
        }

        public void Enviar(string mensaje)
        {
            try
            {
                if (conectado && client.Connected)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(mensaje + "\n");
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar: {ex.Message}");
            }
        }

        public void Desconectar()
        {
            if (!conectado) return;
            conectado = false;
            stream?.Close();
            client?.Close();
            Console.WriteLine("Cliente desconectado.");
        }
    }
}