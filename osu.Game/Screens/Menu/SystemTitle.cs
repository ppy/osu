// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Menu
{
    public partial class SystemTitle : CompositeDrawable
    {
        internal Bindable<APISystemTitle?> Current { get; } = new Bindable<APISystemTitle?>();

        private Container content = null!;
        private CancellationTokenSource? cancellationTokenSource;
        private SystemTitleImage? currentImage;

        [BackgroundDependencyLoader]
        private void load(GameHost? gameHost)
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            AutoSizeAxes = Axes.Both;

            InternalChild = content = new ClickableContainer
            {
                AutoSizeAxes = Axes.Both,
                Action = () =>
                {
                    if (!string.IsNullOrEmpty(Current.Value?.Url))
                        gameHost?.OpenUrlExternally(Current.Value.Url);
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            content.ScaleTo(1.1f, 500, Easing.OutBounce);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            content.ScaleTo(1f, 500, Easing.OutBounce);
            base.OnHoverLost(e);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => loadNewImage(), true);
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
                if (loaded.SystemTitle != Current.Value)
                    loaded.Dispose();

                loaded.FadeInFromZero(500, Easing.OutQuint);
                content.Add(currentImage = loaded);
            }, (cancellationTokenSource ??= new CancellationTokenSource()).Token);
        }

        [LongRunningLoad]
        private partial class SystemTitleImage : Sprite
        {
            public readonly APISystemTitle SystemTitle;

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
                Texture = texture;
            }
        }
    }
}
