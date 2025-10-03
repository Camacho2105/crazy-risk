using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WinFormsApp1
{
    // ====== Estado general (solo UI)
    public enum Phase { Lobby, Setup, Reinforcements, Attacks, Fortify, GameOver }

    // ====== Jugadores (solo UI)
    public record PlayerView(int Id, string Alias, Color Color, int CardsCount)
    {
        public static PlayerView Neutral() => new(0, "Neutral", Color.FromArgb(160, 160, 160), 0);
        public bool IsNeutral => Id == 0;
    }

    // ====== Territorios (solo UI)
    public record TerritoryView(int Id, string Name, string Continent, int OwnerId, int Troops);

    // ====== Eventos de inicio/conexión (solo UI)
    public record HostRequestArgs(string Alias, int Port);
    public record JoinRequestArgs(string Alias, string Host, int Port);

    // ====== Descripción del mapa (solo UI)
    public class TerritoryShape
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Continent { get; set; } = string.Empty;
        public float BorderWidth { get; set; } = 2.0f;
        public List<PointF> Points { get; set; } = new();
    }

    public record Bridge(int FromId, int ToId);

    public record MapLayout(List<TerritoryShape> Shapes, List<Bridge> Bridges);

    // ====== Geometría precalculada por territorio (solo UI)
    internal record GraphicsPathInfo(GraphicsPath Path, PointF Centroid, RectangleF Bounds);
}
