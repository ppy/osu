// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mf;
using osu.Game.Overlays.OnlinePicture;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class PictureOverlay : OsuFocusedOverlayContainer
    {

        [Resolved]
        private GameHost host { get; set; }

        private const float DURATION = 1000;
        private float OriginalWidth;
        private float OriginalHeight;

        private string Target;
        
        private OnlinePictureContentContainer contentContainer;
        private Container bottomContainer;
        private TriangleButton openInBrowserButton;
        private TriangleButton closeButton;

        public float BottomContainerHeight => bottomContainer.Position.Y + bottomContainer.DrawHeight;

        public PictureOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Width = 0.9f;
            Height = 0.9f;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            CornerRadius = 10;
            Masking = true;
            Children = new Drawable[]
            {
                contentContainer = new OnlinePictureContentContainer
                {
                    GetBottomContainerHeight = () => BottomContainerHeight,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#444")),
                        },
                    }
                },
                bottomContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Height = 60f,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.5f),
                        Radius = 12,
                    },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#333"),
                            Alpha = 1f,
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
                                    Action = () => host.OpenUrlExternally(Target),
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
                }
            };
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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            
            OriginalWidth = DrawWidth;
            OriginalHeight = DrawHeight;
        }

        private void UpdateSize()
        {
            var NewHeight = pict.DrawHeight / OriginalHeight;
            var NewWidth = pict.DrawWidth / OriginalWidth;

            this.ResizeHeightTo( Math.Min(0.8f, NewHeight > 0.5f ? NewHeight : 0.5f), DURATION / 2, Easing.OutQuint );
            this.ResizeWidthTo( Math.Min(0.9f, NewWidth > 0.5f ? NewWidth : 0.5f), DURATION / 2, Easing.OutQuint );
        }

        private void ResetSize()
        {
            this.ResizeHeightTo( 0.8f, DURATION / 2, Easing.OutQuint );
            this.ResizeWidthTo( 0.9f, DURATION / 2, Easing.OutQuint );
        }

        DelayedLoadWrapper delayedLoadWrapper;
        UpdateableOnlinePicture pict;

        public void UpdateImage(string NewUri, bool popIn)
        {
            Target = NewUri;

            if (popIn)
                this.Show();

            if ( delayedLoadWrapper != null )
            {
                delayedLoadWrapper.FadeOut(DURATION / 2, Easing.OutQuint);
                delayedLoadWrapper.Expire();
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
            };
        }
    }
}
