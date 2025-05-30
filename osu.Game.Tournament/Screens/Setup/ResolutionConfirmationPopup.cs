// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Setup
{
    public partial class ResolutionConfirmationPopup : CompositeDrawable
    {
        private readonly Action? keepChangesAction;
        private readonly Action? revertAction;

        private ProgressBar countdownBar = null!;

        private readonly BindableDouble countdownProgress = new BindableDouble();

        public ResolutionConfirmationPopup(Action? keepChangesAction = null, Action? revertAction = null)
        {
            this.keepChangesAction = keepChangesAction;
            this.revertAction = revertAction;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.2f),
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 500,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.GreySeaFoamDark,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(15),
                            Padding = new MarginPadding(10),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "Keep changes?",
                                    Font = OsuFont.Torus.With(size: 40, weight: FontWeight.SemiBold),
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "Reverting to previous resolution in 15 seconds.",
                                    Font = OsuFont.Torus.With(size: 20),
                                },
                                countdownBar = new ProgressBar(false)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    EndTime = 15,
                                    Height = 6,
                                    Masking = true,
                                    CornerRadius = 3,
                                    FillColour = colours.Sky,
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10),
                                    Children = new Drawable[]
                                    {
                                        new DangerousRoundedButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "Keep changes",
                                            Width = 200,
                                            Action = () => invokeAndExpire(keepChangesAction),
                                        },
                                        new RoundedButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "Revert",
                                            Width = 200,
                                            Action = () => invokeAndExpire(revertAction),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }

        private void invokeAndExpire(Action? action)
        {
            action?.Invoke();
            this.FadeOut(500, Easing.OutQuint).Then().Expire();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            countdownProgress.BindValueChanged(p =>
            {
                countdownBar.CurrentTime = p.NewValue;

                if (p.NewValue == 15d)
                {
                    invokeAndExpire(revertAction);
                }
            });

            this.TransformBindableTo(countdownProgress, 15d, 15000);
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnScroll(ScrollEvent e) => true;
    }
}
