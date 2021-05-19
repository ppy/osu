// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Online;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneExplicitContentPermission : OsuManualInputManagerTestScene
    {
        [Resolved]
        private IExplicitContentPermission gameExplicitPermission { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            var explicitBeatmap = new TestBeatmap(Ruleset.Value).BeatmapInfo;
            explicitBeatmap.BeatmapSet.OnlineInfo.HasExplicitContent = true;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10f),
                    Children = new Drawable[]
                    {
                        new GridBeatmapPanel(explicitBeatmap.BeatmapSet)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        new DrawableRoomPlaylistItem(new PlaylistItem
                        {
                            Beatmap = { Value = explicitBeatmap },
                            Ruleset = { Value = explicitBeatmap.Ruleset },
                        }, false, false)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                }
            };

            config.Set(OsuSetting.ShowOnlineExplicitContent, false);

            InputManager.MoveMouseTo(this.ChildrenOfType<GridBeatmapPanel>().Single());
        });

        [Test]
        public void TestGlobalPermissionRespectsConfig()
        {
            AddStep("enable explicit content setting", () => config.Set(OsuSetting.ShowOnlineExplicitContent, true));
            AddAssert("global permission set to allowed", () => gameExplicitPermission.UserAllowed.Value);
            AddStep("disable explicit content setting", () => config.Set(OsuSetting.ShowOnlineExplicitContent, false));
            AddAssert("global permission set to denied", () => !gameExplicitPermission.UserAllowed.Value);
        }
    }
}
