// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick
{
    public partial class BeatmapSelectionPanel : Container
    {
        private const float corner_radius = 6;
        private const float border_width = 3;

        public readonly MultiplayerPlaylistItem Item;

        private readonly Container scaleContainer;
        private readonly BeatmapPanel beatmapPanel;
        private readonly BeatmapSelectionOverlay selectionOverlay;
        private readonly Container border;
        private readonly Box flash;

        public bool AllowSelection;

        public Action<MultiplayerPlaylistItem>? Action;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        public override bool PropagatePositionalInputSubTree => AllowSelection;

        public BeatmapSelectionPanel(MultiplayerPlaylistItem item)
        {
            Item = item;
            Size = BeatmapPanel.SIZE;

            InternalChildren = new Drawable[]
            {
                scaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(-border_width),
                            Child = border = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = corner_radius + border_width,
                                Alpha = 0,
                                Child = new Box { RelativeSizeAxes = Axes.Both },
                            }
                        },
                        beatmapPanel = new BeatmapPanel
                        {
                            RelativeSizeAxes = Axes.Both,
                            OverlayLayer =
                            {
                                Children = new[]
                                {
                                    flash = new Box
                                    {
                                        Blending = BlendingParameters.Additive,
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0,
                                    },
                                }
                            }
                        },
                        selectionOverlay = new BeatmapSelectionOverlay
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Horizontal = 10 },
                            Origin = Anchor.CentreLeft,
                        },
                    }
                },
                new HoverClickSounds(),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapLookupCache.GetBeatmapAsync(Item.BeatmapID).ContinueWith(b => Schedule(() =>
            {
                var beatmap = b.GetResultSafely()!;

                beatmap.StarRating = Item.StarRating;

                beatmapPanel.Beatmap = beatmap;
            }));
        }

        public bool AddUser(APIUser user, bool isOwnUser = false) => selectionOverlay.AddUser(user, isOwnUser);

        public bool RemoveUser(int userId) => selectionOverlay.RemoveUser(userId);

        public bool RemoveUser(APIUser user) => RemoveUser(user.Id);

        protected override bool OnHover(HoverEvent e)
        {
            flash.FadeTo(0.2f, 50)
                 .Then()
                 .FadeTo(0.1f, 300);

            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            flash.FadeOut(200);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                scaleContainer.ScaleTo(0.95f, 400, Easing.OutExpo);
                return true;
            }

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButton.Left)
            {
                scaleContainer.ScaleTo(1f, 500, Easing.OutElasticHalf);
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke(Item);

            flash.FadeTo(0.5f, 50)
                 .Then()
                 .FadeTo(0.1f, 400);

            return true;
        }

        public void ShowBorder() => border.Show();

        public void HideBorder() => border.Hide();

        public void FadeInAndEnterFromBelow(double duration = 500, double delay = 0, float distance = 200)
        {
            scaleContainer
                .FadeOut()
                .MoveToY(distance)
                .Delay(delay)
                .FadeIn(duration / 2)
                .MoveToY(0, duration, Easing.OutExpo);
        }

        public void PopOutAndExpire(double duration = 400, double delay = 0, Easing easing = Easing.InCubic)
        {
            AllowSelection = false;

            scaleContainer.Delay(delay)
                          .ScaleTo(0, duration, easing)
                          .FadeOut(duration);

            this.Delay(delay + duration).FadeOut().Expire();
        }
    }
}
