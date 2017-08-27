// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace osu.Game.Online.Chat
{
    public static class LinkFormatter
    {
        //[[Performance Points]] -> wiki:Performance Points (https://osu.ppy.sh/wiki/Performance_Points)
        public static Regex RegexWiki = new Regex(@"\[\[([^\]]+)\]\]");

        //(test)[https://osu.ppy.sh/b/1234] -> test (https://osu.ppy.sh/b/1234)
        public static Regex OldFormatLink = new Regex(@"\(([^\)]*)\)\[([a-z]+://[^ ]+)\]");

        //[https://osu.ppy.sh/b/1234 Beatmap [Hard] (poop)] -> Beatmap [hard] (poop) (https://osu.ppy.sh/b/1234)
        public static Regex NewFormatLink = new Regex(@"\[([a-z]+://[^ ]+) ([^\[\]]*(((?<open>\[)[^\[\]]*)+((?<close-open>\])[^\[\]]*)+)*(?(open)(?!)))\]");

        //https://osu.ppy.sh -> https://osu.ppy.sh (https://osu.ppy.sh)
        //static Regex basicLink = new Regex(@"[a-z]+://[^ ]+[a-zA-Z0-9=/\?]");

        // advanced, RFC-compatible version of basicLink that matches any possible URL, *but* allows certain invalid characters that are widely used
        // This is in the format (<required>, [optional]):
        //      http[s]://<domain>.<tld>[:port][/path][?query][#fragment]
        public static Regex AdvancedLink = new Regex(@"(?<paren>\([^)]*)?" +
                @"(?<link>https?:\/\/" +
                    @"(?<domain>(?:[a-z0-9]\.|[a-z0-9][a-z0-9-]*[a-z0-9]\.)*[a-z][a-z0-9-]*[a-z0-9]" + // domain, TLD
                    @"(?::\d+)?)" + // port
                        @"(?<path>(?:(?:\/+(?:[a-z0-9$_\.\+!\*\',;:\(\)@&~=-]|%[0-9a-f]{2})*)*" + // path
                        @"(?:\?(?:[a-z0-9$_\+!\*\',;:\(\)@&=\/~-]|%[0-9a-f]{2})*)?)?" + // query
                        @"(?:#(?:[a-z0-9$_\+!\*\',;:\(\)@&=\/~-]|%[0-9a-f]{2})*)?)?)", // fragment
                RegexOptions.IgnoreCase);

        //00:00:000 (1,2,3) - test
        public static Regex TimeMatch = new Regex(@"\d\d:\d\d:\d\d\d? [^-]*");

        //#osu
        public static Regex ChannelMatch = new Regex(@"#[a-zA-Z]+[a-zA-Z0-9]+");

        // \:01
        //static Regex emoji = new Regex(@"\\\:\d\d");
        public static Regex Emoji = new Regex(@"(\uD83D[\uDC00-\uDE4F])");

        private static void handleAdvanced(Regex against, LinkFormatterResult result, int startIndex = 0)
        {
            foreach (Match m in against.Matches(result.Text, startIndex))
            {
                int index = m.Index;
                string prefix = m.Groups["paren"].Value;
                string link = m.Groups["link"].Value;
                int indexLength = link.Length;

                if (!String.IsNullOrEmpty(prefix))
                {
                    index += prefix.Length;
                    if (link.EndsWith(")"))
                    {
                        indexLength = indexLength - 1;
                        link = link.Remove(link.Length - 1);
                    }
                }

                result.Links.Add(new Link(link, index, indexLength));

            }
        }

        private static void handleMatches(Regex against, string display, string link, LinkFormatterResult result, int startIndex = 0)
        {
            int captureOffset = 0;
            foreach (Match m in against.Matches(result.Text, startIndex))
            {
                int index = m.Index - captureOffset;

                string displayText = string.Format(display,
                                                m.Groups[0],
                                                m.Groups.Count > 1 ? m.Groups[1].Value : "",
                                                m.Groups.Count > 2 ? m.Groups[2].Value : "").Trim();

                string linkText = string.Format(link,
                                                m.Groups[0],
                                                m.Groups.Count > 1 ? m.Groups[1].Value : "",
                                                m.Groups.Count > 2 ? m.Groups[2].Value : "").Trim();

                if (displayText.Length == 0 || linkText.Length == 0) continue;

                //ensure we don't have encapsulated links.
                if (result.Links.Find(l => l.Index <= index && l.Index + l.Length >= index + m.Length || index <= l.Index && index + m.Length >= l.Index + l.Length) == null)
                {
                    result.Text = result.Text.Remove(index, m.Length).Insert(index, displayText);

                    //since we just changed the line display text, offset any already processed links.
                    result.Links.ForEach(l => l.Index -= l.Index > index ? m.Length - displayText.Length : 0);

                    result.Links.Add(new Link(linkText, index, displayText));

                    //adjust the offset for processing the current matches group.
                    captureOffset += m.Length - displayText.Length;
                }
            }
        }

        public static LinkFormatterResult Format(string input, int startIndex = 0, int space = 3)
        {
            LinkFormatterResult result = new LinkFormatterResult(input);

            // handle the [link display] format
            handleMatches(NewFormatLink, "{2}", "{1}", result, startIndex);

            // handle the ()[] link format
            handleMatches(OldFormatLink, "{1}", "{2}", result, startIndex);

            // handle wiki links
            handleMatches(RegexWiki, "wiki:{1}", "https://osu.ppy.sh/wiki/{1}", result, startIndex);

            // handle bare links
            handleAdvanced(AdvancedLink, result, startIndex);

            // handle editor times
            handleMatches(TimeMatch, "{0}", "osu://edit/{0}", result, startIndex);

            // handle channels
            handleMatches(ChannelMatch, "{0}", "osu://chan/{0}", result, startIndex);

            string empty = "";
            while (space-- > 0)
                empty += "\0";

            handleMatches(Emoji, empty, "{0}", result, startIndex); // 3 space,handleMatches will trim all empty char except \0
            //result.Text = result.Text.Replace('\0', ' ');
            return result;
        }
    }

    public class Link
    {
        public string Url;
        public int Index;
        public int Length;
        public string DisplayText;

        public Link(string url, int startIndex, int length)
        {
            Url = url;
            Index = startIndex;
            DisplayText = url;
            Length = length;
        }

        public Link(string url, int startIndex, string displayText)
        {
            Url = url;
            Index = startIndex;
            DisplayText = displayText;
            Length = displayText.Length;
        }
    }

    public class LinkFormatterResult : ICloneable
    {
        public List<Link> Links = new List<Link>();
        public string Text;
        public string OriginalText;

        public LinkFormatterResult(string text)
        {
            OriginalText = Text = text;
        }

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
