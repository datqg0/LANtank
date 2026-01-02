namespace gametankz.Network
{
    public class TankData
    {
        public int id { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public string Name { get; set; }
        public int hp { get; set; }
        public int dirX { get; set; } // hướng X
        public int dirY { get; set; } // hướng Y
        public int score { get; set; } // Điểm số
    }
}
