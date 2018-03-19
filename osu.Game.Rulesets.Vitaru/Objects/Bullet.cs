namespace osu.Game.Rulesets.Vitaru.Objects
{
    public class Bullet : VitaruHitObject
    {
        public override HitObjectType Type => HitObjectType.Bullet;

        /// <summary>
        /// Basically just bypasses all hitobject functionality (useful for player bullets)
        /// </summary>
        public bool DummyMode { get; set; }

        public float BulletDamage { get; set; } = 10;
        public float BulletSpeed { get; set; } = 1f;
        public float BulletDiameter { get; set; } = 16f;
        public float BulletAngleRadian { get; set; }
        public bool DynamicBulletVelocity { get; set; }
        // ReSharper disable once UnusedMember.Global
        public bool Piercing { get; set; } = false;
        public int Team { get; set; } = -1;
        public bool ShootPlayer { get; set; }
        public bool ObeyBoundries { get; } = true;
        public bool Ghost { get; set; } = false;
    }
}
