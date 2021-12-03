// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using Newtonsoft.Json.Linq;

namespace osu.Desktop.LegacyIpc
{
    /// <summary>
    /// An <see cref="IpcMessage"/> that can be used to communicate to and from legacy clients.
    /// <para>
    /// In order to deserialise types at either end, types must be serialised as their <see cref="System.Type.AssemblyQualifiedName"/>,
    /// however this cannot be done since osu!stable and osu!lazer live in two different assemblies.
    /// <br />
    /// To get around this, this class exists which serialises a payload (<see cref="LegacyIpcMessage.Data"/>) as an <see cref="System.Object"/> type,
    /// which can be deserialised at either end because it is part of the core library (mscorlib / System.Private.CorLib).
    /// The payload contains the data to be sent over the IPC channel.
    /// <br />
    /// At either end, Json.NET deserialises the payload into a <see cref="JObject"/> which is manually converted back into the expected <see cref="LegacyIpcMessage.Data"/> type,
    /// which then further contains another <see cref="JObject"/> representing the data sent over the IPC channel whose type can likewise be lazily matched through
    /// <see cref="LegacyIpcMessage.Data.MessageType"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Synchronise any changes with osu-stable.
    /// </remarks>
    public class LegacyIpcMessage : IpcMessage
    {
        public LegacyIpcMessage()
        {
            // Types/assemblies are not inter-compatible, so always serialise/deserialise into objects.
            base.Type = typeof(object).FullName;
        }

        public new string Type => base.Type; // Hide setter.

        public new object Value
        {
            get => base.Value;
            set => base.Value = new Data
            {
                MessageType = value.GetType().Name,
                MessageData = value
            };
        }

        public class Data
        {
            public string MessageType { get; set; }
            public object MessageData { get; set; }
        }
    }
}
