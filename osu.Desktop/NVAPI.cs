// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

#pragma warning disable IDE1006 // Naming rule violation

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using osu.Framework.Logging;

namespace osu.Desktop
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SupportedOSPlatform("windows")]
    internal static class NVAPI
    {
        private const string osu_filename = "osu!.exe";

        // This is a good reference:
        // https://github.com/errollw/Warp-and-Blend-Quadros/blob/master/WarpBlend-Quadros/UnwarpAll-Quadros/include/nvapi.h
        // Note our Stride == their VERSION (e.g. NVDRS_SETTING_VER)

        public const int MAX_PHYSICAL_GPUS = 64;
        public const int UNICODE_STRING_MAX = 2048;

        public const string APPLICATION_NAME = @"osu!";
        public const string PROFILE_NAME = @"osu!";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus EnumPhysicalGPUsDelegate([Out] IntPtr[] gpuHandles, out int gpuCount);

        public static readonly EnumPhysicalGPUsDelegate EnumPhysicalGPUs;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus EnumLogicalGPUsDelegate([Out] IntPtr[] gpuHandles, out int gpuCount);

        public static readonly EnumLogicalGPUsDelegate EnumLogicalGPUs;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus GetSystemTypeDelegate(IntPtr gpuHandle, out NvSystemType systemType);

        public static readonly GetSystemTypeDelegate GetSystemType;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus GetGPUTypeDelegate(IntPtr gpuHandle, out NvGpuType gpuType);

        public static readonly GetGPUTypeDelegate GetGPUType;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus CreateSessionDelegate(out IntPtr sessionHandle);

        public static CreateSessionDelegate CreateSession;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus LoadSettingsDelegate(IntPtr sessionHandle);

        public static LoadSettingsDelegate LoadSettings;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus FindApplicationByNameDelegate(IntPtr sessionHandle, [MarshalAs(UnmanagedType.BStr)] string appName, out IntPtr profileHandle, ref NvApplication application);

        public static FindApplicationByNameDelegate FindApplicationByName;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus GetCurrentGlobalProfileDelegate(IntPtr sessionHandle, out IntPtr profileHandle);

        public static GetCurrentGlobalProfileDelegate GetCurrentGlobalProfile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus GetProfileInfoDelegate(IntPtr sessionHandle, IntPtr profileHandle, ref NvProfile profile);

        public static GetProfileInfoDelegate GetProfileInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus GetSettingDelegate(IntPtr sessionHandle, IntPtr profileHandle, NvSettingID settingID, ref NvSetting setting);

        public static GetSettingDelegate GetSetting;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvStatus CreateProfileDelegate(IntPtr sessionHandle, ref NvProfile profile, out IntPtr profileHandle);

        private static readonly CreateProfileDelegate CreateProfile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvStatus SetSettingDelegate(IntPtr sessionHandle, IntPtr profileHandle, ref NvSetting setting);

        private static readonly SetSettingDelegate SetSetting;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvStatus EnumApplicationsDelegate(IntPtr sessionHandle, IntPtr profileHandle, uint startIndex, ref uint appCount, [In, Out, MarshalAs(UnmanagedType.LPArray)] NvApplication[] applications);

        private static readonly EnumApplicationsDelegate EnumApplications;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvStatus CreateApplicationDelegate(IntPtr sessionHandle, IntPtr profileHandle, ref NvApplication application);

        private static readonly CreateApplicationDelegate CreateApplication;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvStatus SaveSettingsDelegate(IntPtr sessionHandle);

        private static readonly SaveSettingsDelegate SaveSettings;

        public static NvStatus Status { get; private set; } = NvStatus.OK;
        public static bool Available { get; private set; }

        private static IntPtr sessionHandle;

        public static bool IsUsingOptimusDedicatedGpu
        {
            get
            {
                if (!Available)
                    return false;

                if (!IsLaptop)
                    return false;

                IntPtr profileHandle;
                if (!getProfile(out profileHandle, out _, out bool _))
                    return false;

                // Get the optimus setting
                NvSetting setting;
                if (!getSetting(NvSettingID.SHIM_RENDERING_MODE_ID, profileHandle, out setting))
                    return false;

                return (setting.U32CurrentValue & (uint)NvShimSetting.SHIM_RENDERING_MODE_ENABLE) > 0;
            }
        }

        public static bool IsLaptop
        {
            get
            {
                if (!Available)
                    return false;

                // Make sure that this is a laptop.
                IntPtr[] gpus = new IntPtr[64];
                if (checkError(EnumPhysicalGPUs(gpus, out int gpuCount)))
                    return false;

                for (int i = 0; i < gpuCount; i++)
                {
                    if (checkError(GetSystemType(gpus[i], out var type)))
                        return false;

                    if (type == NvSystemType.LAPTOP)
                        return true;
                }

                return false;
            }
        }

        public static NvThreadControlSetting ThreadedOptimisations
        {
            get
            {
                if (!Available)
                    return NvThreadControlSetting.OGL_THREAD_CONTROL_DEFAULT;

                IntPtr profileHandle;
                if (!getProfile(out profileHandle, out _, out bool _))
                    return NvThreadControlSetting.OGL_THREAD_CONTROL_DEFAULT;

                // Get the threaded optimisations setting
                NvSetting setting;
                if (!getSetting(NvSettingID.OGL_THREAD_CONTROL_ID, profileHandle, out setting))
                    return NvThreadControlSetting.OGL_THREAD_CONTROL_DEFAULT;

                return (NvThreadControlSetting)setting.U32CurrentValue;
            }
            set
            {
                if (!Available)
                    return;

                bool success = setSetting(NvSettingID.OGL_THREAD_CONTROL_ID, (uint)value);

                Logger.Log(success ? $"Threaded optimizations set to \"{value}\"!" : "Threaded optimizations set failed!");
            }
        }

        /// <summary>
        /// Checks if the profile contains the current application.
        /// </summary>
        /// <returns>If the profile contains the current application.</returns>
        private static bool containsApplication(IntPtr profileHandle, NvProfile profile, out NvApplication application)
        {
            application = new NvApplication
            {
                Version = NvApplication.Stride
            };

            if (profile.NumOfApps == 0)
                return false;

            NvApplication[] applications = new NvApplication[profile.NumOfApps];
            applications[0].Version = NvApplication.Stride;

            uint numApps = profile.NumOfApps;

            if (checkError(EnumApplications(sessionHandle, profileHandle, 0, ref numApps, applications)))
                return false;

            for (uint i = 0; i < numApps; i++)
            {
                if (applications[i].AppName == osu_filename)
                {
                    application = applications[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves the profile of the current application.
        /// </summary>
        /// <param name="profileHandle">The profile handle.</param>
        /// <param name="application">The current application description.</param>
        /// <param name="isApplicationSpecific">If this profile is not a global (default) profile.</param>
        /// <returns>If the operation succeeded.</returns>
        private static bool getProfile(out IntPtr profileHandle, out NvApplication application, out bool isApplicationSpecific)
        {
            application = new NvApplication
            {
                Version = NvApplication.Stride
            };

            isApplicationSpecific = true;

            if (checkError(FindApplicationByName(sessionHandle, osu_filename, out profileHandle, ref application)))
            {
                isApplicationSpecific = false;
                if (checkError(GetCurrentGlobalProfile(sessionHandle, out profileHandle)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a profile.
        /// </summary>
        /// <param name="profileHandle">The profile handle.</param>
        /// <returns>If the operation succeeded.</returns>
        private static bool createProfile(out IntPtr profileHandle)
        {
            NvProfile newProfile = new NvProfile
            {
                Version = NvProfile.Stride,
                IsPredefined = 0,
                ProfileName = PROFILE_NAME,
                GPUSupport = new uint[32]
            };

            newProfile.GPUSupport[0] = 1;

            if (checkError(CreateProfile(sessionHandle, ref newProfile, out profileHandle)))
                return false;

            return true;
        }

        /// <summary>
        /// Retrieves a setting from the profile.
        /// </summary>
        /// <param name="settingId">The setting to retrieve.</param>
        /// <param name="profileHandle">The profile handle to retrieve the setting from.</param>
        /// <param name="setting">The setting.</param>
        /// <returns>If the operation succeeded.</returns>
        private static bool getSetting(NvSettingID settingId, IntPtr profileHandle, out NvSetting setting)
        {
            setting = new NvSetting
            {
                Version = NvSetting.Stride,
                SettingID = settingId
            };

            if (checkError(GetSetting(sessionHandle, profileHandle, settingId, ref setting)))
                return false;

            return true;
        }

        private static bool setSetting(NvSettingID settingId, uint settingValue)
        {
            NvApplication application;
            IntPtr profileHandle;
            bool isApplicationSpecific;
            if (!getProfile(out profileHandle, out application, out isApplicationSpecific))
                return false;

            if (!isApplicationSpecific)
            {
                // We don't want to interfere with the user's other settings, so let's create a separate config for osu!
                if (!createProfile(out profileHandle))
                    return false;
            }

            NvSetting newSetting = new NvSetting
            {
                Version = NvSetting.Stride,
                SettingID = settingId,
                U32CurrentValue = settingValue
            };

            // Set the thread state
            if (checkError(SetSetting(sessionHandle, profileHandle, ref newSetting)))
                return false;

            // Get the profile (needed to check app count)
            NvProfile profile = new NvProfile
            {
                Version = NvProfile.Stride
            };
            if (checkError(GetProfileInfo(sessionHandle, profileHandle, ref profile)))
                return false;

            if (!containsApplication(profileHandle, profile, out application))
            {
                // Need to add the current application to the profile
                application.IsPredefined = 0;

                application.AppName = osu_filename;
                application.UserFriendlyName = APPLICATION_NAME;

                if (checkError(CreateApplication(sessionHandle, profileHandle, ref application)))
                    return false;
            }

            // Save!
            return !checkError(SaveSettings(sessionHandle));
        }

        /// <summary>
        /// Creates a session to access the driver configuration.
        /// </summary>
        /// <returns>If the operation succeeded.</returns>
        private static bool createSession()
        {
            if (checkError(CreateSession(out sessionHandle)))
                return false;

            // Load settings into session
            if (checkError(LoadSettings(sessionHandle)))
                return false;

            return true;
        }

        private static bool checkError(NvStatus status)
        {
            Status = status;
            return status != NvStatus.OK;
        }

        static NVAPI()
        {
            // TODO: check whether gpu vendor contains NVIDIA before attempting load?

            try
            {
                // Try to load NVAPI
                if ((IntPtr.Size == 4 && loadLibrary(@"nvapi.dll") == IntPtr.Zero)
                    || (IntPtr.Size == 8 && loadLibrary(@"nvapi64.dll") == IntPtr.Zero))
                {
                    return;
                }

                InitializeDelegate initialize;
                getDelegate(0x0150E828, out initialize);

                if (initialize?.Invoke() == NvStatus.OK)
                {
                    // IDs can be found here: https://github.com/jNizM/AHK_NVIDIA_NvAPI/blob/master/info/NvAPI_IDs.txt

                    getDelegate(0xE5AC921F, out EnumPhysicalGPUs);
                    getDelegate(0x48B3EA59, out EnumLogicalGPUs);
                    getDelegate(0xBAAABFCC, out GetSystemType);
                    getDelegate(0xC33BAEB1, out GetGPUType);
                    getDelegate(0x0694D52E, out CreateSession);
                    getDelegate(0x375DBD6B, out LoadSettings);
                    getDelegate(0xEEE566B2, out FindApplicationByName);
                    getDelegate(0x617BFF9F, out GetCurrentGlobalProfile);
                    getDelegate(0x577DD202, out SetSetting);
                    getDelegate(0x61CD6FD6, out GetProfileInfo);
                    getDelegate(0x73BF8338, out GetSetting);
                    getDelegate(0xCC176068, out CreateProfile);
                    getDelegate(0x7FA2173A, out EnumApplications);
                    getDelegate(0x4347A9DE, out CreateApplication);
                    getDelegate(0xFCBC7E14, out SaveSettings);
                }

                if (createSession())
                    Available = true;
            }
            catch { }
        }

        private static void getDelegate<T>(uint id, out T newDelegate) where T : class
        {
            IntPtr ptr = IntPtr.Size == 4 ? queryInterface32(id) : queryInterface64(id);
            newDelegate = ptr == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
        }

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        private static extern IntPtr loadLibrary(string dllToLoad);

        [DllImport(@"nvapi.dll", EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr queryInterface32(uint id);

        [DllImport(@"nvapi64.dll", EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr queryInterface64(uint id);

        private delegate NvStatus InitializeDelegate();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct NvSetting
    {
        public uint Version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.UNICODE_STRING_MAX)]
        public string SettingName;

        public NvSettingID SettingID;
        public uint SettingType;
        public uint SettingLocation;
        public uint IsCurrentPredefined;
        public uint IsPredefinedValid;

        public uint U32PredefinedValue;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.UNICODE_STRING_MAX)]
        public string StringPredefinedValue;

        public uint U32CurrentValue;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.UNICODE_STRING_MAX)]
        public string StringCurrentValue;

        public static uint Stride => (uint)Marshal.SizeOf(typeof(NvSetting)) | (1 << 16);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    internal struct NvProfile
    {
        public uint Version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.UNICODE_STRING_MAX)]
        public string ProfileName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public uint[] GPUSupport;

        public uint IsPredefined;
        public uint NumOfApps;
        public uint NumOfSettings;

        public static uint Stride => (uint)Marshal.SizeOf(typeof(NvProfile)) | (1 << 16);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    internal struct NvApplication
    {
        public uint Version;
        public uint IsPredefined;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.UNICODE_STRING_MAX)]
        public string AppName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.UNICODE_STRING_MAX)]
        public string UserFriendlyName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.UNICODE_STRING_MAX)]
        public string Launcher;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.UNICODE_STRING_MAX)]
        public string FileInFolder;

        public static uint Stride => (uint)Marshal.SizeOf(typeof(NvApplication)) | (2 << 16);
    }

    // ReSharper disable InconsistentNaming
    internal enum NvStatus
    {
        OK = 0, // Success. Request is completed.
        ERROR = -1, // Generic error
        LIBRARY_NOT_FOUND = -2, // NVAPI support library cannot be loaded.
        NO_IMPLEMENTATION = -3, // not implemented in current driver installation
        API_NOT_INITIALIZED = -4, // Initialize has not been called (successfully)
        INVALID_ARGUMENT = -5, // The argument/parameter value is not valid or NULL.
        NVIDIA_DEVICE_NOT_FOUND = -6, // No NVIDIA display driver, or NVIDIA GPU driving a display, was found.
        END_ENUMERATION = -7, // No more items to enumerate
        INVALID_HANDLE = -8, // Invalid handle
        INCOMPATIBLE_STRUCT_VERSION = -9, // An argument's structure version is not supported
        HANDLE_INVALIDATED = -10, // The handle is no longer valid (likely due to GPU or display re-configuration)
        OPENGL_CONTEXT_NOT_CURRENT = -11, // No NVIDIA OpenGL context is current (but needs to be)
        INVALID_POINTER = -14, // An invalid pointer, usually NULL, was passed as a parameter
        NO_GL_EXPERT = -12, // OpenGL Expert is not supported by the current drivers
        INSTRUMENTATION_DISABLED = -13, // OpenGL Expert is supported, but driver instrumentation is currently disabled
        NO_GL_NSIGHT = -15, // OpenGL does not support Nsight

        EXPECTED_LOGICAL_GPU_HANDLE = -100, // Expected a logical GPU handle for one or more parameters
        EXPECTED_PHYSICAL_GPU_HANDLE = -101, // Expected a physical GPU handle for one or more parameters
        EXPECTED_DISPLAY_HANDLE = -102, // Expected an NV display handle for one or more parameters
        INVALID_COMBINATION = -103, // The combination of parameters is not valid.
        NOT_SUPPORTED = -104, // Requested feature is not supported in the selected GPU
        PORTID_NOT_FOUND = -105, // No port ID was found for the I2C transaction
        EXPECTED_UNATTACHED_DISPLAY_HANDLE = -106, // Expected an unattached display handle as one of the input parameters.
        INVALID_PERF_LEVEL = -107, // Invalid perf level
        DEVICE_BUSY = -108, // Device is busy; request not fulfilled
        NV_PERSIST_FILE_NOT_FOUND = -109, // NV persist file is not found
        PERSIST_DATA_NOT_FOUND = -110, // NV persist data is not found
        EXPECTED_TV_DISPLAY = -111, // Expected a TV output display
        EXPECTED_TV_DISPLAY_ON_DCONNECTOR = -112, // Expected a TV output on the D Connector - HDTV_EIAJ4120.
        NO_ACTIVE_SLI_TOPOLOGY = -113, // SLI is not active on this device.
        SLI_RENDERING_MODE_NOTALLOWED = -114, // Setup of SLI rendering mode is not possible right now.
        EXPECTED_DIGITAL_FLAT_PANEL = -115, // Expected a digital flat panel.
        ARGUMENT_EXCEED_MAX_SIZE = -116, // Argument exceeds the expected size.
        DEVICE_SWITCHING_NOT_ALLOWED = -117, // Inhibit is ON due to one of the flags in NV_GPU_DISPLAY_CHANGE_INHIBIT or SLI active.
        TESTING_CLOCKS_NOT_SUPPORTED = -118, // Testing of clocks is not supported.
        UNKNOWN_UNDERSCAN_CONFIG = -119, // The specified underscan config is from an unknown source (e.g. INF)
        TIMEOUT_RECONFIGURING_GPU_TOPO = -120, // Timeout while reconfiguring GPUs
        DATA_NOT_FOUND = -121, // Requested data was not found
        EXPECTED_ANALOG_DISPLAY = -122, // Expected an analog display
        NO_VIDLINK = -123, // No SLI video bridge is present
        REQUIRES_REBOOT = -124, // NVAPI requires a reboot for the settings to take effect
        INVALID_HYBRID_MODE = -125, // The function is not supported with the current Hybrid mode.
        MIXED_TARGET_TYPES = -126, // The target types are not all the same
        SYSWOW64_NOT_SUPPORTED = -127, // The function is not supported from 32-bit on a 64-bit system.
        IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED = -128, // There is no implicit GPU topology active. Use SetHybridMode to change topology.
        REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS = -129, // Prompt the user to close all non-migratable applications.
        OUT_OF_MEMORY = -130, // Could not allocate sufficient memory to complete the call.
        WAS_STILL_DRAWING = -131, // The previous operation that is transferring information to or from this surface is incomplete.
        FILE_NOT_FOUND = -132, // The file was not found.
        TOO_MANY_UNIQUE_STATE_OBJECTS = -133, // There are too many unique instances of a particular type of state object.
        INVALID_CALL = -134, // The method call is invalid. For example, a method's parameter may not be a valid pointer.
        D3D10_1_LIBRARY_NOT_FOUND = -135, // d3d10_1.dll cannot be loaded.
        FUNCTION_NOT_FOUND = -136, // Couldn't find the function in the loaded DLL.
        INVALID_USER_PRIVILEGE = -137, // Current User is not Admin.
        EXPECTED_NON_PRIMARY_DISPLAY_HANDLE = -138, // The handle corresponds to GDIPrimary.
        EXPECTED_COMPUTE_GPU_HANDLE = -139, // Setting Physx GPU requires that the GPU is compute-capable.
        STEREO_NOT_INITIALIZED = -140, // The Stereo part of NVAPI failed to initialize completely. Check if the stereo driver is installed.
        STEREO_REGISTRY_ACCESS_FAILED = -141, // Access to stereo-related registry keys or values has failed.
        STEREO_REGISTRY_PROFILE_TYPE_NOT_SUPPORTED = -142, // The given registry profile type is not supported.
        STEREO_REGISTRY_VALUE_NOT_SUPPORTED = -143, // The given registry value is not supported.
        STEREO_NOT_ENABLED = -144, // Stereo is not enabled and the function needed it to execute completely.
        STEREO_NOT_TURNED_ON = -145, // Stereo is not turned on and the function needed it to execute completely.
        STEREO_INVALID_DEVICE_INTERFACE = -146, // Invalid device interface.
        STEREO_PARAMETER_OUT_OF_RANGE = -147, // Separation percentage or JPEG image capture quality is out of [0-100] range.
        STEREO_FRUSTUM_ADJUST_MODE_NOT_SUPPORTED = -148, // The given frustum adjust mode is not supported.
        TOPO_NOT_POSSIBLE = -149, // The mosaic topology is not possible given the current state of the hardware.
        MODE_CHANGE_FAILED = -150, // An attempt to do a display resolution mode change has failed.
        D3D11_LIBRARY_NOT_FOUND = -151, // d3d11.dll/d3d11_beta.dll cannot be loaded.
        INVALID_ADDRESS = -152, // Address is outside of valid range.
        STRING_TOO_SMALL = -153, // The pre-allocated string is too small to hold the result.
        MATCHING_DEVICE_NOT_FOUND = -154, // The input does not match any of the available devices.
        DRIVER_RUNNING = -155, // Driver is running.
        DRIVER_NOTRUNNING = -156, // Driver is not running.
        ERROR_DRIVER_RELOAD_REQUIRED = -157, // A driver reload is required to apply these settings.
        SET_NOT_ALLOWED = -158, // Intended setting is not allowed.
        ADVANCED_DISPLAY_TOPOLOGY_REQUIRED = -159, // Information can't be returned due to "advanced display topology".
        SETTING_NOT_FOUND = -160, // Setting is not found.
        SETTING_SIZE_TOO_LARGE = -161, // Setting size is too large.
        TOO_MANY_SETTINGS_IN_PROFILE = -162, // There are too many settings for a profile.
        PROFILE_NOT_FOUND = -163, // Profile is not found.
        PROFILE_NAME_IN_USE = -164, // Profile name is duplicated.
        PROFILE_NAME_EMPTY = -165, // Profile name is empty.
        EXECUTABLE_NOT_FOUND = -166, // Application not found in the Profile.
        EXECUTABLE_ALREADY_IN_USE = -167, // Application already exists in the other profile.
        DATATYPE_MISMATCH = -168, // Data Type mismatch
        PROFILE_REMOVED = -169, // The profile passed as parameter has been removed and is no longer valid.
        UNREGISTERED_RESOURCE = -170, // An unregistered resource was passed as a parameter.
        ID_OUT_OF_RANGE = -171, // The DisplayId corresponds to a display which is not within the normal outputId range.
        DISPLAYCONFIG_VALIDATION_FAILED = -172, // Display topology is not valid so the driver cannot do a mode set on this configuration.
        DPMST_CHANGED = -173, // Display Port Multi-Stream topology has been changed.
        INSUFFICIENT_BUFFER = -174, // Input buffer is insufficient to hold the contents.
        ACCESS_DENIED = -175, // No access to the caller.
        MOSAIC_NOT_ACTIVE = -176, // The requested action cannot be performed without Mosaic being enabled.
        SHARE_RESOURCE_RELOCATED = -177, // The surface is relocated away from video memory.
        REQUEST_USER_TO_DISABLE_DWM = -178, // The user should disable DWM before calling NvAPI.
        D3D_DEVICE_LOST = -179, // D3D device status is D3DERR_DEVICELOST or D3DERR_DEVICENOTRESET - the user has to reset the device.
        INVALID_CONFIGURATION = -180, // The requested action cannot be performed in the current state.
        STEREO_HANDSHAKE_NOT_DONE = -181, // Call failed as stereo handshake not completed.
        EXECUTABLE_PATH_IS_AMBIGUOUS = -182, // The path provided was too short to determine the correct NVDRS_APPLICATION
        DEFAULT_STEREO_PROFILE_IS_NOT_DEFINED = -183, // Default stereo profile is not currently defined
        DEFAULT_STEREO_PROFILE_DOES_NOT_EXIST = -184, // Default stereo profile does not exist
        CLUSTER_ALREADY_EXISTS = -185, // A cluster is already defined with the given configuration.
        DPMST_DISPLAY_ID_EXPECTED = -186, // The input display id is not that of a multi stream enabled connector or a display device in a multi stream topology
        INVALID_DISPLAY_ID = -187, // The input display id is not valid or the monitor associated to it does not support the current operation
        STREAM_IS_OUT_OF_SYNC = -188, // While playing secure audio stream, stream goes out of sync
        INCOMPATIBLE_AUDIO_DRIVER = -189, // Older audio driver version than required
        VALUE_ALREADY_SET = -190, // Value already set, setting again not allowed.
        TIMEOUT = -191, // Requested operation timed out
        GPU_WORKSTATION_FEATURE_INCOMPLETE = -192, // The requested workstation feature set has incomplete driver internal allocation resources
        STEREO_INIT_ACTIVATION_NOT_DONE = -193, // Call failed because InitActivation was not called.
        SYNC_NOT_ACTIVE = -194, // The requested action cannot be performed without Sync being enabled.
        SYNC_MASTER_NOT_FOUND = -195, // The requested action cannot be performed without Sync Master being enabled.
        INVALID_SYNC_TOPOLOGY = -196, // Invalid displays passed in the NV_GSYNC_DISPLAY pointer.
        ECID_SIGN_ALGO_UNSUPPORTED = -197, // The specified signing algorithm is not supported. Either an incorrect value was entered or the current installed driver/hardware does not support the input value.
        ECID_KEY_VERIFICATION_FAILED = -198, // The encrypted public key verification has failed.
        FIRMWARE_OUT_OF_DATE = -199, // The device's firmware is out of date.
        FIRMWARE_REVISION_NOT_SUPPORTED = -200, // The device's firmware is not supported.
    }

    internal enum NvSystemType
    {
        UNKNOWN = 0,
        LAPTOP = 1,
        DESKTOP = 2
    }

    internal enum NvGpuType
    {
        UNKNOWN = 0,
        IGPU = 1, // Integrated
        DGPU = 2, // Discrete
    }

    internal enum NvSettingID : uint
    {
        OGL_AA_LINE_GAMMA_ID = 0x2089BF6C,
        OGL_DEEP_COLOR_SCANOUT_ID = 0x2097C2F6,
        OGL_DEFAULT_SWAP_INTERVAL_ID = 0x206A6582,
        OGL_DEFAULT_SWAP_INTERVAL_FRACTIONAL_ID = 0x206C4581,
        OGL_DEFAULT_SWAP_INTERVAL_SIGN_ID = 0x20655CFA,
        OGL_EVENT_LOG_SEVERITY_THRESHOLD_ID = 0x209DF23E,
        OGL_EXTENSION_STRING_VERSION_ID = 0x20FF7493,
        OGL_FORCE_BLIT_ID = 0x201F619F,
        OGL_FORCE_STEREO_ID = 0x204D9A0C,
        OGL_IMPLICIT_GPU_AFFINITY_ID = 0x20D0F3E6,
        OGL_MAX_FRAMES_ALLOWED_ID = 0x208E55E3,
        OGL_MULTIMON_ID = 0x200AEBFC,
        OGL_OVERLAY_PIXEL_TYPE_ID = 0x209AE66F,
        OGL_OVERLAY_SUPPORT_ID = 0x206C28C4,
        OGL_QUALITY_ENHANCEMENTS_ID = 0x20797D6C,
        OGL_SINGLE_BACKDEPTH_BUFFER_ID = 0x20A29055,
        OGL_THREAD_CONTROL_ID = 0x20C1221E,
        OGL_TRIPLE_BUFFER_ID = 0x20FDD1F9,
        OGL_VIDEO_EDITING_MODE_ID = 0x20EE02B4,
        AA_BEHAVIOR_FLAGS_ID = 0x10ECDB82,
        AA_MODE_ALPHATOCOVERAGE_ID = 0x10FC2D9C,
        AA_MODE_GAMMACORRECTION_ID = 0x107D639D,
        AA_MODE_METHOD_ID = 0x10D773D2,
        AA_MODE_REPLAY_ID = 0x10D48A85,
        AA_MODE_SELECTOR_ID = 0x107EFC5B,
        AA_MODE_SELECTOR_SLIAA_ID = 0x107AFC5B,
        ANISO_MODE_LEVEL_ID = 0x101E61A9,
        ANISO_MODE_SELECTOR_ID = 0x10D2BB16,
        APPLICATION_PROFILE_NOTIFICATION_TIMEOUT_ID = 0x104554B6,
        APPLICATION_STEAM_ID_ID = 0x107CDDBC,
        CPL_HIDDEN_PROFILE_ID = 0x106D5CFF,
        CUDA_EXCLUDED_GPUS_ID = 0x10354FF8,
        D3DOGL_GPU_MAX_POWER_ID = 0x10D1EF29,
        EXPORT_PERF_COUNTERS_ID = 0x108F0841,
        FXAA_ALLOW_ID = 0x1034CB89,
        FXAA_ENABLE_ID = 0x1074C972,
        FXAA_INDICATOR_ENABLE_ID = 0x1068FB9C,
        MCSFRSHOWSPLIT_ID = 0x10287051,
        OPTIMUS_MAXAA_ID = 0x10F9DC83,
        PHYSXINDICATOR_ID = 0x1094F16F,
        PREFERRED_PSTATE_ID = 0x1057EB71,
        PREVENT_UI_AF_OVERRIDE_ID = 0x103BCCB5,
        PS_FRAMERATE_LIMITER_ID = 0x10834FEE,
        PS_FRAMERATE_LIMITER_GPS_CTRL_ID = 0x10834F01,
        SHIM_MAXRES_ID = 0x10F9DC82,
        SHIM_MCCOMPAT_ID = 0x10F9DC80,
        SHIM_RENDERING_MODE_ID = 0x10F9DC81,
        SHIM_RENDERING_OPTIONS_ID = 0x10F9DC84,
        SLI_GPU_COUNT_ID = 0x1033DCD1,
        SLI_PREDEFINED_GPU_COUNT_ID = 0x1033DCD2,
        SLI_PREDEFINED_GPU_COUNT_DX10_ID = 0x1033DCD3,
        SLI_PREDEFINED_MODE_ID = 0x1033CEC1,
        SLI_PREDEFINED_MODE_DX10_ID = 0x1033CEC2,
        SLI_RENDERING_MODE_ID = 0x1033CED1,
        VRRFEATUREINDICATOR_ID = 0x1094F157,
        VRROVERLAYINDICATOR_ID = 0x1095F16F,
        VRRREQUESTSTATE_ID = 0x1094F1F7,
        VSYNCSMOOTHAFR_ID = 0x101AE763,
        VSYNCVRRCONTROL_ID = 0x10A879CE,
        VSYNC_BEHAVIOR_FLAGS_ID = 0x10FDEC23,
        WKS_API_STEREO_EYES_EXCHANGE_ID = 0x11AE435C,
        WKS_API_STEREO_MODE_ID = 0x11E91A61,
        WKS_MEMORY_ALLOCATION_POLICY_ID = 0x11112233,
        WKS_STEREO_DONGLE_SUPPORT_ID = 0x112493BD,
        WKS_STEREO_SUPPORT_ID = 0x11AA9E99,
        WKS_STEREO_SWAP_MODE_ID = 0x11333333,
        AO_MODE_ID = 0x00667329,
        AO_MODE_ACTIVE_ID = 0x00664339,
        AUTO_LODBIASADJUST_ID = 0x00638E8F,
        ICAFE_LOGO_CONFIG_ID = 0x00DB1337,
        LODBIASADJUST_ID = 0x00738E8F,
        PRERENDERLIMIT_ID = 0x007BA09E,
        PS_DYNAMIC_TILING_ID = 0x00E5C6C0,
        PS_SHADERDISKCACHE_ID = 0x00198FFF,
        PS_TEXFILTER_ANISO_OPTS2_ID = 0x00E73211,
        PS_TEXFILTER_BILINEAR_IN_ANISO_ID = 0x0084CD70,
        PS_TEXFILTER_DISABLE_TRILIN_SLOPE_ID = 0x002ECAF2,
        PS_TEXFILTER_NO_NEG_LODBIAS_ID = 0x0019BB68,
        QUALITY_ENHANCEMENTS_ID = 0x00CE2691,
        REFRESH_RATE_OVERRIDE_ID = 0x0064B541,
        SET_POWER_THROTTLE_FOR_PCIe_COMPLIANCE_ID = 0x00AE785C,
        SET_VAB_DATA_ID = 0x00AB8687,
        VSYNCMODE_ID = 0x00A879CF,
        VSYNCTEARCONTROL_ID = 0x005A375C,
        TOTAL_DWORD_SETTING_NUM = 80,
        TOTAL_WSTRING_SETTING_NUM = 4,
        TOTAL_SETTING_NUM = 84,
        INVALID_SETTING_ID = 0xFFFFFFFF
    }

    internal enum NvShimSetting : uint
    {
        SHIM_RENDERING_MODE_INTEGRATED = 0x00000000,
        SHIM_RENDERING_MODE_ENABLE = 0x00000001,
        SHIM_RENDERING_MODE_USER_EDITABLE = 0x00000002,
        SHIM_RENDERING_MODE_MASK = 0x00000003,
        SHIM_RENDERING_MODE_VIDEO_MASK = 0x00000004,
        SHIM_RENDERING_MODE_VARYING_BIT = 0x00000008,
        SHIM_RENDERING_MODE_AUTO_SELECT = 0x00000010,
        SHIM_RENDERING_MODE_OVERRIDE_BIT = 0x80000000,
        SHIM_RENDERING_MODE_NUM_VALUES = 8,
        SHIM_RENDERING_MODE_DEFAULT = SHIM_RENDERING_MODE_AUTO_SELECT
    }

    internal enum NvThreadControlSetting : uint
    {
        OGL_THREAD_CONTROL_ENABLE = 0x00000001,
        OGL_THREAD_CONTROL_DISABLE = 0x00000002,
        OGL_THREAD_CONTROL_NUM_VALUES = 2,
        OGL_THREAD_CONTROL_DEFAULT = 0
    }

    // ReSharper restore InconsistentNaming
}
