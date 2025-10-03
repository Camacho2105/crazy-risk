using System;
using System.Linq;
using CrazyRisk.Datos;  

namespace CrazyRisk.Modelo
{
    // ====== EVENTOS Y NOTIFICACIONES ======
    public class EventoJuego
    {
        private string tipo;
        private string mensaje;
        private object datos;
        private DateTime timestamp;

        public string Tipo => tipo;
        public string Mensaje => mensaje;
        public object Datos => datos;
        public DateTime Timestamp => timestamp;

        public EventoJuego(string tipo, string mensaje, object datos = null)
        {
            this.tipo = tipo;
            this.mensaje = mensaje;
            this.datos = datos;
            this.timestamp = DateTime.Now;
        }
    }

    public class ResultadoCombate
    {
        private int[] dadosAtacante;
        private int[] dadosDefensor;
        private int bajasAtacante;
        private int bajasDefensor;
        private bool territorioConquistado;
        private string nombreTerritorio;
        private string atacanteNombre;
        private string defensorNombre;

        public int[] DadosAtacante => dadosAtacante;
        public int[] DadosDefensor => dadosDefensor;
        public int BajasAtacante => bajasAtacante;
        public int BajasDefensor => bajasDefensor;
        public bool TerritorioConquistado => territorioConquistado;
        public string NombreTerritorio => nombreTerritorio;
        public string AtacanteNombre => atacanteNombre;
        public string DefensorNombre => defensorNombre;

        public ResultadoCombate(int[] dadosAtacante, int[] dadosDefensor, int bajasAtacante, int bajasDefensor,
            bool territorioConquistado, string nombreTerritorio, string atacanteNombre, string defensorNombre)
        {
            this.dadosAtacante = dadosAtacante;
            this.dadosDefensor = dadosDefensor;
            this.bajasAtacante = bajasAtacante;
            this.bajasDefensor = bajasDefensor;
            this.territorioConquistado = territorioConquistado;
            this.nombreTerritorio = nombreTerritorio;
            this.atacanteNombre = atacanteNombre;
            this.defensorNombre = defensorNombre;
        }
    }

    public class EstadoTropas
    {
        private string jugadorNombre;
        private string territorioNombre;
        private int tropasAnteriores;
        private int tropasNuevas;
        private int cambio;

        public string JugadorNombre => jugadorNombre;
        public string TerritorioNombre => territorioNombre;
        public int TropasAnteriores => tropasAnteriores;
        public int TropasNuevas => tropasNuevas;
        public int Cambio => cambio;

        public EstadoTropas(string jugadorNombre, string territorioNombre, int tropasAnteriores, int tropasNuevas, int cambio)
        {
            this.jugadorNombre = jugadorNombre;
            this.territorioNombre = territorioNombre;
            this.tropasAnteriores = tropasAnteriores;
            this.tropasNuevas = tropasNuevas;
            this.cambio = cambio;
        }
    }

    public class InfoCarta
    {
        private string tipoCarta;
        private string jugadorNombre;
        private string razon;
        private bool puedeRecibirMas;
        private int cartasActuales;

        public string TipoCarta => tipoCarta;
        public string JugadorNombre => jugadorNombre;
        public string Razon => razon;
        public bool PuedeRecibirMas => puedeRecibirMas;
        public int CartasActuales => cartasActuales;

        public InfoCarta(string tipoCarta, string jugadorNombre, string razon, bool puedeRecibirMas, int cartasActuales)
        {
            this.tipoCarta = tipoCarta;
            this.jugadorNombre = jugadorNombre;
            this.razon = razon;
            this.puedeRecibirMas = puedeRecibirMas;
            this.cartasActuales = cartasActuales;
        }
    }

    // ====== DELEGADOS PARA EVENTOS ======
    public delegate void EventoJuegoHandler(EventoJuego evento);

    // ====== ENUMS ======
    public enum EstadoJuego
    {
        Preparacion,
        Refuerzos,
        Ataques,
        Planeacion,
        Finalizado
    }

    public enum TipoEvento
    {
        COMBATE,
        CONQUISTA,
        CARTA_RECIBIDA,
        CARTA_RECHAZADA,
        TROPAS_MOVIDAS,
        REFUERZOS_CALCULADOS,
        INTERCAMBIO_CARTAS,
        VICTORIA,
        ERROR,
        TURNO_CAMBIADO,
        ESTADO_CAMBIADO
    }

    // ====== CARTAS (Herencia) ======
    public abstract class Carta
    {
        public abstract string Tipo { get; }
        public abstract int Poder { get; }
    }

    public class CartaInfanteria : Carta
    {
        public override string Tipo => "Infantería";
        public override int Poder => 1;
    }

    public class CartaCaballeria : Carta
    {
        public override string Tipo => "Caballería";
        public override int Poder => 5;
    }

    public class CartaArtilleria : Carta
    {
        public override string Tipo => "Artillería";
        public override int Poder => 10;
    }

    // ====== DADO ======
    public class Dado
    {
        private static Random rnd = new Random();
        public static int Tirar() => rnd.Next(1, 7); // Número entre 1 y 6
    }

    // ====== JUGADOR ======
    public class Jugador    
    {
        private string alias;
        private string color;
        private MyLinkedList<Territorio> territorios;
        private MyLinkedList<Carta> cartas;
        private int tropasDisponibles;

        public string Alias => alias;
        public string Color => color;
        public int TropasDisponibles => tropasDisponibles;

        public Jugador(string alias, string color, int tropasIniciales)
        {
            this.alias = alias;
            this.color = color;
            this.territorios = new MyLinkedList<Territorio>();
            this.cartas = new MyLinkedList<Carta>();
            this.tropasDisponibles = tropasIniciales;
        }

        // TERRITORIOS
        public void AddTerritorio(Territorio t)
        {
            if (!territorios.Contains(t)) territorios.AddLast(t);
            t.SetDueno(this);
        }

        public void RemoveTerritorio(Territorio t)
        {
            if (territorios.Remove(t)) t.SetDueno(null);
        }

        public Territorio[] GetTerritorios() => territorios.ToArray();
        public int GetCantTerritorios() => territorios.Count;

        // CARTAS - Con validación de límite
        public void AddCarta(Carta c) 
        {
            // Si ya tiene 5 cartas y recibe la 6ta, DEBE intercambiar
            if (cartas.Count >= 5) 
            {
                throw new InvalidOperationException($"CARTA_LIMITE_EXCEDIDO:{alias}:{cartas.Count}");
            }
            cartas.AddLast(c);
        }

        public bool RemoveCarta(Carta c) => cartas.Remove(c);
        
        public Carta[] GetCartas() => cartas.ToArray();
        
        public int GetCantCartas() => cartas.Count;

        public bool DebeIntercambiarCartas() => cartas.Count >= 5;
        
        public bool PuedeRecibirCarta() => cartas.Count < 5;

        // TROPAS
        public void AddTropasDisponibles(int n) => tropasDisponibles += n;
        
        public bool RemoveTropasDisponibles(int n)
        {
            if (n <= 0 || tropasDisponibles - n < 0) return false;
            tropasDisponibles -= n;
            return true;
        }
    }

    // JugadorIA hereda de Jugador
    public class JugadorIA : Jugador
    {
        public JugadorIA(string alias, string color, int tropasIniciales)
            : base(alias, color, tropasIniciales) { }

        public virtual void JugarTurno()
        {
            // Lógica de IA sin mensajes de consola
        }
    }

    public class EjercitoNeutral : JugadorIA
    {
        public EjercitoNeutral(string alias, string color, int tropasIniciales)
            : base(alias, color, tropasIniciales) { }

        public void ColocarTropasAleatoriamente()
        {
            var terrs = GetTerritorios();
            if (terrs.Length == 0 || TropasDisponibles <= 0) return;

            Random rnd = new Random();
            int tropasRestantes = TropasDisponibles;

            foreach (var t in terrs)
            {
                if (tropasRestantes <= 0) break;
                
                int maxTropas = Math.Min(tropasRestantes, rnd.Next(1, 4));
                if (RemoveTropasDisponibles(maxTropas))
                {
                    t.AddTropas(maxTropas);
                    tropasRestantes -= maxTropas;
                }
            }
        }

        public override void JugarTurno()
        {
            ColocarTropasAleatoriamente();
        }
    }

    // ====== ENTIDADES DEL MAPA (Herencia) ======
    public abstract class EntidadMapa
    {
        private string nombre;
        public string Nombre => nombre;
        
        protected EntidadMapa(string nombre) 
        { 
            this.nombre = nombre; 
        }
    }

    public class Territorio : EntidadMapa
    {
        private Jugador dueno;
        private int tropas;
        private MyLinkedList<Territorio> adyacentes;
        private string continente;

        public Jugador Dueno => dueno;
        public int Tropas => tropas;
        public string Continente => continente;

        public Territorio(string nombre, string continente) : base(nombre)
        {
            this.continente = continente;
            this.dueno = null;
            this.tropas = 0;
            this.adyacentes = new MyLinkedList<Territorio>();
        }

        // ADYACENTES
        public void AgregarAdyacente(Territorio t)
        {
            if (t != null && t != this && !adyacentes.Contains(t))
            {
                adyacentes.AddLast(t);
                // Agregar conexión bidireccional
                if (!t.adyacentes.Contains(this))
                    t.adyacentes.AddLast(this);
            }
        }

        public Territorio[] GetAdyacentes() => adyacentes.ToArray();
        public bool EsAdyacente(Territorio t) => adyacentes.Contains(t);

        // DUENO
        public void SetDueno(Jugador j) => dueno = j;

        // TROPAS
        public void AddTropas(int n)
        {
            if (n > 0) tropas += n;
        }

        public bool RemoveTropas(int n)
        {
            if (n <= 0 || tropas - n < 0) return false;
            tropas -= n;
            return true;
        }
    }

    public class Continente : EntidadMapa
    {
        private int bono;
        private MyLinkedList<Territorio> territorios;

        public int Bono => bono;

        public Continente(string nombre, int bono) : base(nombre)
        {
            this.bono = bono;
            this.territorios = new MyLinkedList<Territorio>();
        }

        public void AddTerritorio(Territorio t)
        {
            if (!territorios.Contains(t)) territorios.AddLast(t);
        }

        public Territorio[] GetTerritorios() => territorios.ToArray();

        public bool EsControladoPor(Jugador j)
        {
            var terrs = territorios.ToArray();
            if (terrs.Length == 0) return false;
            
            foreach (var t in terrs)
                if (t.Dueno != j) return false;
            return true;
        }
    }

    // ====== MAPA ======
    public class Mapa
    {
        private MyLinkedList<Continente> continentes;
        private MyLinkedList<Territorio> territorios;

        public Mapa()
        {
            this.continentes = new MyLinkedList<Continente>();
            this.territorios = new MyLinkedList<Territorio>();
        }

        public void AddContinente(Continente c) => continentes.AddLast(c);
        public void AddTerritorio(Territorio t) => territorios.AddLast(t);

        public Territorio ObtenerTerritorioPorNombre(string nombre)
        {
            foreach (var t in territorios.ToArray())
                if (t.Nombre == nombre) return t;
            return null;
        }

        public Territorio[] GetTerritorios() => territorios.ToArray();
        public Continente[] GetContinentes() => continentes.ToArray();

        public bool ExisteRutaControlada(Jugador j, Territorio origen, Territorio destino)
        {
            if (origen == null || destino == null) return false;
            if (origen == destino) return true;
            if (origen.Dueno != j || destino.Dueno != j) return false;

            var visited = new MyLinkedList<Territorio>();
            var queue = new MyQueue<Territorio>();
            queue.Enqueue(origen);
            visited.AddLast(origen);

            while (!queue.IsEmpty())
            {
                var cur = queue.Dequeue();
                foreach (var a in cur.GetAdyacentes())
                {
                    if (visited.Contains(a)) continue;
                    if (a.Dueno != j) continue;
                    if (a == destino) return true;

                    visited.AddLast(a);
                    queue.Enqueue(a);
                }
            }
            return false;
        }
    }

    // ====== PARTIDA ======
    public class Partida
    {
        private MyLinkedList<Jugador> jugadores;
        private Mapa mapa;
        private EstadoJuego estado;
        private int contadorFibonacci;
        private int turnoActual;
        private static Random rnd = new Random(); 

        // PROPIEDADES DE SOLO LECTURA
        public MyLinkedList<Jugador> Jugadores => jugadores;
        public Mapa Mapa => mapa;
        public EstadoJuego Estado => estado;

        // EVENTO PARA NOTIFICAR A LA INTERFAZ
        public event EventoJuegoHandler OnEventoJuego;

        public Partida(Mapa mapa)
        {
            this.mapa = mapa;
            this.jugadores = new MyLinkedList<Jugador>();
            this.contadorFibonacci = 0;
            this.turnoActual = 0;
            this.estado = EstadoJuego.Preparacion;
        }

        // MÉTODO PARA DISPARAR EVENTOS
        private void DispararEvento(string tipo, string mensaje, object datos = null)
        {
            OnEventoJuego?.Invoke(new EventoJuego(tipo, mensaje, datos));
        }

        public void AddJugador(Jugador j) => jugadores.AddLast(j);
        public void PrepararPartida()
        {
            // 1. Repartir territorios
            AsignarTerritoriosAleatorio();

            // 2. Dar tropas iniciales según reglas (40 - territorios)
            PrepararEjercitosIniciales();

            // 3. Cambiar a fase de refuerzos (sin bloquear)
            estado = EstadoJuego.Refuerzos;
            
            DispararEvento(TipoEvento.ESTADO_CAMBIADO.ToString(),
                "Partida iniciada. Coloca tus refuerzos.",
                new { Estado = estado });
        }

        public bool ColocarTropa(Jugador j, Territorio t)
        {
            if (t.Dueno != j) return false;
            
            if (j.RemoveTropasDisponibles(1))
            {
                int tropasAnteriores = t.Tropas;
                t.AddTropas(1);
                
                var estadoTropas = new EstadoTropas(j.Alias, t.Nombre, tropasAnteriores, t.Tropas, 1);
                
                DispararEvento(TipoEvento.TROPAS_MOVIDAS.ToString(), 
                    $"{j.Alias} coloca 1 tropa en {t.Nombre}", estadoTropas);
                return true;
            }
            return false;
        }
        public void ColocacionInicial()
        {
            // Solo coloca tropas para EjercitoNeutral. La lógica de humanos será interactiva en Program.cs
            bool quedanTropas = true;
            while (quedanTropas)
            {
                quedanTropas = false;
                foreach (var j in jugadores.ToArray())
                {
                    if (j.TropasDisponibles > 0)
                    {
                        if (j is EjercitoNeutral neutral)
                        {
                            neutral.ColocarTropasAleatoriamente();
                        }
                        else
                        {

                        }
                        quedanTropas = true;
                    }
                }
            }
            estado = EstadoJuego.Refuerzos;
        }


        public void AsignarTerritoriosAleatorio()
        {
            var terrs = mapa.GetTerritorios();

            // Mezclar territorios (algoritmo Fisher-Yates)
            for (int i = terrs.Length - 1; i > 0; i--)
            {
                int k = rnd.Next(0, i + 1);
                var tmp = terrs[i];
                terrs[i] = terrs[k];
                terrs[k] = tmp;
            }

            // Repartir round-robin entre jugadores
            var players = jugadores.ToArray();
            int idx = 0;
            foreach (var t in terrs)
            {
                var j = players[idx % players.Length];
                j.AddTerritorio(t);
                t.AddTropas(1);
                j.RemoveTropasDisponibles(1);
                idx++;
            }
        }
        public void PrepararEjercitosIniciales()
        {
            foreach (var j in jugadores.ToArray())
            {
                int usados = j.GetCantTerritorios(); // ya colocó 1 en cada territorio
                j.AddTropasDisponibles(40 - usados); // quedan disponibles para colocación
            }
        }
        public void ColocarRefuerzosNeutral()
        {
            var neutral = jugadores.FirstOrDefault(j => j is EjercitoNeutral);
            if (neutral == null || neutral.TropasDisponibles <= 0) return;

            var rnd = new Random();
            var territoriosNeutral = neutral.GetTerritorios();
            int tropasRestantes = neutral.TropasDisponibles;

            while (tropasRestantes > 0 && territoriosNeutral.Length > 0)
            {
                var territorio = territoriosNeutral[rnd.Next(territoriosNeutral.Length)];
                territorio.AddTropas(1);
                tropasRestantes--;
            }

            // Resetear tropas disponibles a 0
            neutral.RemoveTropasDisponibles(neutral.TropasDisponibles);

            DispararEvento(TipoEvento.REFUERZOS_CALCULADOS.ToString(),
                $"{neutral.Alias} colocó todas sus tropas iniciales",
                new { Jugador = neutral.Alias });
        }


        // ====== TURNOS ======
        public Jugador GetJugadorActual() => jugadores.ToArray()[turnoActual];
        
        public void AvanzarTurno()
        {
            if (jugadores.Count == 0) return;

            var arr = jugadores.ToArray();
            do
            {
                turnoActual = (turnoActual + 1) % arr.Length;
            }
            while (arr[turnoActual] is EjercitoNeutral);

            Jugador actual = arr[turnoActual];

            if (RequiereIntercambioForzoso(actual))
            {
                DispararEvento(TipoEvento.ERROR.ToString(),
                    $"¡ATENCIÓN! {actual.Alias} debe intercambiar cartas obligatoriamente antes de continuar.",
                    new { Jugador = actual.Alias });
                return;
            }

            DispararEvento(TipoEvento.TURNO_CAMBIADO.ToString(),
                $"Turno de {actual.Alias}", actual);

            // ❌ Quita la parte que daba refuerzos aquí
            // ✅ En su lugar, marca el estado en Refuerzos
            CambiarEstado(EstadoJuego.Refuerzos);
            if (actual is JugadorIA ia)
            {
                ia.JugarTurno();
            }
            // Los refuerzos se calculan y agregan SOLO cuando la UI/jugador entra en fase de Refuerzos
        }



        public void EjecutarFasesTurno()
        {
            var actual = GetJugadorActual();

            // Fase Refuerzos
            CambiarEstado(EstadoJuego.Refuerzos);
            int refuerzos = CalcularRefuerzos(actual);
            actual.AddTropasDisponibles(refuerzos);
            DispararEvento(TipoEvento.REFUERZOS_CALCULADOS.ToString(),
                $"{actual.Alias} tiene {refuerzos} refuerzos disponibles",
                new { Jugador = actual.Alias, Refuerzos = refuerzos });

            // Fase Ataques
            CambiarEstado(EstadoJuego.Ataques);
            DispararEvento(TipoEvento.ESTADO_CAMBIADO.ToString(),
                $"{actual.Alias} entra en fase de Ataques",
                new { Jugador = actual.Alias });
            // Aquí solo marca el estado.

            // Fase Planeación
            CambiarEstado(EstadoJuego.Planeacion);
            DispararEvento(TipoEvento.ESTADO_CAMBIADO.ToString(),
                $"{actual.Alias} entra en fase de Planeación",
                new { Jugador = actual.Alias });

            // Terminar turno
            AvanzarTurno();
        }


        public void CambiarEstado(EstadoJuego nuevoEstado)
        {
            var estadoAnterior = estado;
            estado = nuevoEstado;

            DispararEvento(TipoEvento.ESTADO_CAMBIADO.ToString(),
                $"Estado cambió de {estadoAnterior} a {nuevoEstado}",
                new { Anterior = estadoAnterior, Nuevo = nuevoEstado });
        }

        // ====== REFUERZOS ======
        public int CalcularRefuerzos(Jugador j)
        {
            int baseRef = Math.Max(3, j.GetCantTerritorios() / 3); // mínimo 3
            int bonus = 0;
            foreach (var c in mapa.GetContinentes())
                if (c.EsControladoPor(j)) bonus += c.Bono;
            return baseRef + bonus;
        }

        // ====== CANJE DE CARTAS (Con secuencia Fibonacci corregida) ======
        public int CanjearCartas(Jugador j, Carta[] seleccionadas)
        {
            if (seleccionadas == null || seleccionadas.Length != 3) return 0;

            bool todasInf = seleccionadas.All(c => c is CartaInfanteria);
            bool todasCab = seleccionadas.All(c => c is CartaCaballeria);
            bool todasArt = seleccionadas.All(c => c is CartaArtilleria);
            bool unaDeCada = seleccionadas.Select(c => c.Tipo).Distinct().Count() == 3;

            if (!(todasInf || todasCab || todasArt || unaDeCada))
                return 0;

            // Eliminar las cartas seleccionadas de la mano del jugador
            foreach (var c in seleccionadas)
                j.RemoveCarta(c);

            contadorFibonacci++;
            int tropas = Fibonacci(contadorFibonacci);
            j.AddTropasDisponibles(tropas);
            
            DispararEvento(TipoEvento.INTERCAMBIO_CARTAS.ToString(), 
                $"{j.Alias} intercambia cartas por {tropas} tropas", 
                new { Jugador = j.Alias, Tropas = tropas, Intercambio = contadorFibonacci });
            
            return tropas;
        }

        private int Fibonacci(int n)
        {
            // Secuencia del proyecto: 2, 3, 5, 8, 13, 21, 34, 55...
            if (n == 1) return 2;  // Primer intercambio = 2 tropas
            if (n == 2) return 3;  // Segundo intercambio = 3 tropas
            
            int a = 2, b = 3;
            for (int i = 3; i <= n; i++)
            {
                int temp = a + b;
                a = b;
                b = temp;
            }
            return b;
        }

        public bool RequiereIntercambioForzoso(Jugador j)
        {
            return j.GetCantCartas() >= 6;
        }

        // ====== Generar carta aleatoria ======
        private Carta GenerarCartaAleatoria()
        {
            int r = rnd.Next(0, 3);
            return r switch
            {
                0 => new CartaInfanteria(),
                1 => new CartaCaballeria(),
                _ => new CartaArtilleria(),
            };
        }

        private void DarCartaSiPuede(Jugador atacante, string territorioConquistado)
        {
            var infoCarta = new InfoCarta(
                null, 
                atacante.Alias, 
                $"Conquistar {territorioConquistado}",
                atacante.PuedeRecibirCarta(),
                atacante.GetCantCartas()
            );

            if (atacante.PuedeRecibirCarta())
            {
                Carta carta = GenerarCartaAleatoria();
                atacante.AddCarta(carta);
                
                var infoCartaCompleta = new InfoCarta(
                    carta.Tipo,
                    atacante.Alias,
                    $"Conquistar {territorioConquistado}",
                    atacante.PuedeRecibirCarta(),
                    atacante.GetCantCartas()
                );
                
                DispararEvento(TipoEvento.CARTA_RECIBIDA.ToString(), 
                    $"{atacante.Alias} recibe carta {carta.Tipo} por conquistar {territorioConquistado}", 
                    infoCartaCompleta);
            }
            else
            {
                DispararEvento(TipoEvento.CARTA_RECHAZADA.ToString(), 
                    $"{atacante.Alias} no puede recibir más cartas. Debe intercambiar primero.", 
                    infoCarta);
                
                throw new InvalidOperationException($"INTERCAMBIO_FORZOSO:{atacante.Alias}");
            }
        }

        // ====== ATAQUE (Mejorado con eventos) ======
        public bool RealizarAtaque(Territorio origen, Territorio destino, int tropasAtaque)
        {
            // Validaciones de ataque
            if (origen == null || destino == null)
                throw new ArgumentException("Los territorios no pueden ser nulos.");
            
            if (origen.Dueno == null)
                throw new InvalidOperationException("El territorio origen debe tener un dueño.");
            
            if (origen.Dueno == destino.Dueno)
                throw new InvalidOperationException("No puedes atacar tus propios territorios.");
            
            if (!origen.EsAdyacente(destino)) 
                throw new InvalidOperationException("Solo puedes atacar territorios adyacentes.");
            
            if (origen.Tropas <= 1) 
                throw new InvalidOperationException("Necesitas al menos 2 tropas para atacar.");
            
            if (tropasAtaque <= 0 || tropasAtaque > 3 || tropasAtaque >= origen.Tropas)
                throw new ArgumentException("Número de tropas de ataque inválido.");

            // Determinar dados
            int atacanteDados = Math.Min(3, tropasAtaque);
            int defensorDados = Math.Min(2, destino.Tropas);

            // Tirar dados
            int[] dadosAtacante = new int[atacanteDados];
            int[] dadosDefensor = new int[defensorDados];

            for (int i = 0; i < atacanteDados; i++) dadosAtacante[i] = Dado.Tirar();
            for (int i = 0; i < defensorDados; i++) dadosDefensor[i] = Dado.Tirar();

            // Ordenar de mayor a menor
            Array.Sort(dadosAtacante); Array.Reverse(dadosAtacante);
            Array.Sort(dadosDefensor); Array.Reverse(dadosDefensor);

            // Resolver combate
            int comparaciones = Math.Min(dadosAtacante.Length, dadosDefensor.Length);
            int bajasAtacante = 0, bajasDefensor = 0;

            for (int i = 0; i < comparaciones; i++)
            {
                if (dadosAtacante[i] > dadosDefensor[i]) 
                    bajasDefensor++;
                else 
                    bajasAtacante++;
            }

            // Aplicar bajas
            origen.RemoveTropas(bajasAtacante);
            destino.RemoveTropas(bajasDefensor);

            // Crear resultado del combate para la interfaz
            var resultado = new ResultadoCombate(
                dadosAtacante,
                dadosDefensor,
                bajasAtacante,
                bajasDefensor,
                destino.Tropas == 0,
                destino.Nombre,
                origen.Dueno.Alias,
                destino.Dueno.Alias
            );

            DispararEvento(TipoEvento.COMBATE.ToString(), 
                $"Combate: {origen.Dueno.Alias} vs {destino.Dueno.Alias} en {destino.Nombre}", 
                resultado);

            // Verificar conquista
            if (destino.Tropas == 0)
            {
                return ConquistarTerritorio(origen, destino, tropasAtaque);
            }
            
            return false;
        }

        private bool ConquistarTerritorio(Territorio origen, Territorio destino, int tropasAtaque)
        {
            Jugador atacante = origen.Dueno;
            Jugador defensor = destino.Dueno;

            // Remover territorio del defensor y dárselo al atacante
            defensor.RemoveTerritorio(destino);
            atacante.AddTerritorio(destino);

            // Mover tropas (mínimo las que atacaron)
            int tropasAMover = Math.Min(tropasAtaque, origen.Tropas - 1);
            origen.RemoveTropas(tropasAMover);
            destino.AddTropas(tropasAMover);

            DispararEvento(TipoEvento.CONQUISTA.ToString(), 
                $"{atacante.Alias} conquista {destino.Nombre} de {defensor.Alias}", 
                new { Atacante = atacante.Alias, Defensor = defensor.Alias, Territorio = destino.Nombre, TropasMovidas = tropasAMover });

            // Intentar dar carta al atacante
            try 
            {
                DarCartaSiPuede(atacante, destino.Nombre);
            }
            catch (InvalidOperationException ex)
            {
                // El jugador debe intercambiar cartas antes de continuar
                DispararEvento(TipoEvento.ERROR.ToString(), 
                    $"¡ATENCIÓN! {atacante.Alias} debe intercambiar cartas obligatoriamente antes de continuar.", 
                    new { Jugador = atacante.Alias, Error = ex.Message });
                throw;
            }

            return true;
        }

        // ====== MOVIMIENTO DE TROPAS (Fase Planeación) ======
        public bool MoverTropas(Jugador j, Territorio origen, Territorio destino, int tropas)
        {
            if (origen.Dueno != j || destino.Dueno != j)
                throw new InvalidOperationException("Solo puedes mover tropas entre tus territorios");
            
            if (origen.Tropas <= tropas)
                throw new InvalidOperationException("Debes dejar al menos 1 tropa en el territorio origen");
            
            if (!mapa.ExisteRutaControlada(j, origen, destino))
                throw new InvalidOperationException("No hay ruta controlada entre los territorios");
            
            origen.RemoveTropas(tropas);
            destino.AddTropas(tropas);
            
            DispararEvento(TipoEvento.TROPAS_MOVIDAS.ToString(), 
                $"{j.Alias} mueve {tropas} tropas de {origen.Nombre} a {destino.Nombre}", 
                new { Jugador = j.Alias, Origen = origen.Nombre, Destino = destino.Nombre, Tropas = tropas });
            
            return true;
        }

        //VICTORIA
        public Jugador VerificarVictoria()
        {
            foreach (var j in jugadores.ToArray())
            {
                if (j.GetCantTerritorios() == mapa.GetTerritorios().Length) 
                {
                    estado = EstadoJuego.Finalizado;
                    DispararEvento(TipoEvento.VICTORIA.ToString(), 
                        $"¡{j.Alias} ha conquistado el mundo!", 
                        new { Ganador = j.Alias, TerritoriosConquistados = j.GetCantTerritorios() });
                    return j;
                }
            }
            return null;
        }
    }
}