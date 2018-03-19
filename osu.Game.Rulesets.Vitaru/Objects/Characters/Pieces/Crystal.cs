using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Graphics.Containers;
using OpenTK;

namespace osu.Game.Rulesets.Vitaru.Objects.Characters.Pieces
{
    public class Crystal : BeatSyncedContainer
    {
        public Crystal()
        {
            Alpha = 0;
            Child = new Sprite
            {
                Alpha = 0.8f,
                Scale = new Vector2((float)RNG.NextDouble(100, 200) / 300),
                Texture = VitaruRuleset.VitaruTextures.Get("crystal")
            };
        }

        public void Pop(double duration, Easing easing = Easing.OutQuart)
        {
            this.MoveTo(new Vector2((float)RNG.NextDouble(-200, 200), (float)RNG.NextDouble(-200, 200)), duration, easing)
                .FadeIn(duration / 8);
        }

        public void ReCollect(double duration, Easing easing = Easing.InQuart)
        {
            this.MoveTo(Vector2.Zero, duration, easing)
                .Delay(duration - duration / 8)
                .FadeOut(duration / 8);
        }
    }
}
