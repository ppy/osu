// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.Results
{
    public partial class PanelRoomAward : OsuClickableContainer
    {
        private readonly string text;
        private readonly string description;
        private readonly int userId;

        private Box glossLayer = null!;
        private Container scaleContainer = null!;

        public PanelRoomAward(string text, string description, int userId)
        {
            this.text = text;
            this.description = description;
            this.userId = userId;

            Height = 40;
            RelativeSizeAxes = Axes.X;

            // Just make hover sounds work for now.
            Action = () => { };
        }

        [BackgroundDependencyLoader]
        private void load(UserLookupCache userLookupCache, OverlayColourProvider colourProvider)
        {
            // Should be cached by this point.
            APIUser user = userLookupCache.GetUserAsync(userId).GetResultSafely()!;

            Child = scaleContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Padding = new MarginPadding(10),
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new MatchmakingAvatar(user)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Font = OsuFont.Style.Caption1,
                                        Text = user.Username
                                    },
                                    new OsuSpriteText
                                    {
                                        Font = OsuFont.Style.Caption2.With(weight: FontWeight.Bold),
                                        Text = text
                                    }
                                }
                            },
                        }
                    },
                    glossLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreRight,
                        Rotation = 30,
                        Scale = new Vector2(0.1f, 3),
                        Colour = ColourInfo.GradientHorizontal(
                            colourProvider.Background2.Opacity(0),
                            colourProvider.Background2),
                        Alpha = 0.1f,
                        Blending = BlendingParameters.Additive,
                    },
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            scaleContainer.ScaleTo(1.15f, 2000, Easing.OutPow10);
            glossLayer
                .FadeTo(0.05f, 2000, Easing.OutPow10)
                .MoveToX(-8, 2000, Easing.OutPow10);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            scaleContainer.ScaleTo(1f, 500, Easing.OutQuint);
            glossLayer
                .FadeTo(0.1f, 500, Easing.OutQuint)
                .MoveToX(0, 500, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        public override LocalisableString TooltipText => description;
    }
}
