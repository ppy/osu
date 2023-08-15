// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Edit
{
    public partial class DrawableManiaEditorRuleset : DrawableManiaRuleset
    {
        public readonly IBindable<TernaryState> ShowSpeedChanges = new Bindable<TernaryState>();

        public new IScrollingInfo ScrollingInfo => base.ScrollingInfo;

        public DrawableManiaEditorRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowSpeedChanges.BindValueChanged(state =>
            {
                VisualisationMethod = state.NewValue == TernaryState.True
                    ? ScrollVisualisationMethod.Sequential
                    : ScrollVisualisationMethod.Constant;
            }, true);
        }

        protected override Playfield CreatePlayfield() => new ManiaEditorPlayfield(Beatmap.Stages)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = Vector2.One
        };
    }
}
