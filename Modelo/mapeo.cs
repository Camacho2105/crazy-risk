using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CrazyRisk.Modelo;

namespace WinFormsApp1
{
    //
    /// Mapper que conecta los IDs numéricos (1-42) de la UI hexagonal
    /// con los nombres reales de territorios de la lógica del juego
    ///
    public static class MapeoTerritorios
    {
        // Diccionario: ID Visual (1-42) -> Nombre Real del Territorio
        private static readonly Dictionary<int, string> IdANombre = new Dictionary<int, string>
        {
            // América del Norte (6 territorios) - IDs 1-6
            { 1, "Groenlandia" },
            { 2, "Canada" },
            { 3, "Estados Unidos" },
            { 4, "Mexico" },
            { 5, "Costa Rica" },
            { 6, "Panama" },
            
            // América del Sur (4 territorios) - IDs 7-10
            { 7, "Brasil" },
            { 8, "Argentina" },
            { 9, "Chile" },
            { 10, "Uruguay" },
            
            // Europa (10 territorios) - IDs 11-20
            { 11, "Reino Unido" },
            { 12, "Portugal" },
            { 13, "España" },
            { 14, "Francia" },
            { 15, "Paises bajo" },
            { 16, "Alemania" },
            { 17, "Polonia" },
            { 18, "Italia" },
            { 19, "Croacia" },
            { 20, "Hungría" },
            
            // África (6 territorios) - IDs 21-26
            { 21, "Marruecos" },
            { 22, "Egipto" },
            { 23, "Nigeria" },
            { 24, "Congo" },
            { 25, "Kenia" },
            { 26, "Sudafrica" },
            
            // Asia (13 territorios) - IDs 27-39
            { 27, "Turquia" },
            { 28, "Iran" },
            { 29, "Arabia Saudita" },
            { 30, "Qatar" },
            { 31, "Rusia" },
            { 32, "Mongolia" },
            { 33, "China" },
            { 34, "India" },
            { 35, "Corea del Norte" },
            { 36, "Corea del Sur" },
            { 37, "Japon" },
            { 38, "Singapur" },
            { 39, "Indonesia" },
            
            // Oceanía (3 territorios) - IDs 40-42
            { 40, "Australia" },
            { 41, "Papua Nueva Guinea" },
            { 42, "Nueva Zelanda" }
        };

        // Diccionario inverso para búsquedas rápidas
        private static readonly Dictionary<string, int> NombreAId = 
            IdANombre.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        // Mapeo de códigos de continente (UI) a nombres completos (Lógica)
        private static readonly Dictionary<string, string> CodigoContinenteANombre = new Dictionary<string, string>
        {
            { "NA", "America del Norte" },
            { "SA", "America del Sur" },
            { "EU", "europa" },
            { "AF", "africa" },
            { "AS", "asia" },
            { "OC", "Oceania" }
        };
        private static readonly Color[] _paleta = new[]
        {
            Color.FromArgb(52, 136, 245),   // Azul
            Color.FromArgb(234, 85, 69),    // Rojo
            Color.FromArgb(76, 175, 80),    // Verde
            Color.FromArgb(255, 193, 7),    // Amarillo
            Color.FromArgb(156, 39, 176),   // Púrpura
            Color.FromArgb(255, 87, 34),    // Naranja
            Color.FromArgb(0, 188, 212),    // Cian
            Color.FromArgb(233, 30, 99)     // Rosa
        };

        // Asignar continente según ID para la UI hexagonal
        private static string ObtenerCodigoContinente(int id)
        {
            if (id >= 1 && id <= 6) return "NA";      // América del Norte
            if (id >= 7 && id <= 10) return "SA";     // América del Sur
            if (id >= 11 && id <= 20) return "EU";    // Europa
            if (id >= 21 && id <= 26) return "AF";    // África
            if (id >= 27 && id <= 39) return "AS";    // Asia
            if (id >= 40 && id <= 42) return "OC";    // Oceanía
            return "??";
        }

        //
        /// Convierte un ID numérico (1-42) al nombre real del territorio
        ///
        public static string ObtenerNombreTerritorio(int id)
        {
            return IdANombre.TryGetValue(id, out string nombre) ? nombre : $"Territorio_{id}";
        }

        //
        /// Convierte un nombre real de territorio al ID numérico (1-42)
        ///
        public static int ObtenerIdTerritorio(string nombre)
        {
            return NombreAId.TryGetValue(nombre, out int id) ? id : -1;
        }

        // Método auxiliar para obtener ID consistente de jugador
        // ✅ Método auxiliar para ID consistente
        private static int ObtenerIdJugador(Jugador jugador)
        {
            if (jugador == null) return 0;
            
            if (jugador is EjercitoNeutral || jugador.Alias.ToLower().Contains("neutral"))
                return 0;
            
            // Hash CONSISTENTE del alias
            return (Math.Abs(jugador.Alias.GetHashCode()) % 99) + 1;
        }
        //
        /// Convierte un Territorio de lógica a TerritoryView de UI
        ///
        public static TerritoryView ConvertirATerritoryView(Territorio territorio)
        {
            if (territorio == null) return null;

            int id = ObtenerIdTerritorio(territorio.Nombre);
            if (id == -1) id = Math.Abs(territorio.Nombre.GetHashCode()) % 42 + 1;

            int ownerId = 0; // neutral por defecto
            if (territorio.Dueno != null && _aliasToId.TryGetValue(territorio.Dueno.Alias, out var pid))
                ownerId = pid;

            string codigoContinente = ObtenerCodigoContinente(id);

            return new TerritoryView(id, territorio.Nombre, codigoContinente, ownerId, territorio.Tropas);
        }
        private static Dictionary<string,int> _aliasToId = new(StringComparer.OrdinalIgnoreCase);

       // En MapeoTerritorios.ConvertirAPlayerView, modifica la generación de IDs:
       public static PlayerView ConvertirAPlayerView(Jugador jugador)
        {
            if (jugador == null) return null;

            int id = ObtenerIdJugador(jugador);

            Color color = id == 0 ? Color.Gray : _paleta[(id - 1) % _paleta.Length];
            return new PlayerView(id, jugador.Alias, color, jugador.GetCantCartas());
        }

        //
        /// Convierte todos los territorios del mapa a TerritoryView para la UI
        ///
       public static List<TerritoryView> ConvertirMapaATerritorios(Mapa mapa, Jugador[] jugadores)
        {
            var res = new List<TerritoryView>();
            if (mapa == null) return res;

            foreach (var t in mapa.GetTerritorios())
            {
                int id = ObtenerIdTerritorio(t.Nombre);

                // Buscar índice del jugador dueño dentro del arreglo de jugadores
                int ownerId = 0; // Neutral por defecto
                if (t.Dueno != null)
                {
                    int idx = Array.FindIndex(jugadores, j => j.Alias == t.Dueno.Alias);
                    ownerId = (idx >= 0) ? idx : 0;
                }

                res.Add(new TerritoryView(id, t.Nombre, t.Continente, ownerId, t.Tropas));
            }

            return res;
        }


        //
        /// Convierte todos los jugadores a PlayerView para la UI
        ///
        public static List<PlayerView> ConvertirJugadoresAPlayerView(Jugador[] jugadores)
        {
            var res = new List<PlayerView>();
            int id = 0;

            foreach (var j in jugadores)
            {
                Color color;
                if (j is EjercitoNeutral)
                    color = Color.Gray;
                else
                    color = _paleta[id % _paleta.Length]; // 🎨 paleta local

                res.Add(new PlayerView(id, j.Alias, color, j.GetCantCartas()));
                id++;
            }

            return res;
        }


        //
        /// Obtiene el código de continente de la UI a partir del nombre completo de la lógica
        ///
        public static string ObtenerCodigoContinenteDesdeNombre(string nombreCompleto)
        {
            var codigo = CodigoContinenteANombre.FirstOrDefault(kvp => 
                kvp.Value.Equals(nombreCompleto, StringComparison.OrdinalIgnoreCase));
            
            return codigo.Key ?? "??";
        }

        //
        /// Valida que todos los territorios de la lógica tengan mapeo en la UI
        ///
        public static bool ValidarMapeo(Mapa mapa)
        {
            var territorios = mapa.GetTerritorios();
            int sinMapeo = 0;

            foreach (var territorio in territorios)
            {
                int id = ObtenerIdTerritorio(territorio.Nombre);
                if (id == -1)
                {
                    Console.WriteLine($"⚠️ ADVERTENCIA: Territorio '{territorio.Nombre}' no tiene mapeo a ID");
                    sinMapeo++;
                }
            }

            if (sinMapeo > 0)
            {
                Console.WriteLine($"❌ {sinMapeo} territorios sin mapeo de {territorios.Length} totales");
                return false;
            }

            Console.WriteLine($"✓ Todos los {territorios.Length} territorios tienen mapeo correcto");
            return true;
        }
    }
}