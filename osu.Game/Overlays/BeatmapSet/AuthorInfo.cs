// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class AuthorInfo : Container
    {
        private const float height = 50;

        private readonly UpdateableAvatar avatar;
        private readonly FillFlowContainer fields;

        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                beatmapSet = value;

                updateDisplay();
            }
        }

        private void updateDisplay()
        {
            avatar.User = BeatmapSet?.Metadata.Author;

            fields.Clear();
            if (BeatmapSet == null)
                return;

            var online = BeatmapSet.OnlineInfo;

            fields.Children = new Drawable[]
            {
                new Field("mapped by", BeatmapSet.Metadata.Author.Username, OsuFont.GetFont(weight: FontWeight.Regular, italics: true)),
                new Field("submitted on", online.Submitted.ToString(@"MMMM d, yyyy"), OsuFont.GetFont(weight: FontWeight.Bold))
                {
                    Margin = new MarginPadding { Top = 5 },
                },
            };

            if (online.Ranked.HasValue)
            {
                fields.Add(new Field("ranked on", online.Ranked.Value.ToString(@"MMMM d, yyyy"), OsuFont.GetFont(weight: FontWeight.Bold)));
            }
            else if (online.LastUpdated.HasValue)
            {
                fields.Add(new Field("last updated on", online.LastUpdated.Value.ToString(@"MMMM d, yyyy"), OsuFont.GetFont(weight: FontWeight.Bold)));
            }
        }

        public AuthorInfo()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;

            Children = new Drawable[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 3,
                    Masking = true,
                    Child = avatar = new UpdateableAvatar
                    {
                        ShowGuestOnNull = false,
                        Size = new Vector2(height),
                    },
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.25f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 3,
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
        }

        private void load()
        {
            updateDisplay();
        }

        private class Field : FillFlowContainer
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
                        Font = OsuFont.GetFont(size: 13)
                    },
                    new OsuSpriteText
                    {
                        Text = second,
                        Font = secondFont.With(size: 13)
                    },
                };
            }
        }
    }
}
