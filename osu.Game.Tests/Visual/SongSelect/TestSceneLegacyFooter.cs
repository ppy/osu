// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Screens.Menu;
using osu.Game.Skinning;
using osu.Game.Skinning.Select;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneLegacyFooter : OsuTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Cached(typeof(LocalUserStatisticsProvider))]
        private readonly TestUserStatisticsProvider userStatisticsProvider = new TestUserStatisticsProvider();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Ruleset.BindValueChanged(_ =>
            {
                userStatisticsProvider.UpdateStatistics(new UserStatistics
                {
                    User = new APIUser { Username = "frenzibyte", Id = 14210502 },
                    PP = 2975,
                    Accuracy = 95.84,
                    Level = { Current = 95, Progress = 73 },
                    GlobalRank = 302844,
                }, Ruleset.Value);
            }, true);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            reloadContent(1366, 768);
        });

        [Test]
        public void TestDisplay()
        {
            AddStep("widescreen", () => reloadContent(1366, 768));
            AddStep("non-widescreen", () => reloadContent(1024, 768));

            AddStep("osu! ruleset", () => Ruleset.Value = new OsuRuleset().RulesetInfo);
            AddStep("osu!taiko ruleset", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddStep("osu!catch ruleset", () => Ruleset.Value = new CatchRuleset().RulesetInfo);
            AddStep("osu!mania ruleset", () => Ruleset.Value = new ManiaRuleset().RulesetInfo);
        }

        private void reloadContent(float width, float height)
        {
            LegacyFooter footer;
            OsuLogo logo;
            OsuDropdown<Live<SkinInfo>> skinDropdown;

            Child = new SkinProvidingContainer(skins.CurrentSkin.Value)
            {
                Children = new Drawable[]
                {
                    skinDropdown = new OsuDropdown<Live<SkinInfo>>
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Width = 400,
                        RelativePositionAxes = Axes.Both,
                        Position = new Vector2(-0.025f, 0.025f),
                        Items = skins.GetAllUsableSkins(),
                        Current = skins.CurrentSkinInfo,
                        Depth = float.MinValue,
                    },
                    new DrawSizePreservingFillContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        TargetDrawSize = new Vector2(width, height) / 0.95f,
                        Child = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(width, height),
                            Masking = true,
                            BorderColour = Color4.Orange,
                            BorderThickness = 2f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Transparent,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                footer = new LegacyFooter
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.X,
                                },
                                logo = new OsuLogo
                                {
                                    RelativePositionAxes = Axes.Both,
                                    Scale = new Vector2(0.4f),
                                    Action = () => true,
                                },
                            },
                        },
                    },
                },
            };

            footer.StartTrackingLogo(logo, 400, Easing.OutQuint);

            skinDropdown.Current.BindValueChanged(_ => reloadContent(width, height));
        }

        public partial class TestUserStatisticsProvider : LocalUserStatisticsProvider
        {
            public new void UpdateStatistics(UserStatistics newStatistics, RulesetInfo ruleset, Action<UserStatisticsUpdate>? callback = null)
                => base.UpdateStatistics(newStatistics, ruleset, callback);
        }
    }
}
