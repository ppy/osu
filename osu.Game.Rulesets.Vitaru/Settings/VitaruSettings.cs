using eden.Game.GamePieces;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Rulesets.Vitaru.Multi;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Vitaru.Objects.Characters;
using System.Collections.Generic;
using System.Linq;
using Symcol.Rulesets.Core;
using Symcol.Rulesets.Core.Wiki;
using osu.Game.Rulesets.Vitaru.Wiki;
using osu.Game.Rulesets.Vitaru.Scoring;
using osu.Game.Rulesets.Vitaru.Edit;
using Symcol.Rulesets.Core.Multiplayer.Screens;

namespace osu.Game.Rulesets.Vitaru.Settings
{
    public class VitaruSettings : SymcolSettingsSubsection
    {
        protected override string Header => "vitaru!";

        public override WikiOverlay Wiki => vitaruWiki;

        public override RulesetLobbyItem RulesetLobbyItem => vitaruLobby;

        private readonly VitaruWikiOverlay vitaruWiki = new VitaruWikiOverlay();

        private readonly VitaruLobbyItem vitaruLobby = new VitaruLobbyItem();

        public static VitaruConfigManager VitaruConfigManager;

        private static VitaruAPIContainer api;

        private Bindable<Characters> selectedCharacter;

        private FillFlowContainer multiplayerSettings;
        private Bindable<bool> multiplayer;
        private Bindable<int> friendlyPlayerCount;
        private Bindable<bool> friendlyPlayerOverride;
        private FillFlowContainer friendlyPlayerSettings;
        private Bindable<int> enemyPlayerCount;
        private Bindable<bool> enemyPlayerOverride;
        private FillFlowContainer enemyPlayerSettings;

        private FillFlowContainer debugUiSettings;
        private Bindable<bool> showDebugUi;

        private SettingsDropdown<string> skin;
        private Bindable<string> currentSkin;

        private const int transition_duration = 400;

        [BackgroundDependencyLoader]
        private void load(GameHost host, Storage storage)
        {
            if (api == null)
                Add(api = new VitaruAPIContainer());

            VitaruConfigManager = new VitaruConfigManager(host.Storage);

            Storage skinsStorage = storage.GetStorageForDirectory("Skins");

            showDebugUi = VitaruConfigManager.GetBindable<bool>(VitaruSetting.DebugOverlay);
            selectedCharacter = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.Characters);
            multiplayer = VitaruConfigManager.GetBindable<bool>(VitaruSetting.ShittyMultiplayer);
            friendlyPlayerCount = VitaruConfigManager.GetBindable<int>(VitaruSetting.FriendlyPlayerCount);
            friendlyPlayerOverride = VitaruConfigManager.GetBindable<bool>(VitaruSetting.FriendlyPlayerOverride);
            enemyPlayerCount = VitaruConfigManager.GetBindable<int>(VitaruSetting.EnemyPlayerCount);
            enemyPlayerOverride = VitaruConfigManager.GetBindable<bool>(VitaruSetting.EnemyPlayerOverride);

            Children = new Drawable[]
            {
                new SettingsEnumDropdown<VitaruGamemode>
                {
                    LabelText = "Vitaru's current gamemode",
                    Bindable = VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode)
                },
                new SettingsEnumDropdown<Characters>
                {
                    LabelText = "Selected Character",
                    Bindable = selectedCharacter
                },
                new SettingsEnumDropdown<GraphicsPresets>
                {
                    LabelText = "Graphics Presets",
                    Bindable = VitaruConfigManager.GetBindable<GraphicsPresets>(VitaruSetting.GraphicsPresets)
                },
                new SettingsEnumDropdown<EditorConfiguration>
                {
                    LabelText = "Current Editor Configuration",
                    Bindable = VitaruConfigManager.GetBindable<EditorConfiguration>(VitaruSetting.EditorConfiguration)
                },
                new SettingsEnumDropdown<ScoringMetric>
                {
                    LabelText = "Current Scoring Metric used (Difficulty, Score and PP)",
                    Bindable = VitaruConfigManager.GetBindable<ScoringMetric>(VitaruSetting.ScoringMetric)
                },
                new SettingsCheckbox
                {
                    LabelText = "Enable ComboFire",
                    Bindable = VitaruConfigManager.GetBindable<bool>(VitaruSetting.ComboFire)
                },
                new SettingsCheckbox
                {
                    LabelText = "Offline Multiplayer",
                    Bindable = multiplayer
                },
                multiplayerSettings = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = transition_duration,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,

                    Children = new Drawable[]
                    {
                        new SettingsSlider<int>
                        {
                            LabelText = "How many Friends?",
                            Bindable = friendlyPlayerCount,
                        },
                        new SettingsCheckbox
                        {
                            LabelText = "Override Friendly Characters",
                            Bindable = friendlyPlayerOverride
                        },
                        friendlyPlayerSettings = new FillFlowContainer
                        {
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            AutoSizeDuration = transition_duration,
                            AutoSizeEasing = Easing.OutQuint,
                            Masking = true,

                            Children = new Drawable[]
                            {
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "PlayerOne override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerOne)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "PlayerTwo override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerTwo)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "PlayerThree override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerThree)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "PlayerFour override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerFour)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "PlayerFive override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerFive)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "PlayerSix override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerSix)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "PlayerSeven override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerSeven)
                                }
                            }
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "How many Enemies?",
                            Bindable = enemyPlayerCount,
                        },
                        new SettingsCheckbox
                        {
                            LabelText = "Override Enemy Characters",
                            Bindable = enemyPlayerOverride
                        },
                        enemyPlayerSettings = new FillFlowContainer
                        {
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            AutoSizeDuration = transition_duration,
                            AutoSizeEasing = Easing.OutQuint,
                            Masking = true,

                            Children = new Drawable[]
                            {
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "EnemyOne override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.EnemyOne)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "EnemyTwo override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.EnemyTwo)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "EnemyThree override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.EnemyThree)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "EnemyFour override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.EnemyFour)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "EnemyFive override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.EnemyFive)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "EnemySix override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.EnemySix)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "EnemyEight override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.EnemySeven)
                                },
                                new SettingsEnumDropdown<Characters>
                                {
                                    LabelText = "EnemyEight override",
                                    Bindable = VitaruConfigManager.GetBindable<Characters>(VitaruSetting.EnemyEight)
                                }
                            }
                        },
                    }
                },
                new SettingsButton
                {
                    Text = "Open In-game Wiki",
                    Action = vitaruWiki.Show
                },
                new SettingsCheckbox
                {
                    LabelText = "Show Debug UI In-Game",
                    Bindable = VitaruConfigManager.GetBindable<bool>(VitaruSetting.DebugOverlay)
                },
                debugUiSettings = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = transition_duration,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,

                    Child = new SettingsEnumDropdown<DebugUiConfiguration>
                    {
                        LabelText = "What will be displayed on the DebugUI In-Game",
                        Bindable = VitaruConfigManager.GetBindable<DebugUiConfiguration>(VitaruSetting.DebugUIConfiguration)
                    }
                },
                skin = new SettingsDropdown<string>
                {
                    LabelText = "Current Skin"
                },
                new SettingsButton
                {
                    Text = "Open skins folder",
                    Action = skinsStorage.OpenInNativeExplorer,
                }
            };

            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Default", "default") };

            try
            {
                foreach (string skinName in storage.GetDirectories("Skins"))
                {
                    string[] args = skinName.Split('\\');
                    items.Add(new KeyValuePair<string, string>(args.Last(), args.Last()));
                }

                skin.Items = items.Distinct().ToList();
                currentSkin = VitaruConfigManager.GetBindable<string>(VitaruSetting.Skin);
                skin.Bindable = currentSkin;

                currentSkin.ValueChanged += skin => { VitaruConfigManager.Set(VitaruSetting.Skin, skin); };
            }
            catch { }

            //basically just an ingame wiki for the characters
            selectedCharacter.ValueChanged += character =>
            {
                if (character == Characters.AliceMuyart | character == Characters.ArysaMuyart && !VitaruAPIContainer.Shawdooow)
                {
                    selectedCharacter.Value = Characters.ReimuHakurei;
                    character = Characters.ReimuHakurei;
                }
            };
            selectedCharacter.TriggerChange();

            multiplayer.ValueChanged += isVisible =>
            {
                multiplayerSettings.ClearTransforms();
                multiplayerSettings.AutoSizeAxes = isVisible ? Axes.Y : Axes.None;

                if (!isVisible)
                    multiplayerSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
            };
            multiplayer.TriggerChange();

            friendlyPlayerOverride.ValueChanged += isVisible =>
            {
                friendlyPlayerSettings.ClearTransforms();
                friendlyPlayerSettings.AutoSizeAxes = isVisible ? Axes.Y : Axes.None;

                if (!isVisible)
                    friendlyPlayerSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
            };
            friendlyPlayerOverride.TriggerChange();

            enemyPlayerOverride.ValueChanged += isVisible =>
            {
                enemyPlayerSettings.ClearTransforms();
                enemyPlayerSettings.AutoSizeAxes = isVisible ? Axes.Y : Axes.None;

                if (!isVisible)
                    enemyPlayerSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
            };
            enemyPlayerOverride.TriggerChange();

            showDebugUi.ValueChanged += isVisible =>
            {
                debugUiSettings.ClearTransforms();
                debugUiSettings.AutoSizeAxes = isVisible ? Axes.Y : Axes.None;

                if (!isVisible)
                    debugUiSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
            };
            showDebugUi.TriggerChange();
        }
    }
}
