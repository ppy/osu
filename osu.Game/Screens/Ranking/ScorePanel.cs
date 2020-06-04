// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Contracted;
using osu.Game.Screens.Ranking.Expanded;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking
{
    public class ScorePanel : CompositeDrawable, IStateful<PanelState>
    {
        /// <summary>
        /// Width of the panel when contracted.
        /// </summary>
        public const float CONTRACTED_WIDTH = 130;

        /// <summary>
        /// Height of the panel when contracted.
        /// </summary>
        private const float contracted_height = 355;

        /// <summary>
        /// Width of the panel when expanded.
        /// </summary>
        public const float EXPANDED_WIDTH = 360;

        /// <summary>
        /// Height of the panel when expanded.
        /// </summary>
        private const float expanded_height = 560;

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
        private const double resize_duration = 200;

        /// <summary>
        /// Delay after <see cref="resize_duration"/> before the top layer is expanded.
        /// </summary>
        private const double top_layer_expand_delay = 100;

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

        public event Action<PanelState> StateChanged;
        public readonly ScoreInfo Score;

        private Container content;

        private Container topLayerContainer;
        private Drawable topLayerBackground;
        private Container topLayerContentContainer;
        private Drawable topLayerContent;

        private Container middleLayerContainer;
        private Drawable middleLayerBackground;
        private Container middleLayerContentContainer;
        private Drawable middleLayerContent;

        public ScorePanel(ScoreInfo score)
        {
            Score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(40),
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
                                Child = middleLayerBackground = new Box { RelativeSizeAxes = Axes.Both }
                            },
                            middleLayerContentContainer = new Container { RelativeSizeAxes = Axes.Both }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (state == PanelState.Expanded)
            {
                topLayerBackground.FadeColour(expanded_top_layer_colour);
                middleLayerBackground.FadeColour(expanded_middle_layer_colour);
            }
            else
            {
                topLayerBackground.FadeColour(contracted_top_layer_colour);
                middleLayerBackground.FadeColour(contracted_middle_layer_colour);
            }

            updateState();
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

                if (LoadState >= LoadState.Ready)
                    updateState();

                StateChanged?.Invoke(value);
            }
        }

        private void updateState()
        {
            topLayerContent?.FadeOut(content_fade_duration).Expire();
            middleLayerContent?.FadeOut(content_fade_duration).Expire();

            switch (state)
            {
                case PanelState.Expanded:
                    Size = new Vector2(EXPANDED_WIDTH, expanded_height);

                    topLayerBackground.FadeColour(expanded_top_layer_colour, resize_duration, Easing.OutQuint);
                    middleLayerBackground.FadeColour(expanded_middle_layer_colour, resize_duration, Easing.OutQuint);

                    topLayerContentContainer.Add(middleLayerContent = new ExpandedPanelTopContent(Score.User).With(d => d.Alpha = 0));
                    middleLayerContentContainer.Add(topLayerContent = new ExpandedPanelMiddleContent(Score).With(d => d.Alpha = 0));
                    break;

                case PanelState.Contracted:
                    Size = new Vector2(CONTRACTED_WIDTH, contracted_height);

                    topLayerBackground.FadeColour(contracted_top_layer_colour, resize_duration, Easing.OutQuint);
                    middleLayerBackground.FadeColour(contracted_middle_layer_colour, resize_duration, Easing.OutQuint);

                    middleLayerContentContainer.Add(topLayerContent = new ContractedPanelMiddleContent(Score).With(d => d.Alpha = 0));
                    break;
            }

            content.ResizeTo(Size, resize_duration, Easing.OutQuint);

            bool topLayerExpanded = topLayerContainer.Y < 0;

            // If the top layer was already expanded, then we don't need to wait for the resize and can instead transform immediately. This looks better when changing the panel state.
            using (BeginDelayedSequence(topLayerExpanded ? 0 : resize_duration + top_layer_expand_delay, true))
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

        protected override bool OnClick(ClickEvent e)
        {
            if (State == PanelState.Contracted)
                State = PanelState.Expanded;

            return true;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            => base.ReceivePositionalInputAt(screenSpacePos)
               || topLayerContainer.ReceivePositionalInputAt(screenSpacePos)
               || middleLayerContainer.ReceivePositionalInputAt(screenSpacePos);
    }
}
