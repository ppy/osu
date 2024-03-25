// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Menu
{
    public partial class OnlineMenuBanner : VisibilityContainer
    {
        public double DelayBetweenRotation = 7500;

        internal Bindable<APIMenuContent> Current { get; } = new Bindable<APIMenuContent>(new APIMenuContent());

        private const float transition_duration = 500;

        private Container content = null!;
        private CancellationTokenSource? cancellationTokenSource;

        private int displayIndex = -1;

        private ScheduledDelegate? nextDisplay;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            AutoSizeDuration = transition_duration;
            AutoSizeEasing = Easing.OutQuint;

            InternalChild = content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
            };
        }

        protected override void PopIn() => content.FadeInFromZero(transition_duration, Easing.OutQuint);

        protected override void PopOut() => content.FadeOut(transition_duration, Easing.OutQuint);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => loadNewImages(), true);

            checkForUpdates();
        }

        private void checkForUpdates()
        {
            var request = new GetMenuContentRequest();
            Task.Run(() => request.Perform())
                .ContinueWith(r =>
                {
                    if (r.IsCompletedSuccessfully)
                        Schedule(() => Current.Value = request.ResponseObject);

                    // if the request failed, "observe" the exception.
                    // it isn't very important why this failed, as it's only for display.
                    // the inner error will be logged by framework mechanisms anyway.
                    if (r.IsFaulted)
                        _ = r.Exception;

                    Scheduler.AddDelayed(checkForUpdates, TimeSpan.FromMinutes(5).TotalMilliseconds);
                });
        }

        private void loadNewImages()
        {
            nextDisplay?.Cancel();

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;

            var newContent = Current.Value;

            // A better fade out would be nice, but the menu content changes *very* rarely
            // so let's keep things simple for now.
            content.Clear(true);

            if (newContent.Images.Length == 0)
                return;

            LoadComponentsAsync(newContent.Images.Select(i => new MenuImage(i)), loaded =>
            {
                if (!newContent.Equals(Current.Value))
                    return;

                // start hidden
                foreach (var image in loaded)
                    image.Hide();

                content.AddRange(loaded);

                displayIndex = -1;
                showNext();
            }, (cancellationTokenSource ??= new CancellationTokenSource()).Token);
        }

        private void showNext()
        {
            nextDisplay?.Cancel();

            // If the user is hovering a banner, don't rotate yet.
            bool anyHovered = content.Any(i => i.IsHovered);

            if (!anyHovered)
            {
                bool previousShowing = displayIndex >= 0;
                if (previousShowing)
                    content[displayIndex % content.Count].FadeOut(400, Easing.OutQuint);

                displayIndex++;

                using (BeginDelayedSequence(previousShowing ? 300 : 0))
                    content[displayIndex % content.Count].Show();
            }

            if (content.Count > 1)
            {
                nextDisplay = Scheduler.AddDelayed(showNext, DelayBetweenRotation);
            }
        }

        [LongRunningLoad]
        public partial class MenuImage : OsuClickableContainer
        {
            public readonly APIMenuImage Image;

            private Sprite flash = null!;

            private ScheduledDelegate? openUrlAction;

            public MenuImage(APIMenuImage image)
            {
                AutoSizeAxes = Axes.Both;
                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;

                Image = image;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textureStore, OsuGame? game)
            {
                Texture? texture = textureStore.Get(Image.Image);
                if (texture != null && Image.Image.Contains(@"@2x"))
                    texture.ScaleAdjust *= 2;

                Children = new Drawable[]
                {
                    new Sprite { Texture = texture },
                    flash = new Sprite
                    {
                        Texture = texture,
                        Blending = BlendingParameters.Additive,
                    },
                };

                Action = () =>
                {
                    flash.FadeInFromZero(50)
                         .Then()
                         .FadeOut(500, Easing.OutQuint);

                    // Delay slightly to allow animation to play out.
                    openUrlAction?.Cancel();
                    openUrlAction = Scheduler.AddDelayed(() =>
                    {
                        if (!string.IsNullOrEmpty(Image.Url))
                            game?.HandleLink(Image.Url);
                    }, 250);
                };
            }

            public override void Show()
            {
                this.FadeInFromZero(500, Easing.OutQuint);
                flash.FadeOutFromOne(4000, Easing.OutQuint);
            }

            protected override bool OnHover(HoverEvent e)
            {
                this.ScaleTo(1.05f, 2000, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.ScaleTo(1f, 500, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                this.ScaleTo(0.95f, 500, Easing.OutQuint);
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                this
                    .ScaleTo(0.95f)
                    .ScaleTo(1, 500, Easing.OutElastic);
                base.OnMouseUp(e);
            }
        }
    }
}
