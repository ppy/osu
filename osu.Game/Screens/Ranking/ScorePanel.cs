// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Contracted;
using osu.Game.Screens.Ranking.Expanded;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking
{
    public partial class ScorePanel : CompositeDrawable, IStateful<PanelState>
    {
        /// <summary>
        /// Width of the panel when contracted.
        /// </summary>
        public const float CONTRACTED_WIDTH = 130;

        /// <summary>
        /// Height of the panel when contracted.
        /// </summary>
        private const float contracted_height = 385;

        /// <summary>
        /// Width of the panel when expanded.
        /// </summary>
        public const float EXPANDED_WIDTH = 360;

        /// <summary>
        /// Height of the panel when expanded.
        /// </summary>
        private const float expanded_height = 586;

        /// <summary>
        /// Height of the top layer when the panel is expanded.
        /// </summary>
        private const float expanded_top_layer_height = 53;

        /// <summary>
        /// Height of the top layer when the panel is contracted.
        /// </summary>
        private const float contracted_top_layer_height = 30;

        /// <summary>
        /// Duration for the panel to resize into its expanded/contracted size.
        /// </summary>
        public const double RESIZE_DURATION = 200;

        /// <summary>
        /// Delay after <see cref="RESIZE_DURATION"/> before the top layer is expanded.
        /// </summary>
        public const double TOP_LAYER_EXPAND_DELAY = 100;

        /// <summary>
        /// Duration for the top layer expansion.
        /// </summary>
        private const double top_layer_expand_duration = 200;

        /// <summary>
        /// Duration for the panel contents to fade in.
        /// </summary>
        private const double content_fade_duration = 50;

        private static readonly ColourInfo expanded_top_layer_colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#444"), Color4Extensions.FromHex("#333"));
        private static readonly ColourInfo expanded_middle_layer_colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#333"));
        private static readonly Color4 contracted_top_layer_colour = Color4Extensions.FromHex("#353535");
        private static readonly Color4 contracted_middle_layer_colour = Color4Extensions.FromHex("#353535");

        [CanBeNull]
        public event Action<PanelState> StateChanged;

        /// <summary>
        /// The position of the score in the rankings.
        /// </summary>
        public readonly Bindable<int?> ScorePosition = new Bindable<int?>();

        /// <summary>
        /// An action to be invoked if this <see cref="ScorePanel"/> is clicked while in an expanded state.
        /// </summary>
        public Action PostExpandAction;

        public readonly ScoreInfo Score;

        [Resolved]
        private OsuGameBase game { get; set; }

        private AudioContainer audioContent;

        private bool displayWithFlair;

        private Container topLayerContainer;
        private Drawable topLayerBackground;
        private Container topLayerContentContainer;
        private Drawable topLayerContent;

        private Container middleLayerContainer;
        private Drawable middleLayerBackground;
        private Container middleLayerContentContainer;
        private Drawable middleLayerContent;

        private DrawableSample samplePanelFocus;

        public ScorePanel(ScoreInfo score, bool isNewLocalScore = false)
        {
            Score = score;
            displayWithFlair = isNewLocalScore;

            ScorePosition.Value = score.Position;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            // ScorePanel doesn't include the top extruding area in its own size.
            // Adding a manual offset here allows the expanded version to take on an "acceptable" vertical centre when at 100% UI scale.
            const float vertical_fudge = 20;

            InternalChild = audioContent = new AudioContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(40),
                Y = vertical_fudge,
                Children = new Drawable[]
                {
                    topLayerContainer = new Container
                    {
                        Name = "Top layer",
                        RelativeSizeAxes = Axes.X,
                        Alpha = 0,
                        Height = 120,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                CornerRadius = 20,
                                CornerExponent = 2.5f,
                                Masking = true,
                                Child = topLayerBackground = new Box { RelativeSizeAxes = Axes.Both }
                            },
                            topLayerContentContainer = new Container { RelativeSizeAxes = Axes.Both }
                        }
                    },
                    middleLayerContainer = new Container
                    {
                        Name = "Middle layer",
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                CornerRadius = 20,
                                CornerExponent = 2.5f,
                                Masking = true,
                                Children = new[]
                                {
                                    middleLayerBackground = new Box { RelativeSizeAxes = Axes.Both },
                                    new UserCoverBackground
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        User = Score.User,
                                        Colour = ColourInfo.GradientVertical(Color4.White.Opacity(0.5f), Color4Extensions.FromHex("#444").Opacity(0))
                                    }
                                }
                            },
                            middleLayerContentContainer = new Container { RelativeSizeAxes = Axes.Both }
                        }
                    },
                    samplePanelFocus = new DrawableSample(audio.Samples.Get(@"Results/score-panel-focus"))
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();

            topLayerBackground.FinishTransforms(false, nameof(Colour));
            middleLayerBackground.FinishTransforms(false, nameof(Colour));
        }

        private PanelState state = PanelState.Contracted;

        public PanelState State
        {
            get => state;
            set
            {
                if (state == value)
                    return;

                state = value;

                if (IsLoaded)
                {
                    updateState();

                    if (value == PanelState.Expanded)
                        playAppearSample();
                }

                StateChanged?.Invoke(value);
            }
        }

        protected override void Update()
        {
            base.Update();
            audioContent.Balance.Value = ((ScreenSpaceDrawQuad.Centre.X / game.ScreenSpaceDrawQuad.Width) * 2 - 1) * OsuGameBase.SFX_STEREO_STRENGTH;
        }

        private void playAppearSample()
        {
            var channel = samplePanelFocus?.GetChannel();
            if (channel == null) return;

            channel.Frequency.Value = 0.99 + RNG.NextDouble(0.2);
            channel.Play();
        }

        private void updateState()
        {
            topLayerContent?.FadeOut(content_fade_duration).Expire();
            middleLayerContent?.FadeOut(content_fade_duration).Expire();

            switch (state)
            {
                case PanelState.Expanded:
                    Size = new Vector2(EXPANDED_WIDTH, expanded_height);

                    topLayerBackground.FadeColour(expanded_top_layer_colour, RESIZE_DURATION, Easing.OutQuint);
                    middleLayerBackground.FadeColour(expanded_middle_layer_colour, RESIZE_DURATION, Easing.OutQuint);

                    bool firstLoad = topLayerContent == null;
                    topLayerContentContainer.Add(topLayerContent = new ExpandedPanelTopContent(Score.User, firstLoad) { Alpha = 0 });
                    middleLayerContentContainer.Add(middleLayerContent = new ExpandedPanelMiddleContent(Score, displayWithFlair) { Alpha = 0 });

                    // only the first expanded display should happen with flair.
                    displayWithFlair = false;
                    break;

                case PanelState.Contracted:
                    Size = new Vector2(CONTRACTED_WIDTH, contracted_height);

                    topLayerBackground.FadeColour(contracted_top_layer_colour, RESIZE_DURATION, Easing.OutQuint);
                    middleLayerBackground.FadeColour(contracted_middle_layer_colour, RESIZE_DURATION, Easing.OutQuint);

                    topLayerContentContainer.Add(topLayerContent = new ContractedPanelTopContent
                    {
                        ScorePosition = { BindTarget = ScorePosition },
                        Alpha = 0
                    });

                    middleLayerContentContainer.Add(middleLayerContent = new ContractedPanelMiddleContent(Score) { Alpha = 0 });
                    break;
            }

            audioContent.ResizeTo(Size, RESIZE_DURATION, Easing.OutQuint);

            bool topLayerExpanded = topLayerContainer.Y < 0;

            // If the top layer was already expanded, then we don't need to wait for the resize and can instead transform immediately. This looks better when changing the panel state.
            using (BeginDelayedSequence(topLayerExpanded ? 0 : RESIZE_DURATION + TOP_LAYER_EXPAND_DELAY))
            {
                topLayerContainer.FadeIn();

                switch (state)
                {
                    case PanelState.Expanded:
                        topLayerContainer.MoveToY(-expanded_top_layer_height / 2, top_layer_expand_duration, Easing.OutQuint);
                        middleLayerContainer.MoveToY(expanded_top_layer_height / 2, top_layer_expand_duration, Easing.OutQuint);
                        break;

                    case PanelState.Contracted:
                        topLayerContainer.MoveToY(-contracted_top_layer_height / 2, top_layer_expand_duration, Easing.OutQuint);
                        middleLayerContainer.MoveToY(contracted_top_layer_height / 2, top_layer_expand_duration, Easing.OutQuint);
                        break;
                }

                topLayerContent?.FadeIn(content_fade_duration);
                middleLayerContent?.FadeIn(content_fade_duration);
            }
        }

        public override Vector2 Size
        {
            get => base.Size;
            set
            {
                base.Size = value;

                // Auto-size isn't used to avoid 1-frame issues and because the score panel is removed/re-added to the container.
                if (trackingContainer != null)
                    trackingContainer.Size = value;
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (State == PanelState.Contracted)
            {
                State = PanelState.Expanded;
                return true;
            }

            PostExpandAction?.Invoke();

            return true;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            => base.ReceivePositionalInputAt(screenSpacePos)
               || topLayerContainer.ReceivePositionalInputAt(screenSpacePos)
               || middleLayerContainer.ReceivePositionalInputAt(screenSpacePos);

        private ScorePanelTrackingContainer trackingContainer;

        /// <summary>
        /// Creates a <see cref="ScorePanelTrackingContainer"/> which this <see cref="ScorePanel"/> can reside inside.
        /// The <see cref="ScorePanelTrackingContainer"/> will track the size of this <see cref="ScorePanel"/>.
        /// </summary>
        /// <remarks>
        /// This <see cref="ScorePanel"/> is immediately added as a child of the <see cref="ScorePanelTrackingContainer"/>.
        /// </remarks>
        /// <returns>The <see cref="ScorePanelTrackingContainer"/>.</returns>
        /// <exception cref="InvalidOperationException">If a <see cref="ScorePanelTrackingContainer"/> already exists.</exception>
        public ScorePanelTrackingContainer CreateTrackingContainer()
        {
            if (trackingContainer != null)
                throw new InvalidOperationException("A score panel container has already been created.");

            return trackingContainer = new ScorePanelTrackingContainer(this);
        }
    }
}
