// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    [Description("Testing chat api and overlay")]
    public class TestCaseChatDisplay : OsuTestCase
    {
        private DummyChatOverlay chat;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            DummyAPIAccess api;

            dependencies.CacheAs<IAPIProvider>(api = new DummyAPIAccess());

            Add(chat = new DummyChatOverlay
            {
                State = Visibility.Visible
            });

            AddStep("Set Username DummyUser", () => api.LocalUser.Value.Username = "DummyUser");
            AddStep("Type \"Hello\"", () => chat.PostMessage("Hello"));
            AddStep("Set Long Username", () => api.LocalUser.Value.Username = "Over15LengthUserName");
            AddStep("Type \"Over15LengthUserName\"", () => chat.PostMessage("Over15LengthUserName"));
            AddStep("Set Wide Username", () => api.LocalUser.Value.Username = "WWWWWWWWWWWWWWW");
            AddStep("Type \"Wide!\"", () => chat.PostMessage("Wide!"));

            var channel = new Channel
            {
                Name = "#Dummy",
                Topic = "Test Chat",
                Type = "Test",
                Id = 0
            };

            channel.AddNewMessages(
                new DummyMessage("This message for test from offline."),
                new DummyMessage("TestMessage"),
                new DummyMessage("TestMessage"),
                new DummyMessage("TestMessage")
            );

            chat.OpenChannel(channel);
        }

        private class DummyChatOverlay : ChatOverlay
        {
            public new void PostMessage(string postText) => base.PostMessage(postText);
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
                    Username = username ?? $"User {number}",
                    Id = number,
                    Colour = isImportant ? "#250cc9" : null,
                };
            }
        }
    }
}
