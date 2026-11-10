// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Overlays.Music;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestScenePlaylistOverlay : OsuManualInputManagerTestScene
    {
        protected override bool UseFreshStoragePerRun => true;

        private RulesetStore rulesets = null!;
        private BeatmapManager beatmapManager = null!;

        private const int item_count = 20;

        private List<BeatmapSetInfo> beatmapSets => beatmapManager.GetAllUsableBeatmapSets();

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, Audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300, 500),
                Child = new PlaylistOverlay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    State = { Value = Visibility.Visible }
                }
            };

            for (int i = 0; i < item_count; i++)
            {
                beatmapManager.Import(TestResources.CreateTestBeatmapSetInfo());
            }

            beatmapSets.First().ToLive(Realm);

            // Ensure all the initial imports are present before running any tests.
            Realm.Run(r => r.Refresh());
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesets.IsNotNull())
                rulesets.Dispose();
        }
    }
}
