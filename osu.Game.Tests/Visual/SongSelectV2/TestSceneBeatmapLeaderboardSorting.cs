// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapLeaderboardSorting : SongSelectComponentsTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private BeatmapDetailsArea beatmapDetailsArea = null!;
        private ScoreManager scoreManager = null!;
        private RulesetStore rulesetStore = null!;
        private BeatmapManager beatmapManager = null!;
        private OsuContextMenuContainer contentContainer = null!;
        private DialogOverlay dialogOverlay = null!;

        private LeaderboardManager leaderboardManager = null!;

        private readonly IBindable<Screens.SelectV2.SongSelect.BeatmapSetLookupResult?> onlineLookupResult = new Bindable<Screens.SelectV2.SongSelect.BeatmapSetLookupResult?>();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.Cache(rulesetStore = new RealmRulesetStore(Realm));
            dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, dependencies.Get<AudioManager>(), Resources, dependencies.Get<GameHost>(), Beatmap.Default));
            dependencies.Cache(scoreManager = new ScoreManager(rulesetStore, () => beatmapManager, LocalStorage, Realm, API));
            dependencies.Cache(leaderboardManager = new LeaderboardManager());
            dependencies.CacheAs(onlineLookupResult);

            Dependencies.Cache(Realm);

            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LoadComponent(dialogOverlay = new DialogOverlay
            {
                Depth = -1
            });

            LoadComponent(leaderboardManager);

            Child = contentContainer = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = 500,
                Shear = OsuGame.SHEAR,
                Children = new Drawable[]
                {
                    dialogOverlay,
                }
            };
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            if (beatmapDetailsArea.IsNotNull())
                contentContainer.Remove(beatmapDetailsArea, false);

            contentContainer.Add(beatmapDetailsArea = new BeatmapDetailsArea
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Left = 50 },
                State = { Value = Visibility.Visible },
            });
        });

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
        }

        [Test]
        public void TestLocalScoresSorting()
        {
            BeatmapInfo beatmapInfo = null!;

            AddStep(@"Set beatmap", () =>
            {
                beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                beatmapInfo = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();

                Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapInfo);
            });

            AddStep(@"Import random scores", () =>
            {
                for (int i = 0; i < 10; ++i)
                    importRandomScore(beatmapInfo);
            });

            AddStep("Clear all scores", () => scoreManager.Delete());
        }

        private void importRandomScore(BeatmapInfo beatmapInfo)
        {
            scoreManager.Import(new ScoreInfo
            {
                Rank = ScoreRank.XH,
                Accuracy = RNG.NextDouble(0, 1),
                MaxCombo = RNG.Next(0, 1500),
                TotalScore = RNG.Next(500000, 1200000),
                Date = DateTime.Now.AddMinutes(RNG.Next(0, 1000) * -1),
                Statistics = new Dictionary<HitResult, int>
                {
                    { HitResult.Miss, RNG.Next(0, 25) },
                },
                Ruleset = new OsuRuleset().RulesetInfo,
                BeatmapInfo = beatmapInfo,
                BeatmapHash = beatmapInfo.Hash,
                User = new APIUser
                {
                    Id = 2,
                    Username = @"peppy",
                    CountryCode = CountryCode.JP,
                },
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesetStore.IsNotNull())
                rulesetStore.Dispose();
        }
    }
}
