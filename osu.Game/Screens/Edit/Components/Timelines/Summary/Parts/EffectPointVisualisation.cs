// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public partial class EffectPointVisualisation : CompositeDrawable, IControlPointVisualisation
    {
        private readonly EffectControlPoint effect;
        private Bindable<bool> kiai = null!;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

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
            kiai.BindValueChanged(_ => refreshDisplay(), true);
        }

        private EffectControlPoint? nextControlPoint;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Due to the limitations of ControlPointInfo, it's impossible to know via event flow when the next kiai point has changed.
            // This is due to the fact that an EffectPoint can be added to an existing group. We would need to bind to ItemAdded on *every*
            // future group to track this.
            //
            // I foresee this being a potential performance issue on beatmaps with many control points, so let's limit how often we check
            // for changes. ControlPointInfo needs a refactor to make this flow better, but it should do for now.
            Scheduler.AddDelayed(() =>
            {
                EffectControlPoint? next = null;

                for (int i = 0; i < beatmap.ControlPointInfo.EffectPoints.Count; i++)
                {
                    var point = beatmap.ControlPointInfo.EffectPoints[i];

                    if (point.Time > effect.Time)
                    {
                        next = point;
                        break;
                    }
                }

                if (!ReferenceEquals(nextControlPoint, next))
                {
                    nextControlPoint = next;
                    refreshDisplay();
                }
            }, 100, true);
        }

        private void refreshDisplay()
        {
            ClearInternal();

            AddInternal(new ControlPointVisualisation(effect));

            if (!kiai.Value)
                return;

            // handle kiai duration
            // eventually this will be simpler when we have control points with durations.
            if (nextControlPoint != null)
            {
                RelativeSizeAxes = Axes.Both;
                Origin = Anchor.TopLeft;

                Width = (float)(nextControlPoint.Time - effect.Time);

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
        }

        // kiai sections display duration, so are required to be visualised.
        public bool IsVisuallyRedundant(ControlPoint other) => other is EffectControlPoint otherEffect && effect.KiaiMode == otherEffect.KiaiMode;
    }
}
