// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
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

        private const float DURATION = 1000;

        private string Target;

        private OnlinePictureContentContainer contentContainer;
        private Container topbarContainer;
        private Container bottomContainer;
        private LoadingSpinner loadingSpinner;
        private TriangleButton openInBrowserButton;
        private TriangleButton closeButton;
        private OsuSpriteText infoText;
        private bool CanOpenInBrowser;

        private readonly OverlayColourProvider overlayColourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);

        private BindableBool OptUI = new BindableBool();

        public float BottomContainerHeight => bottomContainer.Position.Y + bottomContainer.DrawHeight;
        public float TopBarHeight => topbarContainer.DrawHeight;

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
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
                                    Action = () => OpenLink(Target),
                                },
                                closeButton = new TriangleButton
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.2f,
                                    Height = 0.75f,
                                    Text = "关闭",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Action = () => this.Hide(),
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

            config.BindWith(MfSetting.OptUI, OptUI);

            OptUI.BindValueChanged(OnOptUIChanged);
        }

        protected override void PopIn()
        {
            base.PopIn();

            this.FadeIn(DURATION, Easing.OutQuint);
            this.ScaleTo(1, DURATION, Easing.OutElastic);
        }

        protected override void PopOut()
        {
            base.PopOut();

            delayedLoadWrapper?.FadeOut(DURATION / 2, Easing.OutQuint);
            delayedLoadWrapper?.Expire();
            this.FadeOut(DURATION, Easing.OutQuint);
            this.ScaleTo(0.9f, DURATION, Easing.OutQuint);
        }

        DelayedLoadWrapper delayedLoadWrapper;
        UpdateableOnlinePicture pict;

        private void OnOptUIChanged(ValueChangedEvent<bool> v)
        {
            if ( this.Alpha != 0 && v.NewValue == false )
                if (OpenLink(Target))
                    this.Hide();
        }

        private bool OpenLink(string link)
        {
            if ( CanOpenInBrowser )
            {
                game?.OpenUrlExternally(Target);
                return true;
            }
            else
                return false;
        }

        public void UpdateImage(string NewUri, bool popIn, bool CanOpenInBrowser = true, string Title = null)
        {
            Target = NewUri;
            this.CanOpenInBrowser = CanOpenInBrowser;

            if ( OptUI.Value != true && CanOpenInBrowser )
            {
                OpenLink(Target);
                return;
            }

            if (popIn)
                this.Show();

            loadingSpinner.Show();

            if (delayedLoadWrapper != null)
            {
                delayedLoadWrapper.FadeOut(DURATION / 2, Easing.OutQuint);
                delayedLoadWrapper.Expire();
            }

            if ( Title != null )
                infoText.Text = Title;
            else
                infoText.Text = Target;

            if ( CanOpenInBrowser )
                openInBrowserButton.Action = () => OpenLink(Target);
            else
                openInBrowserButton.Action = null;

            foreach (var i in contentContainer)
            {
                i.Hide();
                i.Expire();
            }

            contentContainer.Add(delayedLoadWrapper = new DelayedLoadWrapper(
                pict = new UpdateableOnlinePicture(Target)
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
                d.FadeInFromZero(DURATION / 2, Easing.Out);
                loadingSpinner.Hide();
            };
        }
    }
}
