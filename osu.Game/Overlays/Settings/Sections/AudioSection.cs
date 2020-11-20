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

        private IEnumerable<Drawable> getSections()
        {
            yield return new AudioDevicesSettings();
            yield return new VolumeSettings();
            yield return new OffsetSettings();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                // Adjusting latency is neither required nor supported on
                // other platforms.
                yield return new LatencySettings();
            }
            yield return new MainMenuSettings();
        }

        public AudioSection()
        {
            ChildrenEnumerable = getSections();
        }
    }
}
