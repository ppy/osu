// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning;
using osu.Game.Skinning.Select;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLegacyBackButton : SkinnableTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private NowPlayingOverlay overlay = null!;

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(overlay = new NowPlayingOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("load buttons", () =>
            {
                SetContents(s =>
                {
                    if (s is not LegacySkin)
                        return Empty();

                    return new Container
                    {
                        Size = new Vector2(300),
                        Child = new SkinProvidingContainer(skins.DefaultClassicSkin)
                        {
                            Child = new SkinProvidingContainer(s)
                            {
                                Child = new LegacyBackButton
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                }
                            },
                        }
                    };
                });
            });
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("do nothing", () =>
            {
                // for some reason TestBeat automatically runs.
                // we don't want that.
            });
        }

        [Test]
        public void TestBeat()
        {
            AddStep("show now playing overlay", () => overlay.Show());
            AddStep("play my love", () =>
            {
                Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmaps.QueryOnlineBeatmapId(2868389));
                Beatmap.Value.BeginAsyncLoad(); // need beatmap loaded for back button to access timing info
                MusicController.EnsurePlayingSomething();
            });
        }
    }
}
