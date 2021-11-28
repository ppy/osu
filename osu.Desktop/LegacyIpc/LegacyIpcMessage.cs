// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;

namespace osu.Desktop.LegacyIpc
{
    /// <summary>
    /// An <see cref="IpcMessage"/> that can be used to communicate to and from legacy clients.
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
