// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;

namespace osu.Game.Overlays
{
    public abstract partial class OnlineOverlay<T> : FullscreenOverlay<T>
        where T : OverlayHeader
    {
        protected override Container<Drawable> Content => content;

        [Cached]
        protected readonly OverlayScrollContainer ScrollFlow;

        protected readonly LoadingLayer Loading;
        private readonly Container loadingContainer;
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
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new PopoverContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
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
                    }
                },
                loadingContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = Loading = new LoadingLayer(true),
                }
            });

            base.Content.Add(mainContent);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Ensure the scroll-to-top button is displayed above the fixed header.
            AddInternal(ScrollFlow.Button.CreateProxy());
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // don't block header by applying padding equal to the visible header height
            loadingContainer.Padding = new MarginPadding { Top = Math.Max(0, Header.Height - ScrollFlow.Current) };
        }
    }
}
