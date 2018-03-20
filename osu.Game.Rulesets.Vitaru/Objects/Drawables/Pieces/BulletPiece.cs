using osu.Framework.Graphics;
using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Framework.Extensions.Color4Extensions;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables.Pieces
{
    public class BulletPiece : BeatSyncedContainer
    {
        private readonly Characters.Characters currentCharacter = VitaruSettings.VitaruConfigManager.GetBindable<Characters.Characters>(VitaruSetting.Characters);
        private readonly GraphicsPresets currentSkin = VitaruSettings.VitaruConfigManager.GetBindable<GraphicsPresets>(VitaruSetting.GraphicsPresets);

        private Sprite bulletKiai;
        private CircularContainer circle;
        private Box box;

        private readonly float randomRotationValue = 1;
        private readonly bool randomRotateDirection;

        private readonly DrawableBullet drawableBullet;

        public BulletPiece(DrawableBullet drawableBullet)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            this.drawableBullet = drawableBullet;

            randomRotationValue = (float)RNG.Next(10, 15) / 10;
            randomRotateDirection = RNG.NextBool();
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (currentSkin != GraphicsPresets.HighPerformanceCompetitive && currentSkin != GraphicsPresets.HighPerformance)
            {
                if (effectPoint.KiaiMode && bulletKiai.Alpha == 0)
                    bulletKiai.FadeInFromZero(timingPoint.BeatLength / 4);
                if (!effectPoint.KiaiMode && bulletKiai.Alpha == 1)
                    bulletKiai.FadeOutFromOne(timingPoint.BeatLength);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (currentSkin != GraphicsPresets.HighPerformanceCompetitive && currentSkin != GraphicsPresets.HighPerformance && bulletKiai.Alpha > 0)
            {
                if (randomRotateDirection)
                    bulletKiai.RotateTo((float)(Clock.CurrentTime / 1000 * 90) * randomRotationValue);
                else
                    bulletKiai.RotateTo((float)(Clock.CurrentTime / 1000 * 90) * -1 * randomRotationValue);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Size = new Vector2(drawableBullet.Bullet.BulletDiameter + 12);

            if (currentSkin != GraphicsPresets.HighPerformanceCompetitive && currentSkin != GraphicsPresets.HighPerformance)
                Child = bulletKiai = new Sprite
                {
                    //Just to look nice for the time being, will fix the sprite later
                    Scale = new Vector2(2),
                    Alpha = 0,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = drawableBullet.Bullet.ComboColour,
                    Texture = VitaruRuleset.VitaruTextures.Get("bulletKiai"),
                };

            Add(circle = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 1,
                RelativeSizeAxes = Axes.Both,
                BorderColour = drawableBullet.Bullet.ComboColour,
                BorderThickness = 6,
                Masking = true,

                Child = box = new Box
                {
                    RelativeSizeAxes = Axes.Both
                }
            });

            if (currentSkin != GraphicsPresets.HighPerformanceCompetitive && currentSkin != GraphicsPresets.HighPerformance)
                circle.EdgeEffect = new EdgeEffectParameters
                {
                    Radius = drawableBullet.Bullet.BulletDiameter,
                    Type = EdgeEffectType.Shadow,
                    Colour = drawableBullet.Bullet.ComboColour.Opacity(0.2f)
                };

            if (drawableBullet.Bullet.Ghost && currentCharacter == Characters.Characters.YuyukoSaigyouji | currentCharacter == Characters.Characters.AliceMuyart)
                box.Colour = Color4.Cyan;
        }
    }
}
