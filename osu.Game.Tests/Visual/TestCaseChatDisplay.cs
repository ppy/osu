// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Users;
using osu.Game.Online.Chat;
using osu.Game.Configuration;
using osu.Framework.Graphics.UserInterface;
using System;

namespace osu.Game.Tests.Visual
{
    [Description("Testing chat api and overlay")]
    public class TestCaseChatDisplay : OsuTestCase
    {

        DummyChatOverlay chat;

        public TestCaseChatDisplay()
        {
             chat = new DummyChatOverlay
             {
                 State = Visibility.Visible
             };
            Add(chat);
            
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, OsuConfigManager config, OsuColour colours)
        {
            var channel = new DummyChannel()
            {
                Name = "#Dummy",
                Topic = "Test Chat",
                Type = "Test",
                Id = 0
            };

            api.Scheduler.Add(delegate { 
            channel.AddNewMessage("This message for test from offline.");
            channel.AddNewMessage("TestMessage");
            channel.AddNewMessage("!@#$%^&&*()");
            channel.AddNewMessage("testTEST");
            chat.OpenChannel(channel);
            });

        }

        private class DummyChatOverlay : ChatOverlay
        {

            [BackgroundDependencyLoader]
            private void load(APIAccess api, OsuConfigManager config, OsuColour colours)
            {
                textbox.OnCommit = postDummyMessage;
                base.api = api;
                api.Register(this);

            }

            private void postDummyMessage(TextBox textbox, bool newText)
            {
                var postText = textbox.Text;

                textbox.Text = string.Empty;

                if (string.IsNullOrWhiteSpace(postText))
                    return;

                var target = base.CurrentChannel;

                if (target == null) return;

                bool isAction = false;

                if (postText[0] == '/')
                {
                    string[] parameters = postText.Substring(1).Split(new[] { ' ' }, 2);
                    string command = parameters[0];
                    string content = parameters.Length == 2 ? parameters[1] : string.Empty;

                    switch (command)
                    {
                        case "me":

                            if (string.IsNullOrWhiteSpace(content))
                            {
                                CurrentChannel.AddNewMessages(new ErrorMessage("Usage: /me [action]"));
                                return;
                            }

                            isAction = true;
                            postText = content;
                            break;

                        case "help":
                            CurrentChannel.AddNewMessages(new InfoMessage("Supported commands: /help, /me [action]"));
                            return;

                        default:
                            CurrentChannel.AddNewMessages(new ErrorMessage($@"""/{command}"" is not supported! For a list of supported commands see /help"));
                            return;
                    }
                }

                var message = new DummyMessage(postText, "DummyUser", isAction, false, 0);
                api.Scheduler.Add(delegate { CurrentChannel.AddNewMessages(message); });
            }
        }

        private class DummyChannel : Channel
        {

            private int messageCounter;

            //Should be call "API Thread"
            public void AddNewMessage(string message)
            {
                base.AddNewMessages(new DummyMessage(message, null, false, false, messageCounter++));
            }
        }

        private class DummyMessage : Message
        {
            private static long messageCounter;
            public DummyMessage(string text, string username = null, bool isAction = false, bool isImportant = false, int number = 0)
                : base(messageCounter++)
            {
                Content = text;
                IsAction = isAction;
                Timestamp = DateTimeOffset.Now;
                Sender = new User 
                {
                    Username = (username != null)? username : $"User " + number + "",
                    Id = number,
                    Colour = isImportant ? "#250cc9" : null,
                };
            }
        }

    }
}
