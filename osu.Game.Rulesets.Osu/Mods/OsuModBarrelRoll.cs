// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModBarrelRoll : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToDrawableHitObjects
    {
        private float currentRotation;

        [SettingSource("滚动速度", "每分钟多少转")]
        public BindableNumber<double> SpinSpeed { get; } = new BindableDouble(0.5)
        {
            MinValue = 0.02,
            MaxValue = 12,
            Precision = 0.01,
        };

        [SettingSource("方向", "旋转方向")]
        public Bindable<RotationDirection> Direction { get; } = new Bindable<RotationDirection>(RotationDirection.Clockwise);

        public override string Name => "滚筒";
        public override string Acronym => "BR";
        public override string Description => "或许你需要旋转屏幕";
        public override double ScoreMultiplier => 1;

        public override string SettingDescription => $"{SpinSpeed.Value} rpm {Direction.Value.GetDescription().ToLowerInvariant()}";

        public void Update(Playfield playfield)
        {
            playfield.Rotation = currentRotation = (Direction.Value == RotationDirection.Counterclockwise ? -1 : 1) * 360 * (float)(playfield.Time.Current / 60000 * SpinSpeed.Value);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // scale the playfield to allow all hitobjects to stay within the visible region.
            drawableRuleset.Playfield.Scale = new Vector2(OsuPlayfield.BASE_SIZE.Y / OsuPlayfield.BASE_SIZE.X);
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables)
            {
                d.OnUpdate += _ =>
                {
                    switch (d)
                    {
                        case DrawableHitCircle circle:
                            circle.CirclePiece.Rotation = -currentRotation;
                            break;
                    }
                };
            }
        }
    }
}
