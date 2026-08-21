// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Testing.Input;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneBeatmapListingSearchControl : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private BeatmapListingSearchControl control;
        private ManualTextInputSource textInput;

        private OsuConfigManager localConfig;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(localConfig = new OsuConfigManager(LocalStorage));
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            OsuSpriteText query;
            OsuSpriteText general;
            OsuSpriteText ruleset;
            OsuSpriteText category;
            OsuSpriteText genre;
            OsuSpriteText language;
            OsuSpriteText extra;
            OsuSpriteText ranks;
            OsuSpriteText played;
            OsuSpriteText explicitMap;

            ManualTextInputContainer textInputContainer;

            Children = new Drawable[]
            {
                textInputContainer = new ManualTextInputContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = control = new BeatmapListingSearchControl
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        query = new OsuSpriteText(),
                        general = new OsuSpriteText(),
                        ruleset = new OsuSpriteText(),
                        category = new OsuSpriteText(),
                        genre = new OsuSpriteText(),
                        language = new OsuSpriteText(),
                        extra = new OsuSpriteText(),
                        ranks = new OsuSpriteText(),
                        played = new OsuSpriteText(),
                        explicitMap = new OsuSpriteText(),
                    }
                }
            };

            control.Query.BindValueChanged(q => query.Text = $"Query: {q.NewValue}", true);
            control.General.BindCollectionChanged((_, _) => general.Text = $"General: {(control.General.Any() ? string.Join('.', control.General.Select(i => i.ToString().ToSnakeCase())) : "")}", true);
            control.Ruleset.BindValueChanged(r => ruleset.Text = $"Ruleset: {r.NewValue}", true);
            control.Category.BindValueChanged(c => category.Text = $"Category: {c.NewValue}", true);
            control.Genre.BindValueChanged(g => genre.Text = $"Genre: {g.NewValue}", true);
            control.Language.BindValueChanged(l => language.Text = $"Language: {l.NewValue}", true);
            control.Extra.BindCollectionChanged((_, _) => extra.Text = $"Extra: {(control.Extra.Any() ? string.Join('.', control.Extra.Select(i => i.ToString().ToLowerInvariant())) : "")}", true);
            control.Ranks.BindCollectionChanged((_, _) => ranks.Text = $"Ranks: {(control.Ranks.Any() ? string.Join('.', control.Ranks.Select(i => i.ToString())) : "")}", true);
            control.Played.BindValueChanged(p => played.Text = $"Played: {p.NewValue}", true);
            control.ExplicitContent.BindValueChanged(e => explicitMap.Text = $"Explicit Maps: {e.NewValue}", true);
            textInput = textInputContainer.TextInput;
        });

        [Test]
        public void TestCovers()
        {
            AddStep("Set beatmap", () => control.BeatmapSet = beatmap_set);
            AddStep("Set beatmap (no cover)", () => control.BeatmapSet = no_cover_beatmap_set);
            AddStep("Set null beatmap", () => control.BeatmapSet = null);
        }

        [Test]
        public void TestExplicitConfig()
        {
            AddStep("configure explicit content to allowed", () => localConfig.SetValue(OsuSetting.ShowOnlineExplicitContent, true));
            AddAssert("explicit control set to show", () => control.ExplicitContent.Value == SearchExplicit.Show);

            AddStep("configure explicit content to disallowed", () => localConfig.SetValue(OsuSetting.ShowOnlineExplicitContent, false));
            AddAssert("explicit control set to hide", () => control.ExplicitContent.Value == SearchExplicit.Hide);
        }

        [Test]
        public void TestTypingStartedFiredOnTextMutatingInteractions()
        {
            var cases = new (string Name, Action Perform)[]
            {
                ("type a character", () => textInput.Text("a")),
                ("backspace (DeleteBackwardChar)", () => InputManager.Key(Key.BackSpace)),
                ("Ctrl+Backspace (DeleteBackwardWord)", () =>
                {
                    InputManager.PressKey(Key.LControl);
                    InputManager.Key(Key.BackSpace);
                    InputManager.ReleaseKey(Key.LControl);
                }),
                ("Shift+Left (SelectBackwardChar)", () =>
                {
                    InputManager.PressKey(Key.LShift);
                    InputManager.Key(Key.Left);
                    InputManager.ReleaseKey(Key.LShift);
                }),
                ("Ctrl+C (Copy)", () =>
                {
                    InputManager.PressKey(Key.LControl);
                    InputManager.Key(Key.C);
                    InputManager.ReleaseKey(Key.LControl);
                }),
                ("Ctrl+X (Cut)", () =>
                {
                    InputManager.PressKey(Key.LControl);
                    InputManager.Key(Key.X);
                    InputManager.ReleaseKey(Key.LControl);
                }),
                ("Ctrl+V (Paste)", () =>
                {
                    InputManager.PressKey(Key.LControl);
                    InputManager.Key(Key.V);
                    InputManager.ReleaseKey(Key.LControl);
                }),
                ("Ctrl+A (SelectAll)", () =>
                {
                    InputManager.PressKey(Key.LControl);
                    InputManager.Key(Key.A);
                    InputManager.ReleaseKey(Key.LControl);
                }),
                ("escape with text (GlobalAction.Back)", () => InputManager.Key(Key.Escape)),
            };

            bool typingStarted = false;

            foreach (var (name, perform) in cases)
            {
                AddStep($"setup for: {name}", () =>
                {
                    typingStarted = false;
                    control.Query.Value = "test";
                    control.TypingStarted = () => typingStarted = true;
                    control.TakeFocus();
                });
                AddStep(name, perform);
                AddAssert("typing started was called", () => typingStarted);
            }
        }

        [Test]
        public void TestTypingStartedNotFiredOnNonMutatingInteractions()
        {
            var cases = new (string Name, Action Perform)[]
            {
                ("F5 (no PlatformAction)", () => InputManager.Key(Key.F5)),
                ("F12 (no PlatformAction)", () => InputManager.Key(Key.F12)),
                ("Delete (blocked by SearchTextBox)", () => InputManager.Key(Key.Delete)),
                ("Ctrl+S (PlatformAction.Save)", () =>
                {
                    InputManager.PressKey(Key.LControl);
                    InputManager.Key(Key.S);
                    InputManager.ReleaseKey(Key.LControl);
                }),
                ("Ctrl+= (PlatformAction.ZoomIn)", () =>
                {
                    InputManager.PressKey(Key.LControl);
                    InputManager.Key(Key.Plus);
                    InputManager.ReleaseKey(Key.LControl);
                }),
            };

            bool typingStarted = false;

            foreach (var (name, perform) in cases)
            {
                AddStep($"setup for: {name}", () =>
                {
                    typingStarted = false;
                    control.TypingStarted = () => typingStarted = true;
                    control.TakeFocus();
                });
                AddStep(name, perform);
                AddAssert("typing started was not called", () => !typingStarted);
            }
        }

        [Test]
        public void TestEscapeClearsSearchBox()
        {
            AddStep("populate search box", () => control.Query.Value = "test");
            AddStep("focus search box", () => control.TakeFocus());
            AddStep("press escape", () => InputManager.Key(Key.Escape));
            AddAssert("search box is empty", () => string.IsNullOrEmpty(control.Query.Value));
        }

        protected override void Dispose(bool isDisposing)
        {
            localConfig?.Dispose();
            base.Dispose(isDisposing);
        }

        private static readonly APIBeatmapSet beatmap_set = new APIBeatmapSet
        {
            Covers = new BeatmapSetOnlineCovers
            {
                Cover = "https://assets.ppy.sh/beatmaps/1094296/covers/cover@2x.jpg?1581416305"
            }
        };

        private static readonly APIBeatmapSet no_cover_beatmap_set = new APIBeatmapSet
        {
            Covers = new BeatmapSetOnlineCovers
            {
                Cover = string.Empty
            }
        };

        private partial class ManualTextInputContainer : Container
        {
            [Cached(typeof(TextInputSource))]
            public readonly ManualTextInputSource TextInput = new ManualTextInputSource();
        }
    }
}
