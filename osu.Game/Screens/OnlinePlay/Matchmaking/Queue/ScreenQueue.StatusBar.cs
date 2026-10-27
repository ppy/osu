// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Layout;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class ScreenQueue
    {
        public partial class StatusBar : VisibilityContainer
        {
            private const int height = 60;
            private const int middle_thin_width_absolute = 500;
            private const int line_spacing = 6;

            private readonly IBindable<MatchmakingScreenState> currentState = new Bindable<MatchmakingScreenState>();

            private readonly Bindable<float> animationProgress = new Bindable<float>();

            private readonly Circle lineLeft;
            private readonly Circle lineRight;

            private readonly Circle lineMiddleThin;
            private readonly Circle lineMiddleThick;

            private readonly CircularContainer background;
            private readonly OsuSpriteText topText;
            private readonly OsuSpriteText bottomText;

            private ScheduledDelegate? queueTimerDelegate;

            private readonly LayoutValue layout = new LayoutValue(Invalidation.DrawSize);

            [Resolved]
            private QueueController queueController { get; set; } = null!;

            public StatusBar()
            {
                RelativeSizeAxes = Axes.X;

                // Setting absolute height so that the drawable does not jump around
                // due to empty `OsuSpriteText`s having zero height.
                Height = height;

                InternalChildren = new Drawable[]
                {
                    background = new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = middle_thin_width_absolute / 6f,
                        Height = height / 6f,
                        Masking = true,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Hollow = false,
                            Radius = height * 2,
                            Colour = Color4.Black.Opacity(0.5f),
                        },
                        Alpha = 0,
                        Child = new Box { Colour = Color4.Transparent },
                    },
                    lineLeft = new Circle
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Origin = Anchor.CentreRight,
                        Colour = Color4.White.Opacity(0.25f),
                    },
                    lineMiddleThin = new Circle
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.White.Opacity(0.5f),
                    },
                    lineMiddleThick = new Circle
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 4,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.White,
                    },
                    lineRight = new Circle
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Origin = Anchor.CentreLeft,
                        Colour = Color4.White.Opacity(0.25f),
                    },
                    topText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Y = -20,
                        Font = OsuFont.Torus.With(size: 18, weight: FontWeight.Bold),
                        Colour = Color4.White,
                        Spacing = new Vector2(4),
                        AlwaysPresent = true,
                    },
                    bottomText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Y = 20,
                        Font = OsuFont.Torus.With(size: 12, weight: FontWeight.Bold),
                        Colour = Color4.White.Opacity(0.5f),
                        Spacing = new Vector2(2),
                        AlwaysPresent = true,
                    },
                };

                AddLayout(layout);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                currentState.BindTo(queueController.CurrentState);
                currentState.BindValueChanged(e => onStateChanged(e.NewValue), true);

                animationProgress.BindValueChanged(e => updateAnimation(e.NewValue), true);
            }

            private void onStateChanged(MatchmakingScreenState newState)
            {
                queueTimerDelegate?.Cancel();
                queueTimerDelegate = null;

                switch (newState)
                {
                    case MatchmakingScreenState.Idle:
                        // Intentionally not setting `topText`/`bottomText` to empty here since it would
                        // override the disappear animations already done in `PopOut`.
                        break;

                    case MatchmakingScreenState.Queueing:
                        setText(topText, MatchmakingQueueStrings.Searching);
                        setText(bottomText, queueController.QueueTimer.Elapsed.ToFormattedDuration());

                        queueTimerDelegate = Scheduler.AddDelayed(() => bottomText.Text = queueController.QueueTimer.Elapsed.ToFormattedDuration(), 1000, true);

                        break;

                    case MatchmakingScreenState.PendingAccept:
                        setText(topText, MatchmakingQueueStrings.MatchFound);
                        setText(bottomText, string.Empty);
                        break;

                    case MatchmakingScreenState.AcceptedWaitingForRoom:
                        setText(topText, MatchmakingQueueStrings.MatchFound);
                        setText(bottomText, MatchmakingQueueStrings.WaitingForOpponents);

                        break;

                    case MatchmakingScreenState.InRoom:
                        setText(topText, MatchmakingQueueStrings.GoodLuck);
                        setText(bottomText, string.Empty);

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
                }
            }

            private void setText(SpriteText sprite, LocalisableString value)
            {
                if (sprite.Text == value.ToUpper())
                    return;

                // We don't want to fire a transform for updating the text if:
                // - we're in the middle of a `PopIn`/`PopOut` transform which is already
                //   (dis)appearing the `SpriteText`s on its own,
                // - we're currently hidden (in which case there's no point in running
                //   the transforms at all).
                if (Transforms.Any() || State.Value == Visibility.Hidden)
                {
                    sprite.Text = value.ToUpper();
                    return;
                }

                var sequence = sprite.Text == string.Empty
                    ? new TransformSequence<SpriteText>(sprite)
                    : sprite.FadeOut(150);

                sequence.Then().Schedule(() => sprite.Text = value.ToUpper())
                        .Delay(50).FadeIn(150);
            }

            private const int line_duration = 400;
            private const int text_duration = 250;
            private const int stagger = 150;
            private static readonly IEasingFunction easing = new CubicBezierEasingFunction(0, 0.9, 0.35, 1);

            protected override void PopIn()
            {
                this.TransformBindableTo(animationProgress, 1f, line_duration, easing);

                using (BeginDelayedSequence(line_duration + stagger))
                {
                    topText.FadeIn(text_duration);
                    bottomText.Delay(text_duration + stagger).FadeIn(text_duration);

                    background.FadeIn(text_duration * 2 + stagger);
                }
            }

            protected override void PopOut()
            {
                this.TransformBindableTo(animationProgress, 0f, line_duration, easing);

                topText.FadeOut(text_duration);
                bottomText.FadeOut(text_duration);
                background.FadeOut(text_duration);
            }

            protected override void Update()
            {
                base.Update();

                if (!layout.IsValid)
                {
                    lineMiddleThinWidth = middle_thin_width_absolute / DrawWidth;
                    lineSideWidth = (DrawWidth - middle_thin_width_absolute - line_spacing * 2) / 2;

                    lineSideProgressStart = (middle_thin_width_absolute + line_spacing) / DrawWidth;

                    // Anchor the left/right lines to the sides of the middle line.
                    // Since they origin in `CentreRight`/`CentreLeft` respectively,
                    // increasing `Width` will make them expand outwards from the anchor point.
                    lineLeft.RelativeAnchorPosition = new Vector2(lineSideWidth / DrawWidth, 0.5f);
                    lineRight.RelativeAnchorPosition = new Vector2((DrawWidth - lineSideWidth) / DrawWidth, 0.5f);

                    updateAnimation(animationProgress.Value);
                    layout.Validate();
                }

                if (currentState.Value == MatchmakingScreenState.Queueing)
                    updateMiddleThickWidth(animationProgress.Value);
            }

            private float lineMiddleThinWidth;
            private float lineSideWidth;

            /// <summary>
            /// Threshold of <see cref="animationProgress"/> at which the side lines
            /// should start being drawn.
            /// </summary>
            private float lineSideProgressStart;

            private void updateAnimation(float progress)
            {
                lineMiddleThin.Width = Math.Clamp(progress, 0, lineMiddleThinWidth);
                updateMiddleThickWidth(progress);

                lineLeft.Width = lineRight.Width = Interpolation.ValueAt(
                    Math.Clamp(progress, lineSideProgressStart, 1),
                    0,
                    lineSideWidth / DrawWidth,
                    lineSideProgressStart,
                    1
                );
            }

            /// <summary>
            /// Queue duration at which <see cref="lineMiddleThick"/> will reach its maximum width.
            /// </summary>
            private static readonly TimeSpan max_width_queue_duration = TimeSpan.FromMinutes(10);

            private const float middle_thick_width_min = 0.3f;
            private const float middle_thick_width_max = 0.7f;

            private void updateMiddleThickWidth(float progress)
            {
                float thinLineFraction = Interpolation.ValueAt(
                    Math.Clamp(queueController.QueueTimer.ElapsedMilliseconds, 0, max_width_queue_duration.TotalMilliseconds),
                    middle_thick_width_min,
                    middle_thick_width_max,
                    0,
                    max_width_queue_duration.TotalMilliseconds,
                    Easing.OutExpo
                );

                lineMiddleThick.Width = Math.Clamp(progress, 0, lineMiddleThinWidth * thinLineFraction);
            }
        }
    }
}
