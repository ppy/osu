using System;
using Mvis.Plugin.CloudMusicSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Mvis;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public class Toolbox : CompositeDrawable
    {
        private readonly Box bgBox;
        private readonly FillFlowContainer buttonFillFlow;
        private readonly OsuSpriteText idText;

        private readonly IconButton backButton = new IconButton
        {
            Icon = FontAwesome.Solid.ArrowLeft,
            Size = new Vector2(45),
            IconColour = Color4.Black,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        };

        private readonly FillFlowContainer contentFillFlow;

        public Toolbox()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            Margin = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                contentFillFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding(10),
                    Children = new Drawable[]
                    {
                        new TrackTimeIndicator(),
                        buttonFillFlow = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Spacing = new Vector2(5),
                            AutoSizeDuration = 200,
                            AutoSizeEasing = Easing.OutQuint,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight
                        },
                    }
                },
                idText = new OsuSpriteText
                {
                    Margin = new MarginPadding { Horizontal = 15, Top = 15 },
                    Font = OsuFont.GetFont(size: 20),
                    Colour = Color4.Black
                },
            };
        }

        public Action OnBackAction { get; set; }

        public string IdText
        {
            set => idText.Text = value;
        }

        public void AddButtonRange(IconButton[] range, bool isRootScreen)
        {
            buttonFillFlow.Clear(false);

            foreach (var btn in range)
            {
                btn.IconColour = Color4.Black;
                btn.Anchor = btn.Origin = Anchor.Centre;

                buttonFillFlow.Add(btn);
            }

            if (!isRootScreen)
            {
                buttonFillFlow.Add(backButton);
                backButton.Action = OnBackAction;
            }
        }

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [BackgroundDependencyLoader]
        private void load(LyricConfigManager config)
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = colourProvider.ActiveColor;
            }, true);

            contentFillFlow.Add(
                new SettingsSlider<double>
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Current = config.GetBindable<double>(LyricSettings.LyricOffset),
                    LabelText = "全局歌词偏移",
                    RelativeSizeAxes = Axes.None,
                    Width = 200 + 25,
                    Padding = new MarginPadding { Right = 10 },
                    Colour = Color4.Black
                });
        }
    }
}
