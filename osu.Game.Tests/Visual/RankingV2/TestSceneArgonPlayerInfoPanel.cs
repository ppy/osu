// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Argon;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneArgonPlayerInfoPanel : OsuTestScene
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Cached(typeof(UserLookupCache))]
        private readonly TestUserLookupCache userLookupCache = new TestUserLookupCache();

        private ArgonPlayerInfoPanel panel = null!;
        private SkinnableTestScene.OutlineBox? outline;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set score", () => score.Value = createScore());

            AddStep("create component", () => Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    panel = new ArgonPlayerInfoPanel(),
                    outline = new SkinnableTestScene.OutlineBox(),
                ],
            });
        }

        [Test]
        public void TestAppearance()
        {
            AddToggleStep("toggle outline", t => outline?.Alpha = t ? 1 : 0);
            AddStep("begin animation", () => panel.StartAnimating(Time.Current));
            AddStep("cancel animation", () => panel.FinishAnimating());

            AddStep("score without rank", () => score.Value = createScore());
            AddStep("score with rank 1", () => score.Value = new ScoreWithPosition(createScore(), 1));
            AddStep("score with rank ~2k", () => score.Value = new ScoreWithPosition(createScore(), 2_123));
            AddStep("score with rank ~4M", () => score.Value = new ScoreWithPosition(createScore(), 3_842_834));

            // https://github.com/ppy/osu-web/blob/e6aa237c1632ce2980722c5104d1981a2bc40a20/app/Libraries/UsernameValidation.php#L23-L24
            AddStep("shortest possible username", () => score.Value = createScore(s => s.User.Username = "osu"));
            AddStep("longest possible username", () => score.Value = createScore(s => s.User.Username = new string('A', 15)));

            AddStep("user with online lookup", () => score.Value = new ScoreInfo
            {
                RealmUser = new RealmUser
                {
                    Username = "Onliner",
                    OnlineID = 4000,
                },
                Date = DateTimeOffset.Now,
            });
            AddStep("user with failed online lookup", () => score.Value = new ScoreInfo
            {
                RealmUser = new RealmUser
                {
                    Username = "Unknown User",
                    OnlineID = TestUserLookupCache.UNRESOLVED_USER_ID,
                },
                Date = DateTimeOffset.Now,
            });
        }

        private static ScoreInfo createScore(Action<ScoreInfo>? setUp = null)
        {
            var score = new ScoreInfo
            {
                User = new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                    AvatarUrl = "https://a.ppy.sh/2",
                    CoverUrl = TestResources.COVER_IMAGE_3,
                },
                Date = DateTimeOffset.Now,
            };
            setUp?.Invoke(score);
            return score;
        }
    }
}
