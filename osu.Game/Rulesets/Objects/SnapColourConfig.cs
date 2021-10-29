// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Configuration

namespace osu.Game.Rulesets.Objects
{
    // <summary>
    // Holds the logic responsible for the correct application of timing based note colouring in osu! and osu!mania
    // </summary>
    public class SnapColourfetch
    {
        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved(canBeNull: true)]
        private IBeatmap beatmap { get; set; }
        private readonly Bindable<bool> configTimingBasedNoteColouring = new Bindable<bool>();

         [BackgroundDependencyLoader]

        private void load(OsuRulesetConfigManager rulesetConfig)
        {
        rulesetConfig?.BindWith(OsuRulesetSetting.TimingBasedNoteColouring, configTimingBasedNoteColouring);

        }
    }
}

