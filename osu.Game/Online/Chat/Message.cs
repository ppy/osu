// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.Chat
{
    public class Message : IComparable<Message>, IEquatable<Message>
    {
        [JsonProperty(@"message_id")]
        public readonly long? Id;

        [JsonProperty(@"channel_id")]
        public long ChannelId;

        [JsonProperty(@"is_action")]
        public bool IsAction;

        [JsonProperty(@"timestamp")]
        public DateTimeOffset Timestamp;

        [JsonProperty(@"content")]
        public string Content;

        [JsonProperty(@"sender")]
        public APIUser Sender;

        [JsonProperty(@"sender_id")]
        public int SenderId
        {
            get => Sender?.Id ?? 0;
            set => Sender = new APIUser { Id = value };
        }

        /// <summary>
        /// A unique identifier for this message. Sent to and from osu!web to use for deduplication.
        /// </summary>
        [JsonProperty(@"uuid")]
        public string Uuid { get; set; } = string.Empty;

        [JsonConstructor]
        public Message()
        {
        }

        /// <summary>
        /// The text that is displayed in chat.
        /// </summary>
        public string DisplayContent { get; set; }

        /// <summary>
        /// The links found in this message.
        /// </summary>
        /// <remarks>The <see cref="Link"/>s' <see cref="Link.Index"/> and <see cref="Link.Length"/>s are according to <see cref="DisplayContent"/></remarks>
        public List<Link> Links;

        private static long constructionOrderStatic;
        private readonly long constructionOrder;

        public Message(long? id)
        {
            Id = id;

            constructionOrder = Interlocked.Increment(ref constructionOrderStatic);
        }

        public int CompareTo(Message other)
        {
            if (Id.HasValue && other.Id.HasValue)
                return Id.Value.CompareTo(other.Id.Value);

            int timestampComparison = Timestamp.CompareTo(other.Timestamp);

            if (timestampComparison != 0)
                return timestampComparison;

            // Timestamp might not be accurate enough to make a stable sorting decision.
            return constructionOrder.CompareTo(other.constructionOrder);
        }

        public virtual bool Equals(Message other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id.HasValue && Id == other.Id;
        }

        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => $"({(Id?.ToString() ?? "null")}) {Timestamp} {Sender}: {Content}";
    }
}
