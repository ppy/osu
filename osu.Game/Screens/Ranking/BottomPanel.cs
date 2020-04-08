// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public class BottomPanel : Container
    {
        public readonly Bindable<bool> panel_IsHovered = new Bindable<bool>();
        private static readonly Vector2 BOTTOMPANEL_SIZE = new Vector2(TwoLayerButton.SIZE_EXTENDED.X, 60);
        public BottomPanel()
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            RelativeSizeAxes = Axes.X;
            Height = BOTTOMPANEL_SIZE.Y;
            Alpha = 0;
        }
        
        protected override bool OnHover(Framework.Input.Events.HoverEvent e)
        {
            this.panel_IsHovered.Value = true;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(Framework.Input.Events.HoverLostEvent e)
        {
            this.panel_IsHovered.Value = false;
            base.OnHoverLost(e);
        }
    }
}
