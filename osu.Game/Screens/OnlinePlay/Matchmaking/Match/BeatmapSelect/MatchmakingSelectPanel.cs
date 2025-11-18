// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public abstract partial class MatchmakingSelectPanel : Container
    {
        public const float WIDTH = 345;
        public const float HEIGHT = 80;

        public static readonly Vector2 SIZE = new Vector2(WIDTH, HEIGHT);

        public bool AllowSelection { get; set; }

        public readonly MultiplayerPlaylistItem Item;

        public Action<MultiplayerPlaylistItem>? Action { private get; init; }

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private const float border_width = 3;

        private Container scaleContainer = null!;
        private Drawable lighting = null!;
        private Container border = null!;

        protected MatchmakingSelectPanel(MultiplayerPlaylistItem item)
        {
            Item = item;
            Size = SIZE;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                scaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new[]
                    {
                        new Container
                        {
                            Masking = true,
                            CornerRadius = BeatmapCard.CORNER_RADIUS,
                            CornerExponent = 10,
                            RelativeSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                Content,
                                lighting = new Box
                                {
                                    Blending = BlendingParameters.Additive,
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                },
                            }
                        },
                        border = new Container
                        {
                            Alpha = 0,
                            Masking = true,
                            CornerRadius = BeatmapCard.CORNER_RADIUS,
                            CornerExponent = 10,
                            Blending = BlendingParameters.Additive,
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = border_width,
                            BorderColour = colourProvider.Light1,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Glow,
                                Radius = 40,
                                Roundness = 300,
                                Colour = colourProvider.Light3.Opacity(0.1f),
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    AlwaysPresent = true,
                                    Alpha = 0,
                                    Colour = Color4.Black,
                                    RelativeSizeAxes = Axes.Both,
                                },
                            }
                        },
                    }
                },
                new HoverClickSounds(),
            };
        }

        // TODO: making these abstract for now but avatar overlay should really be owned by the top level class
        public abstract void AddUser(APIUser user);

        public abstract void RemoveUser(APIUser user);

        protected override bool OnHover(HoverEvent e)
        {
            if (AllowSelection)
            {
                lighting.FadeTo(0.2f, 50)
                        .Then()
                        .FadeTo(0.1f, 300);
                return true;
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            lighting.FadeOut(200);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (AllowSelection && e.Button == MouseButton.Left)
                scaleContainer.ScaleTo(0.95f, 400, Easing.OutExpo);

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButton.Left)
                scaleContainer.ScaleTo(1f, 500, Easing.OutElasticHalf);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (AllowSelection)
            {
                lighting.FadeTo(0.5f, 50)
                        .Then()
                        .FadeTo(0.1f, 400);

                Action?.Invoke(Item);
            }

            return true;
        }

        public void ShowChosenBorder()
        {
            border.FadeTo(1, 1000, Easing.OutQuint);
        }

        public void ShowBorder()
        {
            border.FadeTo(1, 80, Easing.OutQuint)
                  .Then()
                  .FadeTo(0.7f, 800, Easing.OutQuint);
        }

        public void HideBorder()
        {
            border.FadeOut(500, Easing.OutQuint);
        }

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
