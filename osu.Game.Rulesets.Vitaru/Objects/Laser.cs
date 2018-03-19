using OpenTK;

namespace osu.Game.Rulesets.Vitaru.Objects
{
    public class Laser : VitaruHitObject
    {
        public override HitObjectType Type => HitObjectType.Laser;

        /// <summary>
        /// Basically just bypasses all hitobject functionality (useful for player bullets)
        /// </summary>
        public bool DummyMode { get; set; }

        public double EndTime { get; set; }
        public float LaserDamage { get; set; } = 10;
        public Vector2 LaserSize { get; set; } = new Vector2(2, 8);
        public float LaserAngleRadian { get; set; }
        public int Team { get; set; } = -1;
    }
}
