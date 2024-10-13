// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class ExternalEditOverlay : OsuFocusedOverlayContainer
    {
        private const double transition_duration = 300;
        private FillFlowContainer flow = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        private ExternalEditOperation<SkinInfo>? editOperation;

        private Bindable<Skin>? skinBindable;
        private SkinManager? skinManager;

        protected override bool DimMainContent => false;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    // Since we're drawing this overlay on top of another overlay (SkinEditor), the dimming effect isn't applied. So we need to add a dimming effect manually.
                    new Box
                    {
                        Colour = Color4.Black.Opacity(0.5f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        Masking = true,
                        CornerRadius = 20,
                        AutoSizeAxes = Axes.Both,
                        AutoSizeDuration = 500,
                        AutoSizeEasing = Easing.OutQuint,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colourProvider.Background5,
                                RelativeSizeAxes = Axes.Both,
                            },
                            flow = new FillFlowContainer
                            {
                                Margin = new MarginPadding(20),
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Spacing = new Vector2(15),
                            }
                        }
                    }
                }
            };
        }

        public async Task Begin(SkinInfo skinInfo, Bindable<Skin> skinBindable, SkinManager skinManager)
        {
            Show();
            showSpinner("Mounting external skin...");

            await Task.Delay(500).ConfigureAwait(true);

            try
            {
                editOperation = await skinManager.BeginExternalEditing(skinInfo).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to initialize external edit operation: {ex}", LoggingTarget.Database, LogLevel.Error);
                Schedule(() => showSpinner("Export failed!"));
                await Task.Delay(1000).ConfigureAwait(true);
                Hide();
            }

            this.skinBindable = skinBindable;
            this.skinManager = skinManager;

            Schedule(() =>
            {
                flow.Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = "Skin is mounted externally",
                        Font = OsuFont.Default.With(size: 30),
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                    new OsuTextFlowContainer
                    {
                        Padding = new MarginPadding(5),
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Width = 350,
                        AutoSizeAxes = Axes.Y,
                        Text = "Any changes made to the exported folder will be imported to the game, including file additions, modifications and deletions.",
                    },
                    new PurpleRoundedButton
                    {
                        Text = "Open folder",
                        Width = 350,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Action = openDirectory,
                        Enabled = { Value = false }
                    },
                    new DangerousRoundedButton
                    {
                        Text = EditorStrings.FinishEditingExternally,
                        Width = 350,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Action = () => finish().FireAndForget(),
                        Enabled = { Value = false }
                    }
                };
            });

            Scheduler.AddDelayed(() =>
            {
                foreach (var b in flow.ChildrenOfType<RoundedButton>())
                    b.Enabled.Value = true;
                openDirectory();
            }, 1000);
        }

        private void openDirectory()
        {
            if (editOperation == null)
                return;

            gameHost.OpenFileExternally(editOperation.MountedPath.TrimDirectorySeparator() + Path.DirectorySeparatorChar);
        }

        private async Task finish()
        {
            showSpinner("Cleaning up...");
            await Task.Delay(500).ConfigureAwait(true);

            try
            {
                await editOperation!.Finish().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to finish external edit operation: {ex}", LoggingTarget.Database, LogLevel.Error);
                showSpinner("Import failed!");
                await Task.Delay(1000).ConfigureAwait(true);
                Hide();
            }

            Schedule(() =>
            {
                var oldSkin = skinBindable!.Value;
                var newSkinInfo = oldSkin.SkinInfo.PerformRead(s => s);

                // Create a new skin instance to ensure the skin is reloaded
                // If there's a better way to reload the skin, this should be replaced with it.
                skinBindable.Value = newSkinInfo.CreateInstance(skinManager!);

                oldSkin.Dispose();

                Hide();
            });
        }

        protected override void PopIn()
        {
            this.FadeIn(transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(transition_duration, Easing.OutQuint).Finally(_ =>
            {
                // Set everything to a clean state
                editOperation = null;
                skinManager = null;
                skinBindable = null;
                flow.Children = Array.Empty<Drawable>();
            });
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                case GlobalAction.Select:
                    if (editOperation == null) return base.OnPressed(e);

                    finish().FireAndForget();
                    return true;
            }

            return base.OnPressed(e);
        }

        private void showSpinner(string text)
        {
            foreach (var b in flow.ChildrenOfType<RoundedButton>())
                b.Enabled.Value = false;

            flow.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = text,
                    Font = OsuFont.Default.With(size: 30),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
                new LoadingSpinner
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    State = { Value = Visibility.Visible }
                },
            };
        }
    }
}
