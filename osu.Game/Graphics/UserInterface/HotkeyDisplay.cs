// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Game.Graphics.Sprites;
using osu.Game.Input;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class HotkeyDisplay : CompositeDrawable
    {
        private Hotkey hotkey;

        public Hotkey Hotkey
        {
            get => hotkey;
            set
            {
                if (EqualityComparer<Hotkey>.Default.Equals(hotkey, value))
                    return;

                hotkey = value;

                if (IsLoaded)
                    updateState();
            }
        }

        private FillFlowContainer flow = null!;

        [Resolved]
        private ReadableKeyCombinationProvider readableKeyCombinationProvider { get; set; } = null!;

        [Resolved]
        private RealmKeyBindingStore realmKeyBindingStore { get; set; } = null!;

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = flow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5)
            };

            updateState();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
        }

        private void updateState()
        {
            flow.Clear();
            foreach (string h in hotkey.ResolveKeyCombination(readableKeyCombinationProvider, realmKeyBindingStore, gameHost))
                flow.Add(new HotkeyBox(h));
        }

        private partial class HotkeyBox : CompositeDrawable
        {
            private readonly string hotkey;

            public HotkeyBox(string hotkey)
            {
                this.hotkey = hotkey;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider? colourProvider, OsuColour colours)
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 3;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider?.Background6 ?? Colour4.Black.Opacity(0.7f),
                    },
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding { Horizontal = 5, Bottom = 1, },
                        Text = hotkey.ToUpperInvariant(),
                        Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold),
                        Colour = colourProvider?.Light1 ?? colours.GrayA,
                    }
                };
            }
        }
    }
}
