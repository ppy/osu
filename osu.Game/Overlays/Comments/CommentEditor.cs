// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using System;
using osuTK;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Comments
{
    public abstract class CommentEditor : CompositeDrawable
    {
        private const int side_padding = 8;

        public Action<string> OnCommit;

        public bool IsLoading
        {
            get => commitButton.IsLoading;
            set => commitButton.IsLoading = value;
        }

        protected abstract string FooterText { get; }

        protected abstract string CommitButtonText { get; }

        protected abstract string TextBoxPlaceholder { get; }

        protected FillFlowContainer ButtonsContainer { get; private set; }

        protected readonly Bindable<string> Current = new Bindable<string>();

        private CommitButton commitButton;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            EditorTextBox textBox;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 6;
            BorderThickness = 3;
            BorderColour = colourProvider.Background3;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        textBox = new EditorTextBox
                        {
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            PlaceholderText = TextBoxPlaceholder,
                            Current = Current
                        },
                        new Container
                        {
                            Name = "Footer",
                            RelativeSizeAxes = Axes.X,
                            Height = 35,
                            Padding = new MarginPadding { Horizontal = side_padding },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                    Text = FooterText
                                },
                                ButtonsContainer = new FillFlowContainer
                                {
                                    Name = "Buttons",
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5, 0),
                                    Child = commitButton = new CommitButton(CommitButtonText)
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Action = () =>
                                        {
                                            OnCommit?.Invoke(Current.Value);
                                            Current.Value = string.Empty;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            textBox.OnCommit += (u, v) =>
            {
                if (commitButton.IsBlocked.Value)
                    return;

                commitButton.Click();
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(text => commitButton.IsBlocked.Value = string.IsNullOrEmpty(text.NewValue), true);
        }

        private class EditorTextBox : BasicTextBox
        {
            protected override float LeftRightPadding => side_padding;

            protected override Color4 SelectionColour => Color4.Gray;

            private OsuSpriteText placeholder;

            public EditorTextBox()
            {
                Masking = false;
                TextContainer.Height = 0.4f;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                BackgroundUnfocused = BackgroundFocused = colourProvider.Background5;
                placeholder.Colour = colourProvider.Background3;
                BackgroundCommit = colourProvider.Background3;
            }

            protected override SpriteText CreatePlaceholder() => placeholder = new OsuSpriteText
            {
                Font = OsuFont.GetFont(weight: FontWeight.Regular),
            };

            protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
            {
                AutoSizeAxes = Axes.Both,
                Child = new OsuSpriteText { Text = c.ToString(), Font = OsuFont.GetFont(size: CalculatedTextSize) },
            };
        }

        private class CommitButton : LoadingButton
        {
            private const int duration = 200;

            public readonly BindableBool IsBlocked = new BindableBool();

            public override bool PropagatePositionalInputSubTree => !IsBlocked.Value && base.PropagatePositionalInputSubTree;

            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            private OsuSpriteText drawableText;
            private Box background;
            private Box blockedBackground;

            public CommitButton(string text)
            {
                AutoSizeAxes = Axes.Both;
                LoadingAnimationSize = new Vector2(10);

                drawableText.Text = text;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                IdleColour = colourProvider.Light4;
                HoverColour = colourProvider.Light3;
                blockedBackground.Colour = colourProvider.Background5;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                IsBlocked.BindValueChanged(onBlockedStateChanged, true);
            }

            private void onBlockedStateChanged(ValueChangedEvent<bool> isBlocked)
            {
                drawableText.FadeColour(isBlocked.NewValue ? colourProvider.Foreground1 : Color4.White, duration, Easing.OutQuint);
                background.FadeTo(isBlocked.NewValue ? 0 : 1, duration, Easing.OutQuint);
            }

            protected override Drawable CreateContent() => new CircularContainer
            {
                Masking = true,
                Height = 25,
                AutoSizeAxes = Axes.X,
                Children = new Drawable[]
                {
                    blockedBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0
                    },
                    drawableText = new OsuSpriteText
                    {
                        AlwaysPresent = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                        Margin = new MarginPadding { Horizontal = 20 }
                    }
                }
            };

            protected override void OnLoadStarted() => drawableText.FadeOut(duration, Easing.OutQuint);

            protected override void OnLoadFinished() => drawableText.FadeIn(duration, Easing.OutQuint);
        }
    }
}
