// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapTitleWedge
    {
        public partial class FavouriteButton : OsuClickableContainer
        {
            private readonly BindableBool isFavourite = new BindableBool();

            private Box background = null!;
            private OsuSpriteText valueText = null!;
            private LoadingSpinner loadingSpinner = null!;
            private Box hoverLayer = null!;
            private HeartIcon icon = null!;

            private APIBeatmapSet? onlineBeatmapSet;
            private PostBeatmapFavouriteRequest? favouriteRequest;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            [Resolved]
            private INotificationOverlay? notifications { get; set; }

            internal LocalisableString Text => valueText.Text;

            public FavouriteButton()
            {
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Masking = true;
                CornerRadius = 5;
                Shear = OsuGame.SHEAR;

                AddRange(new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding { Left = 10, Right = 10, Vertical = 5f },
                        Spacing = new Vector2(4f, 0f),
                        Shear = -OsuGame.SHEAR,
                        Children = new Drawable[]
                        {
                            icon = new HeartIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(OsuFont.Style.Heading2.Size),
                            },
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.X,
                                Height = 20,
                                Children = new Drawable[]
                                {
                                    loadingSpinner = new LoadingSpinner
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(12f),
                                        State = { Value = Visibility.Visible },
                                    },
                                    new GridContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize, minSize: 25),
                                        },
                                        Content = new[]
                                        {
                                            new[]
                                            {
                                                valueText = new OsuSpriteText
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Font = OsuFont.Style.Heading2,
                                                    Colour = colourProvider.Content2,
                                                    Margin = new MarginPadding { Bottom = 2f },
                                                    AlwaysPresent = true,
                                                },
                                            }
                                        }
                                    },
                                },
                            },
                        },
                    },
                    hoverLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Colour = Colour4.White.Opacity(0.1f),
                        Blending = BlendingParameters.Additive,
                    },
                });
                Action = toggleFavourite;
            }

            protected override bool OnHover(HoverEvent e)
            {
                hoverLayer.FadeIn(500, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                hoverLayer.FadeOut(500, Easing.OutQuint);
            }

            public override LocalisableString TooltipText => isFavourite.Value ? BeatmapsetsStrings.ShowDetailsUnfavourite.ToSentence() : BeatmapsetsStrings.ShowDetailsFavourite.ToSentence();

            // Note: `setLoading()` and `setBeatmapSet()` are called externally via their public counterparts by song select when the beatmap changes,
            // as well as internally in order to display the progress and result of the (un)favourite operation when the button is clicked.
            // In case of external calls, we want to cancel pending favourite requests, primarily to avoid a situation when a late success callback from an (un)favourite
            // could show the favourite count from a prior beatmap.

            public void SetLoading()
            {
                if (favouriteRequest?.CompletionState == APIRequestCompletionState.Waiting)
                    favouriteRequest.Cancel();
                setLoading();
            }

            private void setLoading()
            {
                loadingSpinner.State.Value = Visibility.Visible;
                valueText.FadeOut(120, Easing.OutQuint);

                onlineBeatmapSet = null;
                updateFavouriteState();
            }

            public void SetBeatmapSet(APIBeatmapSet? beatmapSet)
            {
                if (favouriteRequest?.CompletionState == APIRequestCompletionState.Waiting)
                    favouriteRequest.Cancel();
                setBeatmapSet(beatmapSet);
            }

            private void setBeatmapSet(APIBeatmapSet? beatmapSet, bool withHeartAnimation = false)
            {
                loadingSpinner.State.Value = Visibility.Hidden;
                valueText.FadeIn(120, Easing.OutQuint);

                onlineBeatmapSet = beatmapSet;
                updateFavouriteState(withHeartAnimation);
            }

            private void updateFavouriteState(bool withAnimation = false)
            {
                Enabled.Value = onlineBeatmapSet != null;

                if (loadingSpinner.State.Value == Visibility.Hidden)
                    valueText.Text = onlineBeatmapSet?.FavouriteCount.ToLocalisableString(@"N0") ?? "-";

                isFavourite.Value = onlineBeatmapSet?.HasFavourited == true;

                background.FadeColour(isFavourite.Value ? colours.Pink4.Darken(1f).Opacity(0.5f) : Color4.Black.Opacity(0.2f), 500, Easing.OutQuint);
                valueText.FadeColour(isFavourite.Value ? colours.Pink1 : colourProvider.Content2, 500, Easing.OutQuint);
                icon.SetActive(isFavourite.Value, withAnimation);
            }

            private void toggleFavourite()
            {
                Debug.Assert(onlineBeatmapSet != null);

                // having this copy locally is important to capture this particular beatmap set instance rather than the field in the request success callback,
                // because if it was captured via the field / `this`, it could change value due to an external `setLoading()` or `setBeatmapSet()` call.
                // there's also the part where we want to call `setLoading()` here to show the spinner, but that also sets `onlineBeatmapSet` to null.
                var beatmapSet = onlineBeatmapSet;

                favouriteRequest = new PostBeatmapFavouriteRequest(beatmapSet.OnlineID, isFavourite.Value ? BeatmapFavouriteAction.UnFavourite : BeatmapFavouriteAction.Favourite);
                favouriteRequest.Success += () =>
                {
                    bool hasFavourited = favouriteRequest.Action == BeatmapFavouriteAction.Favourite;
                    beatmapSet.HasFavourited = hasFavourited;
                    beatmapSet.FavouriteCount += hasFavourited ? 1 : -1;

                    // if the beatmap set reference changed under the callback, abort visual updates to avoid showing stale data
                    if (onlineBeatmapSet == null || ReferenceEquals(beatmapSet, onlineBeatmapSet))
                        setBeatmapSet(beatmapSet, withHeartAnimation: hasFavourited);

                    api.LocalUserState.UpdateFavouriteBeatmapSets();
                };
                favouriteRequest.Failure += e =>
                {
                    notifications?.Post(new SimpleNotification
                    {
                        Text = e.Message,
                        Icon = FontAwesome.Solid.Times,
                    });

                    // if the beatmap set reference changed under the callback, abort visual updates to avoid showing stale data
                    if (onlineBeatmapSet == null || ReferenceEquals(beatmapSet, onlineBeatmapSet))
                        setBeatmapSet(beatmapSet, withHeartAnimation: false);
                };
                api.Queue(favouriteRequest);
                setLoading();
            }
        }

        private partial class HeartIcon : CompositeDrawable
        {
            private readonly SpriteIcon icon;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public HeartIcon()
            {
                InternalChildren = new Drawable[]
                {
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Regular.Heart,
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }

            private const double pop_out_duration = 100;
            private const double pop_in_duration = 500;

            private bool active;

            public void SetActive(bool active, bool withAnimation = false)
            {
                if (this.active == active)
                    return;

                this.active = active;

                FinishTransforms(true);

                if (active)
                {
                    transitionIcon(FontAwesome.Solid.Heart, colours.Pink1, emphasised: withAnimation);

                    if (withAnimation)
                        playFavouriteAnimation();
                }
                else
                {
                    transitionIcon(FontAwesome.Regular.Heart, colourProvider.Content2);
                }
            }

            private void transitionIcon(IconUsage newIcon, Color4 colour, bool emphasised = false)
            {
                icon.ScaleTo(emphasised ? 0.5f : 0.8f, pop_out_duration, Easing.OutQuad)
                    .Then()
                    .FadeColour(colour)
                    .Schedule(() => icon.Icon = newIcon)
                    .ScaleTo(1, pop_in_duration, Easing.OutElasticHalf);
            }

            private void playFavouriteAnimation()
            {
                var circle = new FastCircle
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.5f),
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                    Depth = float.MinValue,
                };

                AddInternal(circle);

                circle.Delay(pop_out_duration)
                      .FadeTo(0.35f)
                      .FadeOut(1400, Easing.OutCubic)
                      .ScaleTo(10f, 750, Easing.OutQuint)
                      .Expire();

                const int num_particles = 8;

                static float randomFloat(float min, float max) => min + Random.Shared.NextSingle() * (max - min);

                for (int i = 0; i < num_particles; i++)
                {
                    double duration = randomFloat(600, 1000);
                    float angle = (i + randomFloat(0, 0.75f)) / num_particles * MathF.PI * 2;
                    var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                    float distance = randomFloat(DrawWidth / 2, DrawWidth);

                    var particle = new FastCircle
                    {
                        Position = direction * DrawWidth / 4,
                        Size = new Vector2(3),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                        Depth = 2,
                        Colour = colours.Pink,
                    };

                    AddInternal(particle);

                    particle
                        .Delay(pop_out_duration)
                        .FadeTo(0.5f)
                        .MoveTo(direction * distance, 1300, Easing.OutQuint)
                        .FadeOut(duration, Easing.Out)
                        .ScaleTo(0.5f, duration)
                        .Expire();
                }
            }
        }
    }
}
