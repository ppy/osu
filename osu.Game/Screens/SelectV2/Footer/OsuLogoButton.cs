// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2.Footer
{
    public partial class OsuLogoButton : ShearedButton
    {
        private const float logo_scale = 0.15f;

        private LogoTrackingContainer logoTrackingContainer = null!;
        private Sprite logoTextSprite = null!;

        public OsuLogoButton(float? width = null)
            : base(width)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            ButtonContent.Child = (logoTrackingContainer = new LogoTrackingContainer
            {
                RelativeSizeAxes = Axes.Both,
            }).WithChildren(new Drawable[]
            {
                logoTextSprite = new Sprite
                {
                    Alpha = 0f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get(@"Menu/logo-without-border"),
                    Scale = new Vector2(logo_scale),
                },
                logoTrackingContainer.LogoFacade.With(f =>
                {
                    f.Anchor = Anchor.Centre;
                    f.Origin = Anchor.Centre;
                }),
            });

            DarkerColour = colours.Pink1;
            LighterColour = Color4.White;
        }

        public void AttachLogo(OsuLogo logo, double duration, Easing easing)
        {
            logoTextSprite.Delay(duration)
                          .FadeIn(duration, easing)
                          .MoveToY(0, duration, easing);

            logoTrackingContainer.StartTracking(logo, duration, easing);
        }

        public void DetachLogo(double duration, Easing easing, bool fadeTextOut)
        {
            if (fadeTextOut)
            {
                logoTextSprite.FadeOut(duration, easing)
                              .MoveToY(-10, duration, easing);
            }

            logoTrackingContainer.StopTracking();
        }
    }
}
