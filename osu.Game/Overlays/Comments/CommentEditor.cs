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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Comments
{
    public abstract partial class CommentEditor : CompositeDrawable
    {
        public Bindable<CommentableMeta?> CommentableMeta { get; set; } = new Bindable<CommentableMeta?>();

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
        protected abstract LocalisableString GetPlaceholderText();

        protected bool ShowLoadingSpinner
        {
            set
            {
                if (value)
                    loadingSpinner.Show();
                else
                    loadingSpinner.Hide();

                updateState();
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
            Current.BindValueChanged(_ => updateState());
            apiState.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            CommentableMeta.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            updateState();
        }

        protected abstract void OnCommit(string text);

        private void updateState()
        {
            bool isOnline = apiState.Value > APIState.Offline;
            LocalisableString? canNewCommentReason = CommentEditor.canNewCommentReason(CommentableMeta.Value);
            bool commentsDisabled = canNewCommentReason != null;
            bool canComment = isOnline && !commentsDisabled;

            if (!isOnline)
                TextBox.PlaceholderText = AuthorizationStrings.RequireLogin;
            else if (canNewCommentReason != null)
                TextBox.PlaceholderText = canNewCommentReason.Value;
            else
                TextBox.PlaceholderText = GetPlaceholderText();
            TextBox.ReadOnly = !canComment;

            if (isOnline)
            {
                commitButton.Show();
                commitButton.Enabled.Value = !commentsDisabled && loadingSpinner.State.Value == Visibility.Hidden && !string.IsNullOrEmpty(Current.Value);
                logInButton.Hide();
            }
            else
            {
                commitButton.Hide();
                logInButton.Show();
            }
        }

        // https://github.com/ppy/osu-web/blob/83816dbe24ad2927273cba968f2fcd2694a121a9/resources/js/components/comment-editor.tsx#L54-L60
        // careful here, logic is VERY finicky.
        private static LocalisableString? canNewCommentReason(CommentableMeta? meta)
        {
            if (meta == null)
                return null;

            if (meta.CurrentUserAttributes != null)
            {
                if (meta.CurrentUserAttributes.Value.CanNewCommentReason is string reason)
                    return reason;

                return null;
            }

            return AuthorizationStrings.CommentStoreDisabled;
        }

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
