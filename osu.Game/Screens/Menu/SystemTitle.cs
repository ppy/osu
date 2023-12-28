// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public partial class SystemTitle : CompositeDrawable
    {
        internal Bindable<APISystemTitle?> Current { get; } = new Bindable<APISystemTitle?>();

        private Container content = null!;
        private CancellationTokenSource? cancellationTokenSource;
        private SystemTitleImage? currentImage;

        private ScheduledDelegate? openUrlAction;

        [BackgroundDependencyLoader]
        private void load(OsuGame? game)
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = content = new OsuClickableContainer
            {
                AutoSizeAxes = Axes.Both,
                Action = () =>
                {
                    currentImage?.Flash();

                    // Delay slightly to allow animation to play out.
                    openUrlAction?.Cancel();
                    openUrlAction = Scheduler.AddDelayed(() =>
                    {
                        if (!string.IsNullOrEmpty(Current.Value?.Url))
                            game?.HandleLink(Current.Value.Url);
                    }, 250);
                }
            };
        }

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
            var request = new GetSystemTitleRequest();
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

            if (string.IsNullOrEmpty(Current.Value?.Image))
                return;

            LoadComponentAsync(new SystemTitleImage(Current.Value), loaded =>
            {
                if (!loaded.SystemTitle.Equals(Current.Value))
                    loaded.Dispose();

                content.Add(currentImage = loaded);
            }, (cancellationTokenSource ??= new CancellationTokenSource()).Token);
        }

        [LongRunningLoad]
        private partial class SystemTitleImage : CompositeDrawable
        {
            public readonly APISystemTitle SystemTitle;

            private Sprite flash = null!;

            public SystemTitleImage(APISystemTitle systemTitle)
            {
                SystemTitle = systemTitle;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textureStore)
            {
                var texture = textureStore.Get(SystemTitle.Image);
                if (SystemTitle.Image.Contains(@"@2x"))
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
