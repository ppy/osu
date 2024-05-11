// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Edit;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public partial class CatchEditorTestSceneContainer : Container
    {
        [Cached(typeof(Playfield))]
        public readonly ScrollingPlayfield Playfield;

        protected override Container<Drawable> Content { get; }

        public CatchEditorTestSceneContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Width = CatchPlayfield.WIDTH;
            Height = 1000;
            Padding = new MarginPadding
            {
                Bottom = 100
            };

            InternalChildren = new Drawable[]
            {
                new ScrollingTestContainer(ScrollingDirection.Down)
                {
                    TimeRange = 1000,
                    RelativeSizeAxes = Axes.Both,
                    Child = Playfield = new TestCatchPlayfield
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                },
                new PlayfieldBorder
                {
                    PlayfieldBorderStyle = { Value = PlayfieldBorderStyle.Full },
                    Clock = new FramedClock(new StopwatchClock(true))
                },
                Content = new Container
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        private partial class TestCatchPlayfield : CatchEditorPlayfield
        {
            public TestCatchPlayfield()
                : base(new BeatmapDifficulty { CircleSize = 0 })
            {
            }
        }
    }
}
