// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using Newtonsoft.Json;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class Message : IComparable<Message>, IEquatable<Message>
    {
        [JsonProperty(@"message_id")]
        public readonly long Id;

        //todo: this should be inside sender.
        [JsonProperty(@"sender_id")]
        public int UserId;

        [JsonProperty(@"target_type")]
        public TargetType TargetType;

        [JsonProperty(@"target_id")]
        public int TargetId;

        [JsonProperty(@"timestamp")]
        public DateTimeOffset Timestamp;

        [JsonProperty(@"content")]
        public string Content;

        [JsonProperty(@"sender")]
        public User Sender;

        [JsonConstructor]
        public Message()
        {
        }

        public Message(long id)
        {
            Id = id;
        }

        public int CompareTo(Message other) => Id.CompareTo(other.Id);

        public bool Equals(Message other) => Id == other?.Id;

        public override int GetHashCode() => Id.GetHashCode();
    }

    public enum TargetType
    {
        [Description(@"channel")]
        Channel,
        [Description(@"user")]
        User
    }
}
