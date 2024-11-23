// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public partial class MultiSpectatorSettings : ExpandingContainer
    {
        public const float CONTRACTED_WIDTH = 30;
        public const int EXPANDED_WIDTH = 300;

        public MultiSpectatorSettings()
            : base(CONTRACTED_WIDTH, EXPANDED_WIDTH)
        {
            Origin = Anchor.TopRight;
            Anchor = Anchor.TopRight;

            PlayerSettingsOverlay playerSettingsOverlay;

            InternalChild = new FillFlowContainer
            {
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new IconButton
                    {
                        Icon = FontAwesome.Solid.Cog,
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        Action = () => Expanded.Toggle()
                    },
                    playerSettingsOverlay = new PlayerSettingsOverlay
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft
                    }
                }
            };

            playerSettingsOverlay.Show();
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            // Prevent unexpanding when hovering player settings
            if (!Contains(e.ScreenSpaceMousePosition))
                base.OnHoverLost(e);
        }
    }
}
