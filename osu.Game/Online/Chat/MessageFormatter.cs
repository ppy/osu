// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace osu.Game.Online.Chat
{
    public static class MessageFormatter
    {
        // [[Performance Points]] -> wiki:Performance Points (https://osu.ppy.sh/wiki/Performance_Points)
        private static readonly Regex wiki_regex = new Regex(@"\[\[(?<text>[^\]]+)\]\]");

        // (test)[https://osu.ppy.sh/b/1234] -> test (https://osu.ppy.sh/b/1234)
        private static readonly Regex old_link_regex = new Regex(@"\((?<text>(((?<=\\)[\(\)])|[^\(\)])*(((?<open>\()(((?<=\\)[\(\)])|[^\(\)])*)+((?<close-open>\))(((?<=\\)[\(\)])|[^\(\)])*)+)*(?(open)(?!)))\)\[(?<url>[a-z]+://[^ ]+)\]");

        // [https://osu.ppy.sh/b/1234 Beatmap [Hard] (poop)] -> Beatmap [hard] (poop) (https://osu.ppy.sh/b/1234)
        private static readonly Regex new_link_regex = new Regex(@"\[(?<url>[a-z]+://[^ ]+) (?<text>(((?<=\\)[\[\]])|[^\[\]])*(((?<open>\[)(((?<=\\)[\[\]])|[^\[\]])*)+((?<close-open>\])(((?<=\\)[\[\]])|[^\[\]])*)+)*(?(open)(?!)))\]");

        // [test](https://osu.ppy.sh/b/1234) -> test (https://osu.ppy.sh/b/1234) aka correct markdown format
        private static readonly Regex markdown_link_regex = new Regex(@"\[(?<text>(((?<=\\)[\[\]])|[^\[\]])*(((?<open>\[)(((?<=\\)[\[\]])|[^\[\]])*)+((?<close-open>\])(((?<=\\)[\[\]])|[^\[\]])*)+)*(?(open)(?!)))\]\((?<url>[a-z]+://[^ ]+)(\s+(?<title>""([^""]|(?<=\\)"")*""))?\)");

        // advanced, RFC-compatible regular expression that matches any possible URL, *but* allows certain invalid characters that are widely used
        // This is in the format (<required>, [optional]):
        //      http[s]://<domain>.<tld>[:port][/path][?query][#fragment]
        private static readonly Regex advanced_link_regex = new Regex(
            // protocol
            @"(?<link>[a-z]*?:\/\/" +
            // domain + tld
            @"(?<domain>(?:[a-z0-9]\.|[a-z0-9][a-z0-9-]*[a-z0-9]\.)*[a-z0-9-]*[a-z0-9]" +
            // port (optional)
            @"(?::\d+)?)" +
            // path (optional)
            @"(?<path>(?:(?:\/+(?:[a-z0-9$_\.\+!\*\',;:\(\)@&~=-]|%[0-9a-f]{2})*)*" +
            // query (optional)
            @"(?:\?(?:[a-z0-9$_\+!\*\',;:\(\)@&=\/~-]|%[0-9a-f]{2})*)?)?" +
            // fragment (optional)
            @"(?:#(?:[a-z0-9$_\+!\*\',;:\(\)@&=\/~-]|%[0-9a-f]{2})*)?)?)",
            RegexOptions.IgnoreCase);

        // 00:00:000 (1,2,3) - test
        private static readonly Regex time_regex = new Regex(@"\d\d:\d\d:\d\d\d? [^-]*");

        // #osu
        private static readonly Regex channel_regex = new Regex(@"(#[a-zA-Z]+[a-zA-Z0-9]+)");

        // Unicode emojis
        private static readonly Regex emoji_regex = new Regex(@"(\uD83D[\uDC00-\uDE4F])");

        private static void handleMatches(Regex regex, string display, string link, MessageFormatterResult result, int startIndex = 0, LinkAction? linkActionOverride = null, char[] escapeChars = null)
        {
            int captureOffset = 0;

            foreach (Match m in regex.Matches(result.Text, startIndex))
            {
                var index = m.Index - captureOffset;

                var displayText = string.Format(display,
                    m.Groups[0],
                    m.Groups["text"].Value,
                    m.Groups["url"].Value).Trim();

                var linkText = string.Format(link,
                    m.Groups[0],
                    m.Groups["text"].Value,
                    m.Groups["url"].Value).Trim();

                if (displayText.Length == 0 || linkText.Length == 0) continue;

                // Remove backslash escapes in front of the characters provided in escapeChars
                if (escapeChars != null)
                    displayText = escapeChars.Aggregate(displayText, (current, c) => current.Replace($"\\{c}", c.ToString()));

                // Check for encapsulated links
                if (result.Links.Find(l => (l.Index <= index && l.Index + l.Length >= index + m.Length) || (index <= l.Index && index + m.Length >= l.Index + l.Length)) == null)
                {
                    result.Text = result.Text.Remove(index, m.Length).Insert(index, displayText);

                    //since we just changed the line display text, offset any already processed links.
                    result.Links.ForEach(l => l.Index -= l.Index > index ? m.Length - displayText.Length : 0);

                    var details = getLinkDetails(linkText);
                    result.Links.Add(new Link(linkText, index, displayText.Length, linkActionOverride ?? details.Action, details.Argument));

                    //adjust the offset for processing the current matches group.
                    captureOffset += m.Length - displayText.Length;
                }
            }
        }

        private static void handleAdvanced(Regex regex, MessageFormatterResult result, int startIndex = 0)
        {
            foreach (Match m in regex.Matches(result.Text, startIndex))
            {
                var index = m.Index;
                var linkText = m.Groups["link"].Value;
                var indexLength = linkText.Length;

                var details = getLinkDetails(linkText);
                var link = new Link(linkText, index, indexLength, details.Action, details.Argument);

                // sometimes an already-processed formatted link can reduce to a simple URL, too
                // (example: [mean example - https://osu.ppy.sh](https://osu.ppy.sh))
                // therefore we need to check if any of the pre-existing links contains the raw one we found
                if (result.Links.All(existingLink => !existingLink.Overlaps(link)))
                    result.Links.Add(link);
            }
        }

        private static LinkDetails getLinkDetails(string url)
        {
            var args = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            args[0] = args[0].TrimEnd(':');

            switch (args[0])
            {
                case "http":
                case "https":
                    // length > 3 since all these links need another argument to work
                    if (args.Length > 3 && (args[1] == "osu.ppy.sh" || args[1] == "new.ppy.sh"))
                    {
                        switch (args[2])
                        {
                            case "b":
                            case "beatmaps":
                                return new LinkDetails(LinkAction.OpenBeatmap, args[3]);

                            case "s":
                            case "beatmapsets":
                            case "d":
                                return new LinkDetails(LinkAction.OpenBeatmapSet, args[3]);

                            case "u":
                            case "users":
                                return new LinkDetails(LinkAction.OpenUserProfile, args[3]);
                        }
                    }

                    return new LinkDetails(LinkAction.External, null);

                case "osu":
                    // every internal link also needs some kind of argument
                    if (args.Length < 3)
                        return new LinkDetails(LinkAction.External, null);

                    LinkAction linkType;

                    switch (args[1])
                    {
                        case "chan":
                            linkType = LinkAction.OpenChannel;
                            break;

                        case "edit":
                            linkType = LinkAction.OpenEditorTimestamp;
                            break;

                        case "b":
                            linkType = LinkAction.OpenBeatmap;
                            break;

                        case "s":
                        case "dl":
                            linkType = LinkAction.OpenBeatmapSet;
                            break;

                        case "spectate":
                            linkType = LinkAction.Spectate;
                            break;

                        case "u":
                            linkType = LinkAction.OpenUserProfile;
                            break;

                        default:
                            linkType = LinkAction.External;
                            break;
                    }

                    return new LinkDetails(linkType, args[2]);

                case "osump":
                    return new LinkDetails(LinkAction.JoinMultiplayerMatch, args[1]);

                default:
                    return new LinkDetails(LinkAction.External, null);
            }
        }

        private static MessageFormatterResult format(string toFormat, int startIndex = 0, int space = 3)
        {
            var result = new MessageFormatterResult(toFormat);

            // handle the [link display] format
            handleMatches(new_link_regex, "{1}", "{2}", result, startIndex, escapeChars: new[] { '[', ']' });

            // handle the standard markdown []() format
            handleMatches(markdown_link_regex, "{1}", "{2}", result, startIndex, escapeChars: new[] { '[', ']' });

            // handle the ()[] link format
            handleMatches(old_link_regex, "{1}", "{2}", result, startIndex, escapeChars: new[] { '(', ')' });

            // handle wiki links
            handleMatches(wiki_regex, "{1}", "https://osu.ppy.sh/wiki/{1}", result, startIndex);

            // handle bare links
            handleAdvanced(advanced_link_regex, result, startIndex);

            // handle editor times
            handleMatches(time_regex, "{0}", "osu://edit/{0}", result, startIndex, LinkAction.OpenEditorTimestamp);

            // handle channels
            handleMatches(channel_regex, "{0}", "osu://chan/{0}", result, startIndex, LinkAction.OpenChannel);

            var empty = "";
            while (space-- > 0)
                empty += "\0";

            handleMatches(emoji_regex, empty, "{0}", result, startIndex);

            return result;
        }

        public static Message FormatMessage(Message inputMessage)
        {
            var result = format(inputMessage.Content);

            inputMessage.DisplayContent = result.Text;

            // Sometimes, regex matches are not in order
            result.Links.Sort();
            inputMessage.Links = result.Links;
            return inputMessage;
        }

        public static MessageFormatterResult FormatText(string text)
        {
            var result = format(text);

            result.Links.Sort();

            return result;
        }

        public class MessageFormatterResult
        {
            public List<Link> Links = new List<Link>();
            public string Text;
            public string OriginalText;

            public MessageFormatterResult(string text)
            {
                OriginalText = Text = text;
            }
        }

        public class LinkDetails
        {
            public LinkAction Action;
            public string Argument;

            public LinkDetails(LinkAction action, string argument)
            {
                Action = action;
                Argument = argument;
            }
        }
    }

    public enum LinkAction
    {
        External,
        OpenBeatmap,
        OpenBeatmapSet,
        OpenChannel,
        OpenEditorTimestamp,
        JoinMultiplayerMatch,
        Spectate,
        OpenUserProfile,
    }

    public class Link : IComparable<Link>
    {
        public string Url;
        public int Index;
        public int Length;
        public LinkAction Action;
        public string Argument;

        public Link(string url, int startIndex, int length, LinkAction action, string argument)
        {
            Url = url;
            Index = startIndex;
            Length = length;
            Action = action;
            Argument = argument;
        }

        public bool Overlaps(Link otherLink) => Index < otherLink.Index + otherLink.Length && otherLink.Index < Index + Length;

        public int CompareTo(Link otherLink) => Index > otherLink.Index ? 1 : -1;
    }
}
