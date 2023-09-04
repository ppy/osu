// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Comments
{
    public abstract partial class CommentEditor : CompositeDrawable
    {
        private const int side_padding = 8;

        protected abstract LocalisableString FooterText { get; }

        protected FillFlowContainer ButtonsContainer { get; private set; } = null!;

        protected readonly Bindable<string> Current = new Bindable<string>(string.Empty);

        private RoundedButton commitButton = null!;
        private RoundedButton logInButton = null!;
        private LoadingSpinner loadingSpinner = null!;

        protected TextBox TextBox { get; private set; } = null!;

        [Resolved]
        protected IAPIProvider API { get; private set; } = null!;

        [Resolved]
        private LoginOverlay? loginOverlay { get; set; }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        /// <summary>
        /// Returns the text content of the main action button.
        /// When <paramref name="isLoggedIn"/> is <see langword="true"/>, the text will apply to a button that posts a comment.
        /// When <paramref name="isLoggedIn"/> is <see langword="false"/>, the text will apply to a button that directs the user to the login overlay.
        /// </summary>
        protected abstract LocalisableString GetButtonText(bool isLoggedIn);

        /// <summary>
        /// Returns the placeholder text for the comment box.
        /// </summary>
        /// <param name="isLoggedIn">Whether the current user is logged in.</param>
        protected abstract LocalisableString GetPlaceholderText(bool isLoggedIn);

        protected bool ShowLoadingSpinner
        {
            set
            {
                if (value)
                    loadingSpinner.Show();
                else
                    loadingSpinner.Hide();

                updateCommitButtonState();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
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
                        TextBox = new EditorTextBox
                        {
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            Current = Current
                        },
                        new Container
                        {
                            Name = @"Footer",
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
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5, 0),
                                    Children = new Drawable[]
                                    {
                                        ButtonsContainer = new FillFlowContainer
                                        {
                                            Name = @"Buttons",
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(5, 0),
                                            Children = new Drawable[]
                                            {
                                                commitButton = new EditorButton
                                                {
                                                    Action = () => OnCommit(Current.Value),
                                                    Text = GetButtonText(true)
                                                },
                                                logInButton = new EditorButton
                                                {
                                                    Width = 100,
                                                    Action = () => loginOverlay?.Show(),
                                                    Text = GetButtonText(false)
                                                }
                                            }
                                        },
                                        loadingSpinner = new LoadingSpinner
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Size = new Vector2(18),
                                        },
                                    }
                                },
                            }
                        }
                    }
                }
            });

            TextBox.OnCommit += (_, _) => commitButton.TriggerClick();
            apiState.BindTo(API.State);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(_ => updateCommitButtonState(), true);
            apiState.BindValueChanged(updateStateForLoggedIn, true);
        }

        protected abstract void OnCommit(string text);

        private void updateCommitButtonState() =>
            commitButton.Enabled.Value = loadingSpinner.State.Value == Visibility.Hidden && !string.IsNullOrEmpty(Current.Value);

        private void updateStateForLoggedIn(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            bool isAvailable = state.NewValue > APIState.Offline;

            TextBox.PlaceholderText = GetPlaceholderText(isAvailable);
            TextBox.ReadOnly = !isAvailable;

            if (isAvailable)
            {
                commitButton.Show();
                logInButton.Hide();
            }
            else
            {
                commitButton.Hide();
                logInButton.Show();
            }
        });

        private partial class EditorTextBox : OsuTextBox
        {
            protected override float LeftRightPadding => side_padding;

            protected override Color4 SelectionColour => Color4.Gray;

            private OsuSpriteText placeholder = null!;

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
        }

        protected partial class EditorButton : RoundedButton
        {
            public EditorButton()
            {
                Width = 80;
                Height = 25;
                Anchor = Anchor.CentreRight;
                Origin = Anchor.CentreRight;
            }

            protected override SpriteText CreateText()
            {
                var t = base.CreateText();
                t.Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 12);
                return t;
            }
        }
    }
}
