// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestSceneManiaComposeScreen : EditorClockTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Cached]
        private EditorClipboard clipboard = new EditorClipboard();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup compose screen", () =>
            {
                var beatmap = new ManiaBeatmap(new StageDefinition(4))
                {
                    BeatmapInfo = { Ruleset = new ManiaRuleset().RulesetInfo },
                };

                beatmap.ControlPointInfo.Add(0, new TimingControlPoint());

                var editorBeatmap = new EditorBeatmap(beatmap, new LegacyBeatmapSkin(beatmap.BeatmapInfo, null));

                Beatmap.Value = CreateWorkingBeatmap(editorBeatmap.PlayableBeatmap);

                Child = new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new (Type, object)[]
                    {
                        (typeof(EditorBeatmap), editorBeatmap),
                        (typeof(IBeatSnapProvider), editorBeatmap),
                        (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Green)),
                    },
                    Children = new Drawable[]
                    {
                        editorBeatmap,
                        new ComposeScreen { State = { Value = Visibility.Visible } },
                    }
                };
            });

            AddUntilStep("wait for composer", () => this.ChildrenOfType<HitObjectComposer>().SingleOrDefault()?.IsLoaded == true);
        }

        [Test]
        public void TestDefaultSkin()
        {
            AddStep("set default skin", () => skins.CurrentSkinInfo.Value = TrianglesSkin.CreateInfo().ToLiveUnmanaged());
        }

        [Test]
        public void TestLegacySkin()
        {
            AddStep("set legacy skin", () => skins.CurrentSkinInfo.Value = DefaultLegacySkin.CreateInfo().ToLiveUnmanaged());
        }
    }
}
