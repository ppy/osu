// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneSelectionPanel : MultiplayerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Test]
        public void TestBeatmapPanel()
        {
            SelectionPanel? panel = null;

            AddStep("add panel", () => Child = panel = new SelectionPanel(new MultiplayerPlaylistItem())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddStep("add maarvin", () => panel!.AddUser(new APIUser
            {
                Id = 6411631,
                Username = "Maarvin",
            }, isOwnUser: true));
            AddStep("add peppy", () => panel!.AddUser(new APIUser
            {
                Id = 2,
                Username = "peppy",
            }));
            AddStep("add smogipoo", () => panel!.AddUser(new APIUser
            {
                Id = 1040328,
                Username = "smoogipoo",
            }));
            AddStep("remove smogipoo", () => panel!.RemoveUser(new APIUser { Id = 1040328 }));
            AddStep("remove peppy", () => panel!.RemoveUser(new APIUser { Id = 2 }));
            AddStep("remove maarvin", () => panel!.RemoveUser(new APIUser { Id = 6411631 }));

            AddToggleStep("allow selection", value =>
            {
                if (panel != null)
                    panel.AllowSelection = value;
            });
        }
    }
}
