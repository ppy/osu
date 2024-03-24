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
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;

            var newContent = Current.Value;

            foreach (var i in content)
            {
                i.FadeOutFromOne(100, Easing.OutQuint)
                 .Expire();
            }

            if (newContent.Images.Length == 0)
                return;

            LoadComponentsAsync(newContent.Images.Select(i => new MenuImage(i)), loaded =>
            {
                if (!newContent.Equals(Current.Value))
                    return;

                content.AddRange(loaded);

                loaded.First().Show();
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
