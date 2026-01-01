using System.Collections.Generic;

namespace gametankz.Network
{
    public class GameState
    {
        public List<TankData> tanks { get; set; } = new();
        public List<BulletData> bullets { get; set; } = new();
        public List<HealthData> healths{ get; set; } = new();
    }
}
