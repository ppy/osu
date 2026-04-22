// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModSynesthesia : ModSynesthesia, IApplicableToBeatmap, IApplicableToDrawableHitObject
    {
        private const float triangle_width = 13f;
        private const float triangle_height = 7.5f;

        /// <summary>
        /// Gap from hit content top/bottom edge to the outside of the triangle (before half-height).
        /// </summary>
        private const float content_edge_to_triangle_margin = 4f;

        private static readonly OsuColour colours = new OsuColour();

        private IBeatmap? currentBeatmap { get; set; }

        [SettingSource("Position", "Show snap indicators above, below, or on both sides.")]
        public Bindable<IndicatorPosition> Position { get; } = new Bindable<IndicatorPosition>(IndicatorPosition.Both);

        public override LocalisableString Description => "Add rhythm indicators around hit objects.";

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (currentBeatmap != beatmap)
                currentBeatmap = beatmap;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (currentBeatmap == null || drawable is not DrawableTaikoHitObject taiko)
                return;

            switch (drawable)
            {
                case DrawableDrumRoll drumRoll:
                {
                    var overlay = new SynesthesiaLayerOverlay(
                        Position,
                        new[]
                        {
                            new SnapIndicatorBlock(),
                            new SnapIndicatorBlock(),
                        },
                        new[]
                        {
                            () => drumRoll.HitObject.StartTime,
                            () => drumRoll.HitObject.EndTime,
                        });
                    registerOverlay(taiko, overlay);
                    return;
                }

                case DrawableHit hit:
                {
                    var overlay = new SynesthesiaLayerOverlay(
                        Position,
                        new[] { new SnapIndicatorBlock() },
                        new[]
                        {
                            () => hit.HitObject.StartTime,
                        });
                    registerOverlay(taiko, overlay);
                    return;
                }

                default:
                    return;
            }
        }

        private void registerOverlay(DrawableTaikoHitObject taiko, SynesthesiaLayerOverlay overlay)
        {
            taiko.HitObjectApplied += _ => overlay.RefreshColour(currentBeatmap!);
            taiko.OnUpdate += _ => overlay.RefreshColour(currentBeatmap!);
            taiko.AddModContentOverlay(overlay);
        }

        private static void applySnapColour(Drawable drawable, int divisor) =>
            drawable.Colour = BindableBeatDivisor.GetColourFor(divisor, colours);

        private sealed partial class SnapIndicatorBlock : CompositeDrawable
        {
            private readonly Triangle indicatorAboveNote;
            private readonly Triangle indicatorBelowNote;

            public SnapIndicatorBlock()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                AddRangeInternal(new Drawable[]
                {
                    indicatorAboveNote = new Triangle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(triangle_width, triangle_height),
                        EdgeSmoothness = new Vector2(1.5f),
                        Rotation = 180,
                    },
                    indicatorBelowNote = new Triangle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(triangle_width, triangle_height),
                        EdgeSmoothness = new Vector2(1.5f),
                        Rotation = 0,
                    },
                });
            }

            public void SetPlacement(IndicatorPosition position)
            {
                switch (position)
                {
                    case IndicatorPosition.Both:
                        indicatorAboveNote.Alpha = 1;
                        indicatorBelowNote.Alpha = 1;
                        break;

                    case IndicatorPosition.Top:
                        indicatorAboveNote.Alpha = 1;
                        indicatorBelowNote.Alpha = 0;
                        break;

                    case IndicatorPosition.Bottom:
                        indicatorAboveNote.Alpha = 0;
                        indicatorBelowNote.Alpha = 1;
                        break;
                }
            }

            /// <summary>
            /// Place triangle centres at <c>±(contentHalfHeight + margin + triHalf)</c> from the note centre (local Y=0).
            /// </summary>
            public void LayoutForContentHalfHeight(float contentHalfHeight)
            {
                float offsetFromNoteCentreY = contentHalfHeight + content_edge_to_triangle_margin + triangle_height / 2f;
                indicatorAboveNote.Y = -offsetFromNoteCentreY;
                indicatorBelowNote.Y = +offsetFromNoteCentreY;
            }

            public void RefreshColour(int divisor)
            {
                applySnapColour(indicatorAboveNote, divisor);
                applySnapColour(indicatorBelowNote, divisor);
            }
        }

        private sealed partial class SynesthesiaLayerOverlay : CompositeDrawable
        {
            private readonly Bindable<IndicatorPosition> position;
            private readonly SnapIndicatorBlock[] blocks;
            private readonly Func<double>[] getSnapTimes;

            public SynesthesiaLayerOverlay(Bindable<IndicatorPosition> position, SnapIndicatorBlock[] blocks, Func<double>[] getSnapTimes)
            {
                this.position = position;
                this.blocks = blocks;
                this.getSnapTimes = getSnapTimes;

                RelativeSizeAxes = Axes.Both;

                AddRangeInternal(blocks);
            }

            protected override void Update()
            {
                base.Update();
                if (DrawHeight <= 0)
                    return;

                if (blocks.Length > 1 && DrawWidth <= 0)
                    return;

                float contentHalf = DrawHeight / 2f;
                var placement = position.Value;

                for (int i = 0; i < blocks.Length; i++)
                {
                    blocks[i].X = blocks.Length == 1 ? 0f : (i == 0 ? -DrawWidth / 2f : DrawWidth / 2f);
                    blocks[i].Y = 0f;
                    blocks[i].LayoutForContentHalfHeight(contentHalf);
                    blocks[i].SetPlacement(placement);
                }
            }

            public void RefreshColour(IBeatmap beatmap)
            {
                for (int i = 0; i < blocks.Length; i++)
                {
                    int divisor = beatmap.ControlPointInfo.GetClosestBeatDivisor(getSnapTimes[i]());
                    blocks[i].RefreshColour(divisor);
                }
            }
        }

        public enum IndicatorPosition
        {
            /// <summary>
            /// Indicators above and below the note.
            /// </summary>
            Both = 0,

            /// <summary>
            /// Only the indicator above the note.
            /// </summary>
            Top = 1,

            /// <summary>
            /// Only the indicator below the note.
            /// </summary>
            Bottom = 2,
        }
    }
}
