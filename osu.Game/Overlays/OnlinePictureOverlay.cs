// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.OnlinePicture;
using osuTK;
using osu.Game.Graphics.Sprites;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Overlays
{
    public class OnlinePictureOverlay : OsuFocusedOverlayContainer
    {
        [Resolved]
        private OsuGame game { get; set; }

        private const float duration = 1000;

        private string target;

        private OnlinePictureContentContainer contentContainer;
        private Container topbarContainer;
        private Container bottomContainer;
        private LoadingSpinner loadingSpinner;
        private TriangleButton openInBrowserButton;
        private OsuSpriteText infoText;
        private bool canOpenInBrowser;

        private readonly OverlayColourProvider overlayColourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);

        private readonly BindableBool optUI = new BindableBool();

        public float BottomContainerHeight => bottomContainer.Position.Y + bottomContainer.DrawHeight;
        public float TopBarHeight => topbarContainer.DrawHeight;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            RelativeSizeAxes = Axes.Both;

            Size = new Vector2(0.9f);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Masking = true;
            CornerRadius = 15;

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = overlayColourProvider.Background6,
                    RelativeSizeAxes = Axes.Both,
                },
                loadingSpinner = new LoadingSpinner(true)
                {
                    Depth = -1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                contentContainer = new OnlinePictureContentContainer
                {
                    GetBottomContainerHeight = () => BottomContainerHeight,
                    GetTopBarHeight = () => TopBarHeight,
                    RelativeSizeAxes = Axes.Both,

                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                bottomContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Height = 50f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = overlayColourProvider.Background5
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = new Vector2(20),
                            Children = new Drawable[]
                            {
                                openInBrowserButton = new TriangleButton
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.2f,
                                    Height = 0.75f,
                                    Text = "在浏览器中打开",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Action = () => openLink(target),
                                },
                                new TriangleButton
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.2f,
                                    Height = 0.75f,
                                    Text = "关闭",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Action = Hide,
                                },
                            }
                        }
                    }
                },
                topbarContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = overlayColourProvider.Background5
                        },
                        infoText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 25),
                            Margin = new MarginPadding { Vertical = 10 },
                        }
                    }
                }
            };

            config.BindWith(MSetting.OptUI, optUI);

            optUI.BindValueChanged(OnOptUIChanged);
        }

        protected override void PopIn()
        {
            base.PopIn();

            this.FadeIn(duration, Easing.OutQuint);
            this.ScaleTo(1, duration, Easing.OutElastic);
        }

        protected override void PopOut()
        {
            base.PopOut();

            delayedLoadWrapper?.FadeOut(duration / 2, Easing.OutQuint);
            delayedLoadWrapper?.Expire();
            this.FadeOut(duration, Easing.OutQuint);
            this.ScaleTo(0.9f, duration, Easing.OutQuint);
        }

        private DelayedLoadWrapper delayedLoadWrapper;
        private UpdateableOnlinePicture pict;

        private void OnOptUIChanged(ValueChangedEvent<bool> v)
        {
            if (Alpha != 0 && v.NewValue == false && openLink(target))
                Hide();
        }

        private bool openLink(string link)
        {
            if (canOpenInBrowser)
            {
                game?.OpenUrlExternally(target);
                return true;
            }
            else
                return false;
        }

        public void UpdateImage(string newUri, bool popIn, bool canOpenInBrowser = true, string title = null)
        {
            target = newUri.Replace("http://", "https://");
            this.canOpenInBrowser = canOpenInBrowser;

            if (optUI.Value != true && canOpenInBrowser)
            {
                openLink(target);
                return;
            }

            if (popIn) Show();

            loadingSpinner.Show();

            if (delayedLoadWrapper != null)
            {
                delayedLoadWrapper.FadeOut(duration / 2, Easing.OutQuint);
                delayedLoadWrapper.Expire();
            }

            infoText.Text = title ?? target;

            if (canOpenInBrowser)
                openInBrowserButton.Action = () => openLink(target);
            else
                openInBrowserButton.Action = null;

            foreach (var i in contentContainer)
            {
                i.Hide();
                i.Expire();
            }

            contentContainer.Add(delayedLoadWrapper = new DelayedLoadWrapper(
                pict = new UpdateableOnlinePicture(target)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit
                })
            );

            pict.OnLoadComplete += d =>
            {
                d.Hide();
                d.FadeInFromZero(duration / 2, Easing.Out);
                loadingSpinner.Hide();
            };
        }
    }
}
