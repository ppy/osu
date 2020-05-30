// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays.MfMenu;

namespace osu.Game.Overlays
{
    public class MfMenuOverlay : FullscreenOverlay
    {
        private MfMenuHeader header;
        private MfMenuTexts textContent;
        private OverlayScrollContainer scrollContainer;

        protected Bindable<SelectedTabType> selectedTabType => header.Current;

        public MfMenuOverlay()
            : base(OverlayColourScheme.BlueLighter)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourProvider.Background6
                },
                scrollContainer = new OverlayScrollContainer
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
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    textContent = new MfMenuTexts(),
                                }
                            }
                        }
                    }
                },
                header = new MfMenuHeader(),
            };
        }

        protected override void UpdateAfterChildren()
        {
            var s = scrollContainer.Current; //滚动高度

            header.Y = -(Math.Min(s, header.DrawHeight - 47));

            scrollContainer.Padding = new MarginPadding { Top = header.DrawHeight };
        }

        protected override void LoadComplete()
        {
            selectedTabType.BindValueChanged(OnSelectedTabTypeChanged, true);
            base.LoadComplete();
        }

        private void OnSelectedTabTypeChanged(ValueChangedEvent<SelectedTabType> tab)
        {
            scrollContainer.ScrollToStart();
            textContent.UpdateContent(tab.NewValue);
        }
    }
}
