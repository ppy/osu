// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public sealed class BpmDisplay : ModsEffectDisplay
    {
        protected override LocalisableString Label => "Average BPM";

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        private ModSettingChangeTracker? settingChangeTracker;
        private readonly OsuSpriteText dashText;

        public BpmDisplay()
        {
            Add(dashText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold),
                Text = "-"
            });
        }

        protected override void LoadComplete()
        {
            mods.BindValueChanged(m =>
            {
                settingChangeTracker?.Dispose();

                refresh();

                settingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                settingChangeTracker.SettingChanged += _ => refresh();
            }, true);
            working.ValueChanged += _ => refresh();
            base.LoadComplete();
        }

        /// <summary>
        /// Refreshes counter and background color.
        /// </summary>
        private void refresh()
        {
            var beatmap = working.Value.Beatmap;

            if (beatmap == null)
                return;

            double rate = 1;
            foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                rate = mod.ApplyToRate(0, rate);

            double beatLength = beatmap.GetMostCommonBeatLength();

            if (beatLength == 0d)
            {
                Counter.FadeOut();
                dashText.FadeIn();
                return;
            }

            double bpm = (60000 / beatLength);
            double scaledBpm = bpm * rate;

            dashText.FadeOut();
            Counter.FadeIn();
            Current.Default = bpm;
            Current.Value = scaledBpm;
        }
    }
}
