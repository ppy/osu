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
        internal Bindable<APIMenuContent> Current { get; } = new Bindable<APIMenuContent>(new APIMenuContent());

        private const float transition_duration = 500;

        private Container content = null!;
        private CancellationTokenSource? cancellationTokenSource;
        private MenuImage? currentImage;

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

        protected override bool OnHover(HoverEvent e)
        {
            content.ScaleTo(1.05f, 2000, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            content.ScaleTo(1f, 500, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            content.ScaleTo(0.95f, 500, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            content
                .ScaleTo(0.95f)
                .ScaleTo(1, 500, Easing.OutElastic);
            base.OnMouseUp(e);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => loadNewImage(), true);

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

        private void loadNewImage()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
            currentImage?.FadeOut(500, Easing.OutQuint).Expire();

            if (Current.Value.Images.Length == 0)
                return;

            LoadComponentAsync(new MenuImage(Current.Value.Images.First()), loaded =>
            {
                if (!loaded.Image.Equals(Current.Value.Images.First()))
                    loaded.Dispose();

                content.Add(currentImage = loaded);
            }, (cancellationTokenSource ??= new CancellationTokenSource()).Token);
        }

        [LongRunningLoad]
        private partial class MenuImage : OsuClickableContainer
        {
            public readonly APIMenuImage Image;

            private Sprite flash = null!;

            private ScheduledDelegate? openUrlAction;

            public MenuImage(APIMenuImage image)
            {
                Image = image;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textureStore, OsuGame game)
            {
                Texture? texture = textureStore.Get(Image.Image);
                if (texture != null && Image.Image.Contains(@"@2x"))
                    texture.ScaleAdjust *= 2;

                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
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
                    Flash();

                    // Delay slightly to allow animation to play out.
                    openUrlAction?.Cancel();
                    openUrlAction = Scheduler.AddDelayed(() =>
                    {
                        if (!string.IsNullOrEmpty(Image.Url))
                            game?.HandleLink(Image.Url);
                    }, 250);
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                this.FadeInFromZero(500, Easing.OutQuint);
                flash.FadeOutFromOne(4000, Easing.OutQuint);
            }

            public Drawable Flash()
            {
                flash.FadeInFromZero(50)
                     .Then()
                     .FadeOut(500, Easing.OutQuint);

                return this;
            }
        }
    }
}
