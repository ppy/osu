//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.IO;
using System.Security.AccessControl;
using Microsoft.Win32;

namespace osu.Framework.Desktop.OS.Windows.Native
{
    public static class Registry
    {
        public static bool Check(string progId, string executable)
        {
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(progId))
                {
                    if (key == null || key.OpenSubKey(@"shell\open\command").GetValue(string.Empty).ToString() != string.Format("\"{0}\" \"%1\"", executable))
                        return false;
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        public static bool Add(string extension, string progId, string description, string executable, bool urlHandler = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(extension))
                {
                    if (extension[0] != '.')
                        extension = "." + extension;

                    // register the extension, if necessary
                    using (RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension, true))
                    {
                        if (key == null)
                        {
                            using (RegistryKey extKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(extension))
                            {
                                extKey.SetValue(string.Empty, progId);
                            }
                        }
                        else
                        {
                            key.SetValue(string.Empty, progId);
                        }
                    }
                }

                // register the progId, if necessary
                using (RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(progId))
                {
                    if (key == null || key.OpenSubKey("shell\\open\\command").GetValue(string.Empty).ToString() != string.Format("\"{0}\" \"%1\"", executable))
                    {
                        using (RegistryKey progIdKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(progId))
                        {
                            progIdKey.SetValue(string.Empty, description);
                            if (urlHandler) progIdKey.SetValue("URL Protocol", string.Empty);

                            using (RegistryKey command = progIdKey.CreateSubKey("shell\\open\\command"))
                            {
                                command.SetValue(string.Empty, string.Format("\"{0}\" \"%1\"", executable));
                            }

                            using (RegistryKey command = progIdKey.CreateSubKey("DefaultIcon"))
                            {
                                command.SetValue(string.Empty, string.Format("\"{0}\",1", executable));
                            }
                        }
                    }
                    // Add new icon for existing osu! users anyway
                    else if (key.OpenSubKey("DefaultIcon").GetValue(string.Empty) == null)
                    {
                        using (RegistryKey progIdKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(progId))
                        {
                            using (RegistryKey command = progIdKey.CreateSubKey("DefaultIcon"))
                            {
                                command.SetValue(string.Empty, string.Format("\"{0}\",1", executable));
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static void Delete(string progId)
        {
            try
            {
                if (null != Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(progId))
                    Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(progId);
            }
            catch
            {
            }

        }

        // Adds an ACL entry on the specified directory for the specified account.
        public static void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights,
                                                InheritanceFlags Inheritance, PropagationFlags Propogation,
                                                AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);
            // Get a DirectorySecurity object that represents the
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            // Add the FileSystemAccessRule to the security settings.
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account,
                                                             Rights,
                                                             Inheritance,
                                                             Propogation,
                                                             ControlType));
            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);
        }
    }
}
