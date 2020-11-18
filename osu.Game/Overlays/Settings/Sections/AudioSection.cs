// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings.Sections.Audio;

namespace osu.Game.Overlays.Settings.Sections
{
    public class AudioSection : SettingsSection
    {
        public override string Header => "Audio";

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.VolumeUp
        };

        public override IEnumerable<string> FilterTerms => base.FilterTerms.Concat(new[] { "sound" });

        public AudioSection()
        {
            List<Drawable> children = new List<Drawable>();
            children.Add(new AudioDevicesSettings());
            children.Add(new VolumeSettings());
            children.Add(new OffsetSettings());
            if (!(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
            {
                // Adjusting latency is neither required nor supported on
                // Windows and Mac.
                children.Add(new LatencySettings());
            }
            children.Add(new MainMenuSettings());
            Children = children;
        }
    }
}
