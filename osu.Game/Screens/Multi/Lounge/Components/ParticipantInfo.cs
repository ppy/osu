// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class ParticipantInfo : Container
    {
        private readonly FillFlowContainer summaryContainer;

        public readonly IBindable<User> Host = new Bindable<User>();
        public readonly IBindable<IEnumerable<User>> Participants = new Bindable<IEnumerable<User>>();
        public readonly IBindable<int> ParticipantCount = new Bindable<int>();

        public ParticipantInfo()
        {
            OsuSpriteText summary;
            RelativeSizeAxes = Axes.X;
            Height = 15f;

            OsuSpriteText levelRangeHigher;
            OsuSpriteText levelRangeLower;
            Container flagContainer;
            LinkFlowContainer hostText;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5f, 0f),
                    Children = new Drawable[]
                    {
                        flagContainer = new Container
                        {
                            Width = 22f,
                            RelativeSizeAxes = Axes.Y,
                        },
                        /*new Container //todo: team banners
                        {
                            Width = 38f,
                            RelativeSizeAxes = Axes.Y,
                            CornerRadius = 2f,
                            Masking = true,
                            Children = new[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex(@"ad387e"),
                                },
                            },
                        },*/
                        hostText = new LinkFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both
                        }
                    },
                },
                summaryContainer = new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new[]
                    {
                        summary = new OsuSpriteText
                        {
                            Text = "0 participants",
                            TextSize = 14,
                        }
                    },
                },
            };

            Host.BindValueChanged(v =>
            {
                hostText.Clear();
                flagContainer.Clear();

                if (v != null)
                {
                    hostText.AddText("hosted by ");
                    hostText.AddLink(v.Username, null, LinkAction.OpenUserProfile, v.Id.ToString(), "Open profile", s => s.Font = "Exo2.0-BoldItalic");
                    flagContainer.Child = new DrawableFlag(v.Country) { RelativeSizeAxes = Axes.Both };
                }
            });

            ParticipantCount.BindValueChanged(v => summary.Text = $"{v:#,0}{" participant".Pluralize(v == 1)}");

            /*Participants.BindValueChanged(v =>
            {
                var ranks = v.Select(u => u.Statistics.Ranks.Global);
                levelRangeLower.Text = ranks.Min().ToString();
                levelRangeHigher.Text = ranks.Max().ToString();
            });*/
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            summaryContainer.Colour = colours.Gray9;
        }
    }
}
