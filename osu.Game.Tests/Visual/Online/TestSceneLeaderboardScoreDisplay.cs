// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneLeaderboardScoreDisplay : OsuTestScene
    {
        private Container content = null!;
        protected override Container<Drawable> Content => content;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create components", () => base.Content.Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 200,
                Height = 50,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Gray5,
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("create content", () => Child = new LeaderboardScoreDisplay
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Current = { Value = "1,000,000" },
            });
        }

        [Test]
        public void TestCustomContent()
        {
            AddStep("create content", () => Child = new LeaderboardScoreDisplay
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Current = { Value = "1,000,000" },
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(-10, 0),
                    Padding = new MarginPadding { Top = 4 },
                    ChildrenEnumerable = new Mod[] { new OsuModDoubleTime(), new OsuModHardRock() }.Select(mod => new ModIcon(mod)
                    {
                        Scale = new Vector2(0.3f),
                        Height = ModIcon.MOD_ICON_SIZE.Y * 3 / 4f,
                    }),
                },
            });
        }

        [Test]
        public void TestCustomContent2()
        {
            AddStep("create content", () => Child = new LeaderboardScoreDisplay
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Current = { Value = "1,000,000" },
                Child = new LeaderboardStatistic("Completed Beatmaps".ToUpperInvariant(), "2", false, 0)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Top = 5, Right = 1.5f },
                },
            });
        }

        [Test]
        public void TestUpdateScore()
        {
            LeaderboardScoreDisplay display = null!;
            Bindable<string> score = null!;

            AddStep("create content", () =>
            {
                score = new Bindable<string>("1,000,000");

                Child = display = new LeaderboardScoreDisplay
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Current = { BindTarget = score },
                };
            });
            AddUntilStep("score text is correct", () => display.ChildrenOfType<IHasText>().Single().Text.ToString(), () => Is.EqualTo("1,000,000"));
            AddStep("update score", () => score.Value = "1,234,567");
            AddUntilStep("score text is correct", () => display.ChildrenOfType<IHasText>().Single().Text.ToString(), () => Is.EqualTo("1,234,567"));
        }
    }
}
