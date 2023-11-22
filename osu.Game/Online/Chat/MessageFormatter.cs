// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Edit;

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
            @"(?<link>(https?|osu(mp)?):\/\/" +
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

        // #osu
        private static readonly Regex channel_regex = new Regex(@"(#[a-zA-Z]+[a-zA-Z0-9]+)");

        // Unicode emojis
        private static readonly Regex emoji_regex = new Regex(@"(\uD83D[\uDC00-\uDE4F])");

        /// <summary>
        /// The root URL for the website, used for chat link matching.
        /// </summary>
        public static string WebsiteRootUrl
        {
            get => websiteRootUrl;
            set => websiteRootUrl = value
                                    .Trim('/') // trim potential trailing slash/
                                    .Split('/').Last(); // only keep domain name, ignoring protocol.
        }

        private static string websiteRootUrl = "osu.ppy.sh";

        private static void handleMatches(Regex regex, string display, string link, MessageFormatterResult result, int startIndex = 0, LinkAction? linkActionOverride = null, char[]? escapeChars = null)
        {
            int captureOffset = 0;

            foreach (Match m in regex.Matches(result.Text, startIndex))
            {
                int index = m.Index - captureOffset;

                string displayText = string.Format(display,
                    m.Groups[0],
                    m.Groups["text"].Value,
                    m.Groups["url"].Value).Trim();

                string linkText = string.Format(link,
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

                    // since we just changed the line display text, offset any already processed links.
                    result.Links.ForEach(l => l.Index -= l.Index > index ? m.Length - displayText.Length : 0);

                    var details = GetLinkDetails(linkText);
                    result.Links.Add(new Link(linkText, index, displayText.Length, linkActionOverride ?? details.Action, details.Argument));

                    // adjust the offset for processing the current matches group.
                    captureOffset += m.Length - displayText.Length;
                }
            }
        }

        private static void handleAdvanced(Regex regex, MessageFormatterResult result, int startIndex = 0)
        {
            foreach (Match m in regex.Matches(result.Text, startIndex))
            {
                int index = m.Index;
                string linkText = m.Groups["link"].Value;
                int indexLength = linkText.Length;

                var details = GetLinkDetails(linkText);
                var link = new Link(linkText, index, indexLength, details.Action, details.Argument);

                // sometimes an already-processed formatted link can reduce to a simple URL, too
                // (example: [mean example - https://osu.ppy.sh](https://osu.ppy.sh))
                // therefore we need to check if any of the pre-existing links contains the raw one we found
                if (result.Links.All(existingLink => !existingLink.Overlaps(link)))
                    result.Links.Add(link);
            }
        }

        public static LinkDetails GetLinkDetails(string url)
        {
            string[] args = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
            args[0] = args[0].TrimEnd(':');

            switch (args[0])
            {
                case "http":
                case "https":
                    // length > 3 since all these links need another argument to work
                    if (args.Length > 3 && args[1].EndsWith(WebsiteRootUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        string mainArg = args[3];

                        switch (args[2])
                        {
                            // old site only
                            case "b":
                            case "beatmaps":
                            {
                                string trimmed = mainArg.Split('?').First();
                                if (int.TryParse(trimmed, out int id))
                                    return new LinkDetails(LinkAction.OpenBeatmap, id.ToString());

                                break;
                            }

                            case "s":
                            case "beatmapsets":
                            case "d":
                            {
                                if (mainArg == "discussions")
                                    // handle discussion links externally for now
                                    return new LinkDetails(LinkAction.External, url);

                                if (args.Length > 4 && int.TryParse(args[4], out int id))
                                    // https://osu.ppy.sh/beatmapsets/1154158#osu/2768184
                                    return new LinkDetails(LinkAction.OpenBeatmap, id.ToString());

                                // https://osu.ppy.sh/beatmapsets/1154158#whatever
                                string trimmed = mainArg.Split('#').First();
                                if (int.TryParse(trimmed, out id))
                                    return new LinkDetails(LinkAction.OpenBeatmapSet, id.ToString());

                                break;
                            }

                            case "u":
                            case "users":
                                return getUserLink(mainArg);

                            case "wiki":
                                return new LinkDetails(LinkAction.OpenWiki, string.Join('/', args.Skip(3)));

                            case "home":
                                if (mainArg != "changelog")
                                    // handle link other than changelog as external for now
                                    return new LinkDetails(LinkAction.External, url);

                                switch (args.Length)
                                {
                                    case 4:
                                        // https://osu.ppy.sh/home/changelog
                                        return new LinkDetails(LinkAction.OpenChangelog, string.Empty);

                                    case 6:
                                        // https://osu.ppy.sh/home/changelog/lazer/2021.1006
                                        return new LinkDetails(LinkAction.OpenChangelog, $"{args[4]}/{args[5]}");
                                }

                                break;
                        }
                    }

                    break;

                case "osu":
                    // every internal link also needs some kind of argument
                    if (args.Length < 3)
                        break;

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
                            return getUserLink(args[2]);

                        default:
                            return new LinkDetails(LinkAction.External, url);
                    }

                    return new LinkDetails(linkType, args[2]);

                case "osump":
                    return new LinkDetails(LinkAction.JoinMultiplayerMatch, args[1]);
            }

            return new LinkDetails(LinkAction.External, url);
        }

        private static LinkDetails getUserLink(string argument)
        {
            if (int.TryParse(argument, out int userId))
                return new LinkDetails(LinkAction.OpenUserProfile, new APIUser { Id = userId });

            return new LinkDetails(LinkAction.OpenUserProfile, new APIUser { Username = argument });
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
            handleMatches(wiki_regex, "{1}", $"https://{WebsiteRootUrl}/wiki/{{1}}", result, startIndex);

            // handle bare links
            handleAdvanced(advanced_link_regex, result, startIndex);

            // handle editor times
            handleMatches(EditorTimestampParser.TIME_REGEX, "{0}", $@"{OsuGameBase.OSU_PROTOCOL}edit/{{0}}", result, startIndex, LinkAction.OpenEditorTimestamp);

            // handle channels
            handleMatches(channel_regex, "{0}", $@"{OsuGameBase.OSU_PROTOCOL}chan/{{0}}", result, startIndex, LinkAction.OpenChannel);

            // see: https://github.com/ppy/osu/pull/24190
            result.Text = Regex.Replace(result.Text, emoji_regex.ToString(), "[emoji]");

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
    }

    public class LinkDetails
    {
        public readonly LinkAction Action;

        public readonly object Argument;

        public LinkDetails(LinkAction action, object argument)
        {
            Action = action;
            Argument = argument;
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
        SearchBeatmapSet,
        OpenWiki,
        Custom,
        OpenChangelog,
        FilterBeatmapSetGenre,
        FilterBeatmapSetLanguage,
    }

    public class Link : IComparable<Link>
    {
        public string Url;
        public int Index;
        public int Length;
        public LinkAction Action;
        public object Argument;

        public Link(string url, int startIndex, int length, LinkAction action, object argument)
        {
            Url = url;
            Index = startIndex;
            Length = length;
            Action = action;
            Argument = argument;
        }

        public bool Overlaps(Link otherLink) => Index < otherLink.Index + otherLink.Length && otherLink.Index < Index + Length;

        public int CompareTo(Link? otherLink) => Index > otherLink?.Index ? 1 : -1;
    }
}
