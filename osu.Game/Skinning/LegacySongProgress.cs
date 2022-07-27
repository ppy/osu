// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacySongProgress : CompositeDrawable, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private GameplayClock gameplayClock { get; set; }

        [Resolved(canBeNull: true)]
        private DrawableRuleset drawableRuleset { get; set; }

        [Resolved(canBeNull: true)]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        private double lastHitTime;
        private double firstHitTime;
        private double firstEventTime;
        private CircularProgress pie;

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(35);

            InternalChildren = new Drawable[]
            {
                pie = new CircularProgress
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f,
                },
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderColour = Colour4.White,
                    BorderThickness = 2,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Alpha = 0,
                    }
                },
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Colour4.White,
                    Size = new Vector2(3),
                }
            };

            firstEventTime = beatmap?.Value.Storyboard.EarliestEventTime ?? 0;

            if (drawableRuleset != null)
            {
                firstHitTime = drawableRuleset.Objects.First().StartTime;
                //TODO: this isn't always correct (consider mania where a non-last object may last for longer than the last in the list).
                lastHitTime = drawableRuleset.Objects.Last().GetEndTime() + 1;
            }
        }

        protected override void Update()
        {
            base.Update();

            double gameplayTime = gameplayClock?.CurrentTime ?? Time.Current;

            if (gameplayTime < firstHitTime)
            {
                pie.Scale = new Vector2(-1, 1);
                pie.Anchor = Anchor.TopRight;
                pie.Colour = Colour4.LimeGreen;
                pie.Current.Value = 1 - Math.Clamp((gameplayTime - firstEventTime) / (firstHitTime - firstEventTime), 0, 1);
            }
            else
            {
                pie.Scale = new Vector2(1);
                pie.Anchor = Anchor.TopLeft;
                pie.Colour = Colour4.White;
                pie.Current.Value = Math.Clamp((gameplayTime - firstHitTime) / (lastHitTime - firstHitTime), 0, 1);
            }
        }
    }
}
