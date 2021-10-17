// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

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
        private void load([CanBeNull] OverlayColourProvider overlayColourProvider, OsuColour colours)
        {
            BackgroundColour = overlayColourProvider?.Highlight1 ?? colours.Blue3;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateCornerRadius();
        }

        private void updateCornerRadius() => Content.CornerRadius = DrawHeight / 2;

        public virtual IEnumerable<string> FilterTerms => new[] { Text.ToString() };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }
    }
}
