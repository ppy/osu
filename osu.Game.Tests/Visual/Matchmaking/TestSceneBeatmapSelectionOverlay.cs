// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneBeatmapSelectionOverlay : OsuTestScene
    {
        private BeatmapSelectionOverlay selectionOverlay = null!;

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("add drawable", () => Child = new Container
            {
                Width = 100,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(2),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.1f,
                    },
                    selectionOverlay = new BeatmapSelectionOverlay
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    }
                }
            });
        }

        [Test]
        public void TestSelectionOverlay()
        {
            AddStep("add maarvin", () => selectionOverlay.AddUser(new APIUser
            {
                Id = 6411631,
                Username = "Maarvin",
            }, isOwnUser: true));
            AddStep("add peppy", () => selectionOverlay.AddUser(new APIUser
            {
                Id = 2,
                Username = "peppy",
            }, false));
            AddStep("add smogipoo", () => selectionOverlay.AddUser(new APIUser
            {
                Id = 1040328,
                Username = "smoogipoo",
            }, false));
            AddStep("remove smogipoo", () => selectionOverlay.RemoveUser(1040328));
            AddStep("remove peppy", () => selectionOverlay.RemoveUser(2));
            AddStep("remove maarvin", () => selectionOverlay.RemoveUser(6411631));
        }
    }
}
