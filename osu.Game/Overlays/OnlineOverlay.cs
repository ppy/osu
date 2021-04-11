// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;

namespace osu.Game.Overlays
{
    public abstract class OnlineOverlay<T> : FullscreenOverlay<T>
        where T : OverlayHeader
    {
        protected override Container<Drawable> Content => content;

        protected readonly OverlayScrollContainer ScrollFlow;
        protected readonly LoadingLayer Loading;
        private readonly Container content;

        protected OnlineOverlay(OverlayColourScheme colourScheme, bool requiresSignIn = true)
            : base(colourScheme)
        {
            var mainContent = requiresSignIn
                ? new OnlineViewContainer($"Sign in to view the {Header.Title.Title}")
                : new Container();

            mainContent.RelativeSizeAxes = Axes.Both;

            mainContent.AddRange(new Drawable[]
            {
                ScrollFlow = new OverlayScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            Header.With(h => h.Depth = float.MinValue),
                            content = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            }
                        }
                    }
                },
                Loading = new LoadingLayer(true)
            });

            base.Content.Add(mainContent);
        }
    }
}
