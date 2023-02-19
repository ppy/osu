using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin.Plugins.Internal.LuaSupport.Graphics
{
    internal partial class OutputLine : CompositeDrawable, IHasContextMenu
    {
        public string Content { get; private set; }

        private readonly string timeString;
        private readonly bool isCritical;
        private readonly bool isNewSegment;
        private Box? bgBox;

        public OutputLine(string content, bool isCritical, bool newSegment = false)
        {
            Content = content;
            timeString = DateTime.Now.ToString("T");
            this.isCritical = isCritical;
            this.isNewSegment = newSegment;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 8;

            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.6f),
                    Alpha = 0,
                    Y = 2
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(4),

                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,

                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize)
                        },
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 68),
                            new Dimension(GridSizeMode.Absolute, 12),
                            new Dimension(GridSizeMode.Distributed)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = timeString,
                                    AlwaysPresent = true,
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                                },
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 4,
                                    Colour = (isCritical ? Color4.Gold : Color4.White),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Margin = new MarginPadding { Left = 2 },
                                },
                                new OsuTextFlowContainer(s =>
                                {
                                    s.Colour = isCritical ? Color4.Red : Color4.White;
                                    s.Font = OsuFont.GetFont(weight: (isNewSegment || isCritical) ? FontWeight.Bold : FontWeight.Regular);
                                    s.AlwaysPresent = true;
                                })
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Left = 4 },
                                    Text = Content
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            bgBox?.FadeIn(450, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            bgBox?.FadeOut(450, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("复制", MenuItemType.Standard, () =>
            {
                SDL2.SDL.SDL_SetClipboardText(Content);
            })
        };
    }
}
