// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public partial class ModAutoHitsounds<T> : Mod, IApplicableToDrawableHitObject, IUpdatableByPlayfield, IApplicableToDrawableRuleset<T> where T : HitObject
    {
        public override string Name => "AutoHitsounds";

        public override string Acronym => "AH";

        public override LocalisableString Description => "Hitsounds to the music!";

        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.Automation;

        public override IconUsage? Icon => FontAwesome.Solid.Clone;

        public AutoHitsoundsContainer AutoHitsounds { get; private set; } = null!;

        [SettingSource("Hitsounds volume", "The volume of the hitsounds", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> HitsoundsVolume { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.01,
        };

        [SettingSource("Hitsounds offset", "The offset of the hitsounds", SettingControlType = typeof(IntegerSettingsSlider))]
        public BindableNumber<double> HitsoundsOffset { get; } = new BindableDouble(0)
        {
            MinValue = -500,
            MaxValue = 500,
            Precision = 1,
        };

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            drawable.PlaySamplesOnHit = false;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<T> drawableRuleset)
        {
            drawableRuleset.Audio.AddAdjustment(AdjustableProperty.Volume, HitsoundsVolume);
            drawableRuleset.Overlays.Add(AutoHitsounds = new AutoHitsoundsContainer());
        }

        public void Update(Playfield playfield)
        {
            foreach (var hitObject in playfield.AllHitObjects)
                UpdateObject(playfield, hitObject);
        }

        public void UpdateObject(Playfield playfield, DrawableHitObject hitObject)
        {
            double start = hitObject.HitObject.StartTime + HitsoundsOffset.Value;
            double currentTime = AutoHitsounds.ClockContainer.CurrentTime + AutoHitsounds.ClockContainer.GetUserOffset();
            if (start > currentTime - AutoHitsounds.ClockContainer.ElapsedFrameTime && start <= currentTime)
                hitObject.PlaySamples();

            foreach (var nestedHitObject in hitObject.NestedHitObjects)
                UpdateObject(playfield, nestedHitObject);
        }


        public partial class AutoHitsoundsContainer : Container
        {
            [Resolved]
            public GameplayClockContainer ClockContainer { get; private set; } = null!;
        }
    }
}
