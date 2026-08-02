// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osuTK;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayBackground : OsuTestScene
    {
        private readonly RankedPlayBackground background;

        private readonly Bindable<Colour4> gradientOuter = new Bindable<Colour4>(Color4Extensions.FromHex("AC6D97"));
        private readonly Bindable<Colour4> gradientInner = new Bindable<Colour4>(Color4Extensions.FromHex("544483"));

        public TestSceneRankedPlayBackground()
        {
            Children =
            [
                background = new RankedPlayBackground { RelativeSizeAxes = Axes.Both },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children =
                    [
                        new BasicColourPicker
                        {
                            Scale = new Vector2(0.4f),
                            Current = gradientOuter,
                        },
                        new BasicColourPicker
                        {
                            Scale = new Vector2(0.4f),
                            Current = gradientInner,
                        },
                    ]
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            gradientOuter.BindValueChanged(e => background.GradientBottom = e.NewValue, true);
            gradientInner.BindValueChanged(e => background.GradientTop = e.NewValue, true);
        }
    }
}
