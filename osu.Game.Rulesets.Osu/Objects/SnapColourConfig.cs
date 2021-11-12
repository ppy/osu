// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Screens.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SnapColourfetch
    {
        public SnapColourfetch colourHolder
        {
            get => colourHolder;
            set
            {
                colourHolder = value;
            }
        }
        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved(canBeNull: true)]
        private IBeatmap beatmap { get; set; }
        private readonly Bindable<bool> configTimingBasedNoteColouring = new Bindable<bool>();
        
        private void load(OsuRulesetConfigManager rulesetConfig)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.TimingBasedNoteColouring, configTimingBasedNoteColouring);
        }
        private void updatecolourHolder()
        {
            if (beatmap == null || HitObject == null) return;
            int snapDivisor = beatmap.ControlPointInfo.GetClosestBeatDivisor(HitObject.StartTime);
            colourHolder = configTimingBasedNoteColouring.Value ? BindableBeatDivisor.GetColourFor(snapDivisor, colours) : Color4.White;
        }
    }
}

