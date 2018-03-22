using eden.Game.GamePieces;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Rulesets.Vitaru.Edit;
using osu.Game.Rulesets.Vitaru.Objects.Characters;
using osu.Game.Rulesets.Vitaru.Scoring;

namespace osu.Game.Rulesets.Vitaru.Settings
{
    public class VitaruConfigManager : IniConfigManager<VitaruSetting>
    {
        protected override string Filename => @"vitaru.ini";

        public VitaruConfigManager(Storage storage) : base(storage) { }

        protected override void InitialiseDefaults()
        {
            Set(VitaruSetting.ScoringMetric, ScoringMetric.Graze);
            Set(VitaruSetting.DebugOverlay, false);
            Set(VitaruSetting.DebugUIConfiguration, DebugUiConfiguration.PerformanceMetrics);
            Set(VitaruSetting.GraphicsPresets, GraphicsPresets.Standard);
            Set(VitaruSetting.GameMode, VitaruGamemode.Vitaru);
            Set(VitaruSetting.Characters, Characters.ReimuHakurei);
            Set(VitaruSetting.EditorConfiguration, EditorConfiguration.Simple);
            Set(VitaruSetting.ComboFire, true);
            Set(VitaruSetting.ShittyMultiplayer, false);
            Set(VitaruSetting.FriendlyPlayerCount, 0, 0, 7);
            Set(VitaruSetting.FriendlyPlayerOverride, false);
            Set(VitaruSetting.EnemyPlayerCount, 0, 0, 8);
            Set(VitaruSetting.EnemyPlayerOverride, false);

            Set(VitaruSetting.PlayerOne, Characters.MarisaKirisame);
            Set(VitaruSetting.PlayerTwo, Characters.SakuyaIzayoi);
            Set(VitaruSetting.PlayerThree, Characters.FlandreScarlet);
            Set(VitaruSetting.PlayerFour, Characters.RemiliaScarlet);
            Set(VitaruSetting.PlayerFive, Characters.Cirno);
            Set(VitaruSetting.PlayerSix, Characters.TenshiHinanai);
            Set(VitaruSetting.PlayerSeven, Characters.YukariYakumo);

            Set(VitaruSetting.EnemyOne, Characters.MarisaKirisame);
            Set(VitaruSetting.EnemyTwo, Characters.SakuyaIzayoi);
            Set(VitaruSetting.EnemyThree, Characters.FlandreScarlet);
            Set(VitaruSetting.EnemyFour, Characters.RemiliaScarlet);
            Set(VitaruSetting.EnemyFive, Characters.Cirno);
            Set(VitaruSetting.EnemySix, Characters.TenshiHinanai);
            Set(VitaruSetting.EnemySeven, Characters.YukariYakumo);
            Set(VitaruSetting.EnemyEight, Characters.Chen);

            Set(VitaruSetting.VectorVideos, true);
            Set(VitaruSetting.Skin, "default");

            //Touhosu
            Set(VitaruSetting.Familiar, false);
            Set(VitaruSetting.Late, false);
            Set(VitaruSetting.LastDance, false);
            Set(VitaruSetting.Insane, true);
            Set(VitaruSetting.Awoken, false);
            Set(VitaruSetting.Sacred, false);
            Set(VitaruSetting.Resurrected, false);
            Set(VitaruSetting.Bonded, false);
            Set(VitaruSetting.Revenge, false);

            //Online Multiplayer
            Set(VitaruSetting.HostIP, "Host IP Address");
            Set(VitaruSetting.LocalIP, "Your Local IP Address");
        }

    }

    public enum VitaruSetting
    {
        ScoringMetric,
        DebugOverlay,
        DebugUIConfiguration,
        GraphicsPresets,
        GameMode,
        Characters,
        EditorConfiguration,
        ComboFire,
        ShittyMultiplayer,
        FriendlyPlayerCount,
        FriendlyPlayerOverride,
        EnemyPlayerCount,
        EnemyPlayerOverride,

        //Becuase fuck arrays
        PlayerOne,
        PlayerTwo,
        PlayerThree,
        PlayerFour,
        PlayerFive,
        PlayerSix,
        PlayerSeven,

        //See above comment
        EnemyOne,
        EnemyTwo,
        EnemyThree,
        EnemyFour,
        EnemyFive,
        EnemySix,
        EnemySeven,
        EnemyEight,

        VectorVideos,
        Skin,

        //Touhosu
        Familiar,
        Late,
        LastDance,
        Insane,
        Awoken,
        Sacred,
        Resurrected,
        Bonded,
        Revenge,

        HostIP,
        LocalIP,
    }

    public enum GraphicsPresets
    {
        HighPerformance,
        Standard,
        StandardCompetitive,
        HighPerformanceCompetitive
    }
}
