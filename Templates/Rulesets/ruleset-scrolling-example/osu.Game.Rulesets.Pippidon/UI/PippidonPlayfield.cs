// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Pippidon.UI
{
    [Cached]
    public class PippidonPlayfield : ScrollingPlayfield
    {
        public const float LANE_HEIGHT = 70;

        public const int LANE_COUNT = 6;

        public BindableInt CurrentLane => pippidon.LanePosition;

        private PippidonCharacter pippidon;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new LaneContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding
                        {
                            Left = 200,
                            Top = LANE_HEIGHT / 2,
                            Bottom = LANE_HEIGHT / 2
                        },
                        Children = new Drawable[]
                        {
                            HitObjectContainer,
                            pippidon = new PippidonCharacter
                            {
                                Origin = Anchor.Centre,
                            },
                        }
                    },
                },
            });
        }

        private class LaneContainer : BeatSyncedContainer
        {
            private OsuColour colours;
            private FillFlowContainer fill;

            private readonly Container content = new Container
            {
                RelativeSizeAxes = Axes.Both,
            };

            protected override Container<Drawable> Content => content;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                this.colours = colours;

                InternalChildren = new Drawable[]
                {
                    fill = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Colour = colours.BlueLight,
                        Direction = FillDirection.Vertical,
                    },
                    content,
                };

                for (int i = 0; i < LANE_COUNT; i++)
                {
                    fill.Add(new Lane
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = LANE_HEIGHT,
                    });
                }
            }

            private class Lane : CompositeDrawable
            {
                public Lane()
                {
                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Height = 0.95f,
                        },
                    };
                }
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                if (effectPoint.KiaiMode)
                    fill.FlashColour(colours.PinkLight, 800, Easing.In);
            }
        }
    }
}
