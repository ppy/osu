// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Team;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneTeamProfileHeader : OsuTestScene
    {
        private TeamProfileHeader header = null!;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create header", () =>
            {
                header = new TeamProfileHeader();

                Child = new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new (Type, object)[]
                    {
                        (typeof(TeamProfileHeader), header),
                        (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Pink))
                    },
                    Child = new OsuScrollContainer(Direction.Vertical)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new PopoverContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = header,
                        },
                    },
                };
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("show team", () => header.TeamData.Value = new TeamProfileData(TEST_TEAM, new OsuRuleset().RulesetInfo));
        }

        [Test]
        public void TestMissingImages()
        {
            AddStep("show team with no flag", () =>
            {
                header.TeamData.Value = new TeamProfileData(new APITeam
                {
                    Name = "mom?",
                    Id = 1,
                    ShortName = "MOM",
                    CoverUrl = TestResources.COVER_IMAGE_1,
                }, new OsuRuleset().RulesetInfo);
            });
            AddStep("show team with no cover", () =>
            {
                header.TeamData.Value = new TeamProfileData(new APITeam
                {
                    Name = "mom?",
                    Id = 1,
                    ShortName = "MOM",
                    FlagUrl = "https://assets.ppy.sh/teams/flag/1/b46fb10dbfd8a35dc50e6c00296c0dc6172dffc3ed3d3a4b379277ba498399fe.png",
                }, new OsuRuleset().RulesetInfo);
            });
        }

        public static readonly APITeam TEST_TEAM = new APITeam
        {
            Name = "mom?",
            Id = 1,
            ShortName = "MOM",
            CoverUrl = TestResources.COVER_IMAGE_1,
            FlagUrl = "https://assets.ppy.sh/teams/flag/1/b46fb10dbfd8a35dc50e6c00296c0dc6172dffc3ed3d3a4b379277ba498399fe.png",
        };
    }
}
