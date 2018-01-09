// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Colour;
using osu.Game.Online.Chat;
using System;

namespace osu.Game.Graphics.Containers
{
    public class ChatFlowContainer : OsuTextFlowContainer
    {
        private readonly Action<ChatLink> defaultCreationParameters;
        private ColourInfo urlColour;

        public ChatFlowContainer(Action<ChatLink> defaultCreationParameters = null)
        {
            this.defaultCreationParameters = defaultCreationParameters;
        }

        public override bool HandleInput => true;

        public void AddLink(string text, string url, LinkAction linkType, string linkArgument)
        {
            var chatSprite = new ChatLink
            {
                Text = text,
                Url = url,
                TextColour = urlColour,
                LinkAction = linkType,
                LinkArgument = linkArgument,
            };

            defaultCreationParameters?.Invoke(chatSprite);

            AddInternal(chatSprite);
        }

        public void AddText(string text, Action<ChatLink> creationParameters = null)
        {
            foreach (var word in SplitWords(text))
            {
                if (string.IsNullOrEmpty(word))
                    continue;

                var chatSprite = new ChatLink { Text = word };

                defaultCreationParameters?.Invoke(chatSprite);
                creationParameters?.Invoke(chatSprite);

                AddInternal(chatSprite);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            urlColour = colours.Blue;
        }
    }
}
