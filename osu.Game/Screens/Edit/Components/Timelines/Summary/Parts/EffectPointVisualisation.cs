// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public class EffectPointVisualisation : CompositeDrawable, IControlPointVisualisation
    {
        private readonly EffectControlPoint effect;
        private Bindable<bool> kiai;

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        public EffectPointVisualisation(EffectControlPoint point)
        {
            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Y;

            effect = point;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            kiai = effect.KiaiModeBindable.GetBoundCopy();
            kiai.BindValueChanged(_ =>
            {
                ClearInternal();

                AddInternal(new ControlPointVisualisation(effect));

                if (!kiai.Value)
                    return;

                var endControlPoint = beatmap.ControlPointInfo.EffectPoints.FirstOrDefault(c => c.Time > effect.Time && !c.KiaiMode);

                // handle kiai duration
                // eventually this will be simpler when we have control points with durations.
                if (endControlPoint != null)
                {
                    RelativeSizeAxes = Axes.Both;
                    Origin = Anchor.TopLeft;

                    Width = (float)(endControlPoint.Time - effect.Time);

                    AddInternal(new PointVisualisation
                    {
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.TopLeft,
                        Width = 1,
                        Height = 0.25f,
                        Depth = float.MaxValue,
                        Colour = effect.GetRepresentingColour(colours).Darken(0.5f),
                    });
                }
            }, true);
        }

        // kiai sections display duration, so are required to be visualised.
        public bool IsVisuallyRedundant(ControlPoint other) => other is EffectControlPoint otherEffect && effect.KiaiMode == otherEffect.KiaiMode;
    }
}
