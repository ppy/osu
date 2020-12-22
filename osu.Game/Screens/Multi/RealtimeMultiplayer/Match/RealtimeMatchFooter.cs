// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer.Match
{
    public class RealtimeMatchFooter : CompositeDrawable
    {
        public const float HEIGHT = 50;

        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        private readonly Drawable background;

        public RealtimeMatchFooter()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            InternalChildren = new[]
            {
                background = new Box { RelativeSizeAxes = Axes.Both },
                new RealtimeReadyButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(600, 50),
                    SelectedItem = { BindTarget = SelectedItem }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = Color4Extensions.FromHex(@"28242d");
        }
    }
}
