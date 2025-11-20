// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public abstract partial class MatchmakingTestScene : MultiplayerTestScene
    {
        protected override Container<Drawable> Content { get; }

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        protected MatchmakingTestScene()
        {
            base.Content.AddRange(new Drawable[]
            {
                new MatchmakingBackgroundScreen.Content
                {
                    RelativeSizeAxes = Axes.Both,
                },
                Content = new Container { RelativeSizeAxes = Axes.Both }
            });
        }
    }
}
