// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public partial class EffectPointVisualisation : CompositeDrawable, IControlPointVisualisation
    {
        private readonly EffectControlPoint effect;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        public EffectPointVisualisation(EffectControlPoint point)
        {
            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Y;

            effect = point;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (beatmap.BeatmapInfo.Ruleset.CreateInstance().EditorShowScrollSpeed)
            {
                AddInternal(new ControlPointVisualisation(effect)
                {
                    // importantly, override the x position being set since we do that in the GroupVisualisation parent drawable.
                    X = 0,
                });
            }
        }

        public bool IsVisuallyRedundant(ControlPoint other) => other is EffectControlPoint;
    }
}
