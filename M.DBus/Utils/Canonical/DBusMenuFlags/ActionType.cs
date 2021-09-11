namespace M.DBus.Utils.Canonical.DBusMenuFlags
{
    public class ToggleType
    {
        public static string SIndependentToggleable => "checkmark";
        public static string SRadio => "radio";
        public static string SNone => string.Empty;

        public string IndependentToggleable => SIndependentToggleable;
        public string Radio => SRadio;
        public string None => SNone;
    }
}
