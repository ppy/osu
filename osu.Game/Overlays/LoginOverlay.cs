// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Overlays.Login;
using osu.Game.Overlays.Settings;

namespace osu.Game.Overlays
{
    public partial class LoginOverlay : OsuFocusedOverlayContainer
    {
        private LoginPanel panel = null!;

        private const float transition_time = 400;

        protected override double PopInOutSampleBalance => OsuGameBase.SFX_STEREO_STRENGTH;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        public LoginOverlay()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black,
                Type = EdgeEffectType.Shadow,
                Radius = 10,
                Hollow = true,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new OsuContextMenuContainer
                {
                    Width = 360,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            AutoSizeDuration = transition_time,
                            AutoSizeEasing = Easing.OutQuint,
                            Child = panel = new LoginPanel
                            {
                                Padding = new MarginPadding { Vertical = SettingsSection.ITEM_SPACING },
                                RequestHide = Hide,
                            },
                        }
                    }
                }
            };
        }

        protected override void PopIn()
        {
            panel.Bounding = true;
            this.FadeIn(transition_time, Easing.OutQuint);
            FadeEdgeEffectTo(WaveContainer.SHADOW_OPACITY, WaveContainer.APPEAR_DURATION, Easing.Out);

            ScheduleAfterChildren(() => GetContainingInputManager().ChangeFocus(panel));
        }

        protected override void PopOut()
        {
            base.PopOut();

            panel.Bounding = false;
            this.FadeOut(transition_time);
            FadeEdgeEffectTo(0, WaveContainer.DISAPPEAR_DURATION, Easing.In);
        }
    }
}
