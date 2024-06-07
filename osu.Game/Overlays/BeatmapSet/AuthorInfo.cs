// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class AuthorInfo : Container
    {
        private const float height = 50;

        private UpdateableAvatar avatar;
        private FillFlowContainer fields;

        private APIBeatmapSet beatmapSet;

        public APIBeatmapSet BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                beatmapSet = value;
                Scheduler.AddOnce(updateDisplay);
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;

            Children = new Drawable[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 4,
                    Masking = true,
                    Child = avatar = new UpdateableAvatar(showUserPanelOnHover: true, showGuestOnNull: false)
                    {
                        Size = new Vector2(height),
                    },
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.25f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 4,
                        Offset = new Vector2(0f, 1f),
                    },
                },
                fields = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = height + 5 },
                },
            };

            Scheduler.AddOnce(updateDisplay);
        }

        private void updateDisplay()
        {
            avatar.User = BeatmapSet?.Author;

            fields.Clear();
            if (BeatmapSet == null)
                return;

            fields.Children = new Drawable[]
            {
                new Field("mapped by", BeatmapSet.Author, OsuFont.GetFont(weight: FontWeight.Regular, italics: true)),
                new Field("submitted", BeatmapSet.Submitted, OsuFont.GetFont(weight: FontWeight.Bold))
                {
                    Margin = new MarginPadding { Top = 5 },
                },
            };

            if (BeatmapSet.Ranked.HasValue)
            {
                fields.Add(new Field(BeatmapSet.Status.ToString().ToLowerInvariant(), BeatmapSet.Ranked.Value, OsuFont.GetFont(weight: FontWeight.Bold)));
            }
            else if (BeatmapSet.LastUpdated.HasValue)
            {
                fields.Add(new Field("last updated", BeatmapSet.LastUpdated.Value, OsuFont.GetFont(weight: FontWeight.Bold)));
            }
        }

        private partial class Field : FillFlowContainer
        {
            public Field(string first, string second, FontUsage secondFont)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;

                Children = new[]
                {
                    new OsuSpriteText
                    {
                        Text = $"{first} ",
                        Font = OsuFont.GetFont(size: 11)
                    },
                    new OsuSpriteText
                    {
                        Text = second,
                        Font = secondFont.With(size: 11)
                    },
                };
            }

            public Field(string first, DateTimeOffset second, FontUsage secondFont)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;

                Children = new[]
                {
                    new OsuSpriteText
                    {
                        Text = $"{first} ",
                        Font = OsuFont.GetFont(size: 13)
                    },
                    new DrawableDate(second)
                    {
                        Font = secondFont.With(size: 13)
                    }
                };
            }

            public Field(string first, APIUser second, FontUsage secondFont)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;

                Children = new[]
                {
                    new LinkFlowContainer(s =>
                    {
                        s.Font = OsuFont.GetFont(size: 11);
                    }).With(d =>
                    {
                        d.AutoSizeAxes = Axes.Both;
                        d.AddText($"{first} ");
                        d.AddUserLink(second, s => s.Font = secondFont.With(size: 11));
                    }),
                };
            }
        }
    }
}
