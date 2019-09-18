// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Input.Events;
using osu.Game.Users;

namespace osu.Game.Overlays.Social
{
    public class SocialPanel : UserPanel
    {
        private const double hover_transition_time = 400;

        public SocialPanel(User user)
            : base(user)
        {
        }

        private readonly EdgeEffectParameters edgeEffectNormal = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Offset = new Vector2(0f, 1f),
            Radius = 2f,
            Colour = Color4.Black.Opacity(0.25f),
        };

        private readonly EdgeEffectParameters edgeEffectHovered = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Offset = new Vector2(0f, 5f),
            Radius = 10f,
            Colour = Color4.Black.Opacity(0.3f),
        };

        protected override bool OnHover(HoverEvent e)
        {
            Content.TweenEdgeEffectTo(edgeEffectHovered, hover_transition_time, Easing.OutQuint);
            Content.MoveToY(-4, hover_transition_time, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Content.TweenEdgeEffectTo(edgeEffectNormal, hover_transition_time, Easing.OutQuint);
            Content.MoveToY(0, hover_transition_time, Easing.OutQuint);

            base.OnHoverLost(e);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(200, Easing.Out);
        }
    }
}
