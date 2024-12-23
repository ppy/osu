// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class TrackTick : GridContainer
    {
        public HitObject HitObject;

        public TrackTick(HitObject hitObject)
        {
            hitObject.StartTimeBindable.BindValueChanged(v =>
            {
                X = (float)v.NewValue;
            }, true);

            HitObject = hitObject;
            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            RowDimensions = [
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Distributed),
            ];
            ColumnDimensions = [
                new Dimension(GridSizeMode.Distributed),
            ];
            Content = new[]
            {
                new[]
                {
                    createTick(),
                },
                new[]
                {
                    createTick(),
                },
                new[]
                {
                    createTick(),
                },
            };
        }

        private Drawable createTick()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = OsuIcon.FilledCircle,
                    Enabled = { Value = true },
                }
            };
        }
    }

    public partial class TrackTicksDisplay : TimelinePart<TrackTick>
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            editorBeatmap.HitObjectRemoved += (HitObject hitObject) =>
            {
                Children.First(tick => tick.HitObject == hitObject).Expire();
            };
            editorBeatmap.HitObjectAdded += (HitObject hitObject) =>
            {
                Add(new TrackTick(hitObject));
            };
            recreateTicks();
        }

        protected override void Update()
        {
            base.Update();
        }

        private void recreateTicks()
        {
            Clear();

            List<TrackTick> ticks = [];

            editorBeatmap.HitObjects.ForEach(hitObject =>
            {
                ticks.Add(new TrackTick(hitObject));
            });

            AddRange(ticks);
        }
    }
}
