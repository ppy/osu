// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerSkipOverlay : SkipOverlay
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Button skipButton = null!;

        public MultiplayerSkipOverlay(double startTime)
            : base(startTime)
        {
        }

        protected override OsuClickableContainer CreateButton() => skipButton = new Button
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            skipButton.Enabled.BindValueChanged(e =>
            {
                RemainingTimeBox.Colour = e.NewValue ? colours.Orange3 : Button.COLOUR_GRAY;
            }, true);

            client.UserLeft += onUserLeft;
            client.UserStateChanged += onUserStateChanged;
            client.UserVotedToSkipIntro += onUserVotedToSkipIntro;

            updateCount();
        }

        private void onUserLeft(MultiplayerRoomUser user) => Schedule(updateCount);

        private void onUserStateChanged(MultiplayerRoomUser user, MultiplayerUserState state) => Schedule(updateCount);

        private void onUserVotedToSkipIntro(int userId, bool voted) => Schedule(() =>
        {
            FadingContent.TriggerShow();
            updateCount();
        });

        private void updateCount()
        {
            if (client.Room == null || client.Room.Settings.AutoSkip)
                return;

            int countTotal = client.Room.Users.Count(u => u.State == MultiplayerUserState.Playing);
            int countSkipped = client.Room.Users.Count(u => u.State == MultiplayerUserState.Playing && u.VotedToSkipIntro);
            int countRequired = countTotal / 2 + 1;

            skipButton.SkippedCount.Value = Math.Min(countRequired, countSkipped);
            skipButton.RequiredCount.Value = countRequired;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.UserLeft -= onUserLeft;
                client.UserStateChanged -= onUserStateChanged;
                client.UserVotedToSkipIntro -= onUserVotedToSkipIntro;
            }
        }

        public partial class Button : OsuClickableContainer
        {
            private const float chevron_y = 0.4f;
            private const float secondary_y = 0.7f;

            public static readonly Color4 COLOUR_GRAY = OsuColour.Gray(0.4f);

            private Box background = null!;
            private Box box = null!;
            private TrianglesV2 triangles = null!;
            private OsuSpriteText countText = null!;
            private OsuSpriteText skipText = null!;
            private AspectContainer aspect = null!;

            private FillFlowContainer chevrons = null!;

            private Sample sampleConfirm = null!;

            public readonly BindableInt SkippedCount = new BindableInt();
            public readonly BindableInt RequiredCount = new BindableInt();

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public Button()
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                sampleConfirm = audio.Samples.Get(@"UI/submit-select");

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        Alpha = 0.2f,
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                    aspect = new AspectContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Height = 0.6f,
                        Masking = true,
                        CornerRadius = 15,
                        Children = new Drawable[]
                        {
                            box = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            triangles = new TrianglesV2
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            countText = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                RelativePositionAxes = Axes.Y,
                                Y = 0.35f,
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 24),
                                Origin = Anchor.Centre,
                            },
                            chevrons = new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                RelativePositionAxes = Axes.Y,
                                AutoSizeAxes = Axes.Both,
                                Origin = Anchor.Centre,
                                Direction = FillDirection.Horizontal,
                                Children = new[]
                                {
                                    new SpriteIcon { Size = new Vector2(15), Shadow = true, Icon = FontAwesome.Solid.ChevronRight },
                                    new SpriteIcon { Size = new Vector2(15), Shadow = true, Icon = FontAwesome.Solid.ChevronRight },
                                    new SpriteIcon { Size = new Vector2(15), Shadow = true, Icon = FontAwesome.Solid.ChevronRight },
                                }
                            },
                            skipText = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                RelativePositionAxes = Axes.Y,
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 12),
                                Origin = Anchor.Centre,
                                Text = @"SKIP",
                                Y = secondary_y,
                            },
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SkippedCount.BindValueChanged(_ => updateCount());
                RequiredCount.BindValueChanged(_ => updateCount(), true);
                Enabled.BindValueChanged(_ => updateColours(), true);

                FinishTransforms(true);
            }

            private void updateChevronsSpacing()
            {
                if (SkippedCount.Value > 0 && RequiredCount.Value > 1)
                    chevrons.TransformSpacingTo(new Vector2(-5f), 500, Easing.OutQuint);
                else
                    chevrons.TransformSpacingTo(IsHovered ? new Vector2(5f) : new Vector2(0f), 500, Easing.OutQuint);
            }

            private void updateCount()
            {
                if (SkippedCount.Value > 0 && RequiredCount.Value > 1)
                {
                    countText.FadeIn(300, Easing.OutQuint);
                    countText.Text = $"{SkippedCount.Value} / {RequiredCount.Value}";

                    chevrons.ScaleTo(0.5f, 300, Easing.OutQuint)
                            .MoveTo(new Vector2(-11, secondary_y), 300, Easing.OutQuint);

                    skipText.MoveToX(11f, 300, Easing.OutQuint);
                }
                else
                {
                    countText.FadeOut(300, Easing.OutQuint);

                    chevrons.ScaleTo(1f, 300, Easing.OutQuint)
                            .MoveTo(new Vector2(0, chevron_y), 300, Easing.OutQuint);

                    skipText.MoveToX(0f, 300, Easing.OutQuint);
                }

                updateChevronsSpacing();
                updateColours();
            }

            private void updateColours()
            {
                if (!Enabled.Value)
                {
                    box.FadeColour(COLOUR_GRAY, 500, Easing.OutQuint);
                    triangles.FadeColour(ColourInfo.GradientVertical(COLOUR_GRAY.Lighten(0.2f), COLOUR_GRAY), 500, Easing.OutQuint);
                }
                else
                {
                    box.FadeColour(IsHovered ? colours.Orange3.Lighten(0.2f) : colours.Orange3, 500, Easing.OutQuint);
                    triangles.FadeColour(ColourInfo.GradientVertical(colours.Orange3.Lighten(0.2f), colours.Orange3), 500, Easing.OutQuint);
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (Enabled.Value)
                {
                    updateChevronsSpacing();
                    updateColours();
                    background.FadeTo(0.4f, 500, Easing.OutQuint);
                }

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateChevronsSpacing();
                updateColours();
                background.FadeTo(0.2f, 500, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (Enabled.Value)
                    aspect.ScaleTo(0.75f, 2000, Easing.OutQuint);
                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (Enabled.Value)
                    aspect.ScaleTo(1, 1000, Easing.OutElastic);
                base.OnMouseUp(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!Enabled.Value)
                    return false;

                sampleConfirm.Play();

                box.FlashColour(Color4.White, 500, Easing.OutQuint);
                aspect.ScaleTo(1.2f, 2000, Easing.OutQuint);

                base.OnClick(e);

                Enabled.Value = false;
                return true;
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                countText.Scale = new Vector2(Math.Min(0.85f * aspect.DrawWidth / countText.DrawWidth, 1));
            }
        }
    }
}
