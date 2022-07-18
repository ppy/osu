// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class RoundedButton : OsuButton, IFilterable
    {
        public override float Height
        {
            get => base.Height;
            set
            {
                base.Height = value;

                if (IsLoaded)
                    updateCornerRadius();
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours)
        {
            // According to flyte, buttons are supposed to have explicit colours for now.
            // Not sure this is the correct direction, but we haven't decided on an `OverlayColourProvider` stand-in yet.
            // This is a better default. See `SettingsButton` for an override which uses `OverlayColourProvider`.
            DefaultBackgroundColour = colours.Blue3;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateCornerRadius();
        }

        private void updateCornerRadius() => Content.CornerRadius = DrawHeight / 2;

        public virtual IEnumerable<LocalisableString> FilterTerms => new[] { Text };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }
    }
}
