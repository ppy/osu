// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public partial class MultiSpectatorSettings : CompositeDrawable
    {
        private const double slide_duration = 200;

        private readonly PlayerSettingsOverlay playerSettingsOverlay;
        private readonly Container slidingContainer;

        private readonly BindableBool opened = new BindableBool();

        public MultiSpectatorSettings()
        {
            Origin = Anchor.TopLeft;
            Anchor = Anchor.TopRight;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                slidingContainer = new Container
                {
                    Origin = Anchor.TopLeft,
                    Anchor = Anchor.TopLeft,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new IconButton
                        {
                            Icon = FontAwesome.Solid.Cog,
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            Position = new Vector2(-30, 0),
                            Action = () => opened.Toggle()
                        },
                        playerSettingsOverlay = new PlayerSettingsOverlay()
                    }
                }
            };

            playerSettingsOverlay.Show();

            opened.BindValueChanged(value =>
            {
                if (value.NewValue)
                    open();
                else
                    close();
            });
        }

        private void open()
        {
            slidingContainer.MoveToOffset(new Vector2(-playerSettingsOverlay.Width, 0), slide_duration, Easing.Out).Then().OnComplete(c =>
            {
                c.Origin = Anchor.TopRight;
                c.Position = Vector2.Zero;
            });
        }

        private void close()
        {
            slidingContainer.MoveToOffset(new Vector2(playerSettingsOverlay.Width, 0), slide_duration, Easing.Out).Then().OnComplete(c =>
            {
                c.Origin = Anchor.TopLeft;
                c.Position = Vector2.Zero;
            });
        }
    }
}
