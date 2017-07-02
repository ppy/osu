// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Overlays.Chat
{
    public class ChatLine : Container
    {
        public readonly Message Message;

        private static readonly Color4[] username_colours = {
            OsuColour.FromHex("588c7e"),
            OsuColour.FromHex("b2a367"),
            OsuColour.FromHex("c98f65"),
            OsuColour.FromHex("bc5151"),
            OsuColour.FromHex("5c8bd6"),
            OsuColour.FromHex("7f6ab7"),
            OsuColour.FromHex("a368ad"),
            OsuColour.FromHex("aa6880"),

            OsuColour.FromHex("6fad9b"),
            OsuColour.FromHex("f2e394"),
            OsuColour.FromHex("f2ae72"),
            OsuColour.FromHex("f98f8a"),
            OsuColour.FromHex("7daef4"),
            OsuColour.FromHex("a691f2"),
            OsuColour.FromHex("c894d3"),
            OsuColour.FromHex("d895b0"),

            OsuColour.FromHex("53c4a1"),
            OsuColour.FromHex("eace5c"),
            OsuColour.FromHex("ea8c47"),
            OsuColour.FromHex("fc4f4f"),
            OsuColour.FromHex("3d94ea"),
            OsuColour.FromHex("7760ea"),
            OsuColour.FromHex("af52c6"),
            OsuColour.FromHex("e25696"),

            OsuColour.FromHex("677c66"),
            OsuColour.FromHex("9b8732"),
            OsuColour.FromHex("8c5129"),
            OsuColour.FromHex("8c3030"),
            OsuColour.FromHex("1f5d91"),
            OsuColour.FromHex("4335a5"),
            OsuColour.FromHex("812a96"),
            OsuColour.FromHex("992861"),
        };

        private Color4 getUsernameColour(Message message)
        {
            if (!string.IsNullOrEmpty(message.Sender?.Colour))
                return OsuColour.FromHex(message.Sender.Colour);

            //todo: use User instead of Message when user_id is correctly populated.
            return username_colours[message.UserId % username_colours.Length];
        }

        public const float LEFT_PADDING = message_padding + padding * 2;

        private const float padding = 15;
        private const float message_padding = 200;
        private const float text_size = 20;

        public ChatLine(Message message)
        {
            Message = message;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Left = padding, Right = padding };

            TextFlowContainer textContainer;

            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(message_padding, text_size),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = @"Exo2.0-SemiBold",
                            Text = $@"{Message.Timestamp.LocalDateTime:HH:mm:ss}",
                            FixedWidth = true,
                            TextSize = text_size * 0.75f,
                            Alpha = 0.4f,
                        },
                        new OsuSpriteText
                        {
                            Font = @"Exo2.0-BoldItalic",
                            Text = $@"{Message.Sender.Username}:",
                            Colour = getUsernameColour(Message),
                            TextSize = text_size,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = message_padding + padding },
                    Children = new Drawable[]
                    {
                        textContainer = new TextFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        }
                    }
                }
            };

            string toParse = string.Copy(message.Content);
            List<SplitMarker> markers = new List<SplitMarker>();
            markers.AddRange(processMarkers(ref toParse, "**", SplitType.Bold));
            markers.AddRange(processMarkers(ref toParse, "*", SplitType.Italic));
            markers.AddRange(processMarkers(ref toParse, "_", SplitType.Italic));

            // Add a sentinel marker for the end of the string such that the entire string is rendered
            // without requiring code duplication.
            markers.Add(new SplitMarker { Index = toParse.Length, Length = 0, Type = SplitType.None });

            // Sort markers from earliest to latest
            markers.Sort((a, b) => a.Index.CompareTo(b.Index));

            // Cut up string into parts according to all found markers
            int lastIndex = 0;
            bool bold = false, italic = false;
            foreach (var marker in markers)
            {
                // We do not need to add empty strings if we have 2 consecutive markers
                if (lastIndex != marker.Index)
                {
                    string font = "Exo2.0-" + (bold ? "Bold" : "Regular") + (italic ? "Italic" : string.Empty);
                    textContainer.AddText(message.Content.Substring(lastIndex, marker.Index - lastIndex), spriteText =>
                    {
                        spriteText.TextSize = text_size;
                        spriteText.Font = font;
                    });
                }

                lastIndex = marker.Index + marker.Length;

                // Switch bold / italic state based on the marker we just encountered.
                switch (marker.Type)
                {
                    case SplitType.Bold: bold = !bold; break;
                    case SplitType.Italic: italic = !italic; break;
                }
            }
        }

        enum SplitType
        {
            None,
            Italic,
            Bold,
        }

        struct SplitMarker
        {
            public int Index;
            public int Length;
            public SplitType Type;
        }

        private static List<SplitMarker> processMarkers(ref string toParse, string delimiter, SplitType type)
        {
            List<SplitMarker> output = new List<SplitMarker>();

            // For each char in toParse...
            for (int i = 0; i < toParse.Length; i++)
            {
                // ...check whether delimiter is matched char-by-char.
                for (int j = 0; j + i < toParse.Length && j < delimiter.Length; j++)
                {
                    if (toParse[j + i] != delimiter[j])
                        break;
                    else if (j == delimiter.Length - 1)
                    {
                        // Were we escaped? In this case put a marker skipping the escape character
                        if (i > 0 && toParse[i - 1] == '\\')
                            output.Add(new SplitMarker { Index = i-1, Type = SplitType.None, Length = 1 });
                        else
                        {
                            output.Add(new SplitMarker { Index = i, Type = type, Length = delimiter.Length });
                            i += delimiter.Length - 1; // Make sure we advance beyond the end of the discovered delimiter
                        }
                    }
                }
            }

            // Disregard trailing marker if we have an odd amount
            if (output.Count % 2 == 1)
                output.RemoveAt(output.Count - 1);

            // We replace all occurences of the delimiter with spaces such that further delimiters
            // potentially being a substring of the current delimiter parse correctly. This is needed
            // to parse both ** and * correctly in markdown. The replacement with spaces happens such that
            // the string remains the same length and indices remain valid.
            toParse = toParse.Replace(delimiter, new string(' ', delimiter.Length));

            return output;
        }
    }
}
