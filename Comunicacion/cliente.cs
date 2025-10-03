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

                Thread hiloEscucha = new Thread(EscucharServidor);
                hiloEscucha.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar: {ex.Message}");
            }
        }

        private void EscucharServidor()
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (conectado && client.Connected)
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0) break;
                    string mensaje = Encoding.UTF8.GetString(buffer, 0, bytes);

                    Console.WriteLine("\n[Servidor] " + mensaje);
                    OnMensajeRecibido?.Invoke(mensaje);
                }
            }
            catch
            {
                Console.WriteLine("Conexión perdida con el servidor.");
            }
            finally
            {
                Desconectar();
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