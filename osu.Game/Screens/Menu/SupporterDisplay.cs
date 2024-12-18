// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public partial class SupporterDisplay : CompositeDrawable
    {
        private LinkFlowContainer supportFlow = null!;

        private Drawable heart = null!;

        private readonly IBindable<APIUser> currentUser = new Bindable<APIUser>();

        private Box backgroundBox = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = 40;

            AutoSizeAxes = Axes.X;
            AutoSizeDuration = 1000;
            AutoSizeEasing = Easing.OutQuint;

            Masking = true;
            CornerExponent = 2.5f;
            CornerRadius = 15;

            InternalChildren = new Drawable[]
            {
                backgroundBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.4f,
                },
                supportFlow = new LinkFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding(10),
                    Spacing = new Vector2(0, 2),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            const float font_size = 14;

            static void formatSemiBold(SpriteText t) => t.Font = OsuFont.GetFont(size: font_size, weight: FontWeight.SemiBold);

            currentUser.BindTo(api.LocalUser);
            currentUser.BindValueChanged(e =>
            {
                supportFlow.Children.ForEach(d => d.FadeOut().Expire());

                if (e.NewValue.IsSupporter)
                {
                    supportFlow.AddText("Eternal thanks to you for supporting osu!", formatSemiBold);

                    backgroundBox.FadeColour(colours.Pink, 250);
                }
                else
                {
                    supportFlow.AddText("Consider becoming an ", formatSemiBold);
                    supportFlow.AddLink("osu!supporter", "https://osu.ppy.sh/home/support", formatSemiBold);
                    supportFlow.AddText(" to help support osu!'s development", formatSemiBold);

                    backgroundBox.FadeColour(colours.Pink4, 250);
                }

                supportFlow.AddIcon(FontAwesome.Solid.Heart, t =>
                {
                    heart = t;

                    t.Padding = new MarginPadding { Left = 5, Top = 1 };
                    t.Font = t.Font.With(size: font_size);
                    t.Origin = Anchor.Centre;
                    t.Colour = colours.Pink;

                    Schedule(() =>
                    {
                        heart?.FlashColour(Color4.White, 750, Easing.OutQuint).Loop();
                    });
                });
            }, true);

            this
                .FadeOut()
                .Delay(1000)
                .FadeInFromZero(800, Easing.OutQuint);

            scheduleDismissal();
        }

        protected override bool OnClick(ClickEvent e)
        {
            dismissalDelegate?.Cancel();

            supportFlow.BypassAutoSizeAxes = Axes.X;
            this.FadeOut(500, Easing.OutQuint);
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            backgroundBox.FadeTo(0.6f, 500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            backgroundBox.FadeTo(0.4f, 500, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private ScheduledDelegate? dismissalDelegate;

        private void scheduleDismissal()
        {
            dismissalDelegate?.Cancel();
            dismissalDelegate = Scheduler.AddDelayed(() =>
            {
                // If the user is hovering they may want to interact with the link.
                // Give them more time.
                if (IsHovered)
                {
                    scheduleDismissal();
                    return;
                }

                dismissalDelegate?.Cancel();

                AutoSizeEasing = Easing.In;
                supportFlow.BypassAutoSizeAxes = Axes.X;
                this
                    .Delay(200)
                    .FadeOut(750, Easing.Out);
            }, 6000);
        }
    }
}
