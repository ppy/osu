// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
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
                    resolveCombinations();
            }
        }

        private FillFlowContainer<HotkeyBox> flow = null!;

        [Resolved]
        private ReadableKeyCombinationProvider readableKeyCombinationProvider { get; set; } = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        private IDisposable? realmSubscription;
        private readonly BindableList<string> resolvedCombinations = new BindableList<string>();

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = flow = new FillFlowContainer<HotkeyBox>
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            resolveCombinations();
            resolvedCombinations.BindCollectionChanged((_, _) => updateState(), true);
        }

        private void updateState()
        {
            while (flow.Count > resolvedCombinations.Count)
                flow.Remove(flow[^1], true);

            while (flow.Count < resolvedCombinations.Count)
                flow.Add(new HotkeyBox());

            Debug.Assert(flow.Count == resolvedCombinations.Count);

            for (int i = 0; i < resolvedCombinations.Count; ++i)
                flow[i].Text = resolvedCombinations[i];
        }

        private void resolveCombinations()
        {
            if (IsDisposed)
                return;

            realmSubscription?.Dispose();
            realmSubscription = null;

            resolvedCombinations.Clear();

            if (Hotkey.KeyCombinations != null)
            {
                resolvedCombinations.AddRange(Hotkey.KeyCombinations.Select(readableKeyCombinationProvider.GetReadableString));
                return;
            }

            if (Hotkey.GlobalAction is GlobalAction globalAction)
            {
                Debug.Assert(realmSubscription == null);

                realmSubscription = realmAccess.RegisterForNotifications(
                    r => r.All<RealmKeyBinding>().Where(kb => string.IsNullOrEmpty(kb.RulesetName) && kb.ActionInt == (int)globalAction),
                    (bindings, _) =>
                    {
                        resolvedCombinations.Clear();
                        resolvedCombinations.AddRange(bindings.Select(kb => readableKeyCombinationProvider.GetReadableString(kb.KeyCombination)));
                    });
                return;
            }

            if (Hotkey.RulesetAction is (string ruleset, var variant, var rulesetAction))
            {
                Debug.Assert(realmSubscription == null);

                realmSubscription = realmAccess.RegisterForNotifications(
                    r => r.All<RealmKeyBinding>().Where(kb => kb.RulesetName == ruleset && kb.Variant == variant && kb.ActionInt == rulesetAction),
                    (bindings, _) =>
                    {
                        resolvedCombinations.Clear();
                        resolvedCombinations.AddRange(bindings.Select(kb => readableKeyCombinationProvider.GetReadableString(kb.KeyCombination)));
                    });
                return;
            }

            if (hotkey.PlatformAction != null)
            {
                var action = hotkey.PlatformAction.Value;
                var bindings = gameHost.PlatformKeyBindings.Where(kb => (PlatformAction)kb.Action == action);
                resolvedCombinations.AddRange(bindings.Select(b => readableKeyCombinationProvider.GetReadableString(b.KeyCombination)));
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            realmSubscription?.Dispose();
            realmSubscription = null;
        }

        private partial class HotkeyBox : CompositeDrawable, IHasText
        {
            private LocalisableString text;
            private OsuSpriteText textSprite = null!;

            public LocalisableString Text
            {
                get => text;
                set
                {
                    text = value;
                    if (IsLoaded)
                        textSprite.Text = text.ToUpper();
                }
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
                        Colour = colourProvider?.Background4 ?? Colour4.Black.Opacity(0.7f),
                    },
                    textSprite = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Horizontal = 5, Bottom = 1, },
                        Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold),
                        Colour = colourProvider?.Light2 ?? colours.GrayA,
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                textSprite.Text = text.ToUpper();
            }
        }
    }
}
