// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Ranking
{
    public partial class UserTagControl
    {
        public partial class DrawableUserTag : OsuAnimatedButton
        {
            /// <summary>
            /// Minimum count of votes required to display a tag on the beatmap's page.
            /// Should match value specified web-side as https://github.com/ppy/osu-web/blob/cae2fdf03cfb8c30c8e332cfb142e03188ceffef/config/osu.php#L59.
            /// </summary>
            public const int MIN_VOTES_DISPLAY = 5;

            public readonly UserTag UserTag;

            public Action<UserTag>? OnSelected { get; set; }

            private readonly Bindable<int> voteCount = new Bindable<int>();
            private readonly BindableBool voted = new BindableBool();
            private readonly Bindable<bool> confirmed = new BindableBool();
            private readonly BindableBool updating = new BindableBool();

            protected Box MainBackground { get; private set; } = null!;
            private Box voteBackground = null!;

            protected OsuSpriteText TagCategoryText { get; private set; } = null!;
            protected OsuSpriteText TagNameText { get; private set; } = null!;

            private VoteCountText voteCountText = null!;

            private readonly bool showVoteCount;

            private LoadingLayer loadingLayer = null!;

            private FillFlowContainer contentFlow = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public DrawableUserTag(UserTag userTag, bool showVoteCount = true)
            {
                UserTag = userTag;
                this.showVoteCount = showVoteCount;
                voteCount.BindTo(userTag.VoteCount);
                updating.BindTo(userTag.Updating);
                voted.BindTo(userTag.Voted);

                ScaleOnMouseDown = 0.95f;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                CornerRadius = 5;
                Masking = true;

                EdgeEffect = new EdgeEffectParameters
                {
                    Colour = colours.Lime1,
                    Radius = 6,
                    Type = EdgeEffectType.Glow,
                };

                Content.AddRange(new Drawable[]
                {
                    MainBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = float.MaxValue,
                    },
                    contentFlow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Children = new[]
                        {
                            TagCategoryText = new OsuSpriteText
                            {
                                Alpha = UserTag.GroupName != null ? 0.6f : 0,
                                Text = UserTag.GroupName ?? default(LocalisableString),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Horizontal = 6 }
                            },
                            new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.1f,
                                        Blending = BlendingParameters.Additive,
                                    },
                                    TagNameText = new OsuSpriteText
                                    {
                                        Text = UserTag.DisplayName,
                                        Font = OsuFont.Default.With(weight: FontWeight.SemiBold),
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Margin = new MarginPadding { Horizontal = 6, Vertical = 3, },
                                    },
                                }
                            },
                            showVoteCount
                                ? new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Children = new Drawable[]
                                    {
                                        voteBackground = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        voteCountText = new VoteCountText(voteCount)
                                        {
                                            Margin = new MarginPadding { Horizontal = 6 },
                                        },
                                    }
                                }
                                : Empty(),
                        }
                    },
                    loadingLayer = new LoadingLayer(dimBackground: true),
                });

                TooltipText = UserTag.Description;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                const double transition_duration = 300;

                updating.BindValueChanged(u => loadingLayer.State.Value = u.NewValue ? Visibility.Visible : Visibility.Hidden);

                if (showVoteCount)
                {
                    voteCount.BindValueChanged(_ =>
                    {
                        confirmed.Value = voteCount.Value >= MIN_VOTES_DISPLAY;
                    }, true);
                    voted.BindValueChanged(v =>
                    {
                        if (v.NewValue)
                        {
                            voteBackground.FadeColour(colours.Lime2, transition_duration, Easing.OutQuint);
                            voteCountText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                        }
                        else
                        {
                            voteBackground.FadeColour(colours.Gray2, transition_duration, Easing.OutQuint);
                            voteCountText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                        }
                    }, true);

                    confirmed.BindValueChanged(c =>
                    {
                        if (c.NewValue)
                        {
                            MainBackground.FadeColour(colours.Lime2, transition_duration, Easing.OutQuint);
                            TagCategoryText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                            TagNameText.FadeColour(Colour4.Black, transition_duration, Easing.OutQuint);
                            FadeEdgeEffectTo(0.3f, transition_duration, Easing.OutQuint);
                        }
                        else
                        {
                            MainBackground.FadeColour(colours.Gray6, transition_duration, Easing.OutQuint);
                            TagCategoryText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                            TagNameText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                            FadeEdgeEffectTo(0f, transition_duration, Easing.OutQuint);
                        }
                    }, true);
                }

                FinishTransforms(true);

                Action = () => OnSelected?.Invoke(UserTag);
            }

            protected override void Update()
            {
                base.Update();

                // Grab size from the actual flow. If we were to use AutoSize, the mouse down animation would cause
                // our size to change, resulting in weird fill flow interactions.
                Size = contentFlow.Size;
            }

            private partial class VoteCountText : CompositeDrawable
            {
                private OsuSpriteText? text;

                private readonly Bindable<int> voteCount;

                public VoteCountText(Bindable<int> voteCount)
                {
                    RelativeSizeAxes = Axes.Y;
                    AutoSizeAxes = Axes.X;

                    this.voteCount = voteCount.GetBoundCopy();
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    voteCount.BindValueChanged(count =>
                    {
                        OsuSpriteText? previousText = text;

                        AddInternal(text = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                            Text = voteCount.Value.ToLocalisableString(),
                        });

                        if (previousText != null)
                        {
                            const double transition_duration = 500;

                            bool isIncrease = count.NewValue > count.OldValue;

                            text.MoveToY(isIncrease ? 20 : -20)
                                .MoveToY(0, transition_duration, Easing.OutExpo);

                            previousText.BypassAutoSizeAxes = Axes.Both;
                            previousText.MoveToY(isIncrease ? -20 : 20, transition_duration, Easing.OutExpo).Expire();

                            AutoSizeDuration = 300;
                            AutoSizeEasing = Easing.OutQuint;
                        }
                    }, true);
                }
            }
        }
    }
}
