/*
 * Copyright 2017 Neurable Inc.
 */

using System;
using System.Runtime.InteropServices;
using Hash = System.Int32;

namespace Neurable.API {
    /* Raw API Interface */
    public static class Library
    {
        /* NEURABLE LIBRARY NAME */
        public const string NEURABLE_LIBRARY = "libNeurable";
        public const UInt32 NEURABLE_SDK_BUILD = 20u;

        /* VERSION */
        [DllImport(NEURABLE_LIBRARY, EntryPoint = "GetNeurableVersion", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetNeurableVersion();
        public static String GetVersion()
        {
            IntPtr cstr = GetNeurableVersion();
            return Marshal.PtrToStringAnsi(cstr);
        }
        public static UInt32 GetNeurableSDKBuild() { return NEURABLE_SDK_BUILD; }

        /* TAG */
        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern Hash CreateTag();

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool DeleteTag(Hash tagID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TagSetDescription(Hash t, string description);

        [DllImport(NEURABLE_LIBRARY, EntryPoint = "TagGetDescription", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tagGetDescription(Hash tagID, IntPtr outString);
        public static String TagGetDescription(Hash tagID) { IntPtr p = Marshal.AllocHGlobal(99); tagGetDescription(tagID, p); return Marshal.PtrToStringAnsi(p); }

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TagSetCameraPerspective(Hash t, float x, float y, float width, float height);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TagSetAction(Hash tagID, [MarshalAs(UnmanagedType.FunctionPtr)] Types.TagCallback action, IntPtr user_pointer);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TagSimulateAction(Hash tagID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TagSetAnimation(Hash tagID, [MarshalAs(UnmanagedType.FunctionPtr)] Types.TagCallback animation, IntPtr user_pointer);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TagSimulateAnimation(Hash tagID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TagSetActive(Hash tagID, [MarshalAs(UnmanagedType.I1)] bool active);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TagGetActive(Hash tagID);


        /* USER */
        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern Hash CreateUser([MarshalAs(UnmanagedType.LPStr)] string EEGDevicePort, bool UseEyetracker);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool DeleteUser(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserStartDataCollection(Hash userID, [MarshalAs(UnmanagedType.I1)] bool simulate);

        [DllImport(NEURABLE_LIBRARY, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserStopDataCollection(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserIsConnected(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserIsConnectedEEG(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserIsConnectedEye(Hash userID);

        [DllImport(NEURABLE_LIBRARY, EntryPoint = "UserIsConnected", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserIsReady(Hash userID); // Deprecated: Now Returns IsConnected

        [DllImport(NEURABLE_LIBRARY, EntryPoint = "UserIsConnectedEEG", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserIsReadyEEG(Hash userID); // Deprecated: Now Returns IsConnected

        [DllImport(NEURABLE_LIBRARY, EntryPoint = "UserIsConnectedEye", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserIsReadyEye(Hash userID); // Deprecated: Now Returns IsConnected

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetCameraResolution(Hash userID, float x, float y);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetCameraFOV(Hash userID, float x, float y, float nearClip, float farClip);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetHMDMatrix(Hash userID, byte matrixID, ref Types.OpenGLMatrix newMatrix);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetMetaDataHeader(Hash userID, [MarshalAs(UnmanagedType.LPStr)] string metaData = "");

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetMetaData(Hash userID, [MarshalAs(UnmanagedType.LPStr)] string metaData = "");

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetMetaDataFooter(Hash userID, [MarshalAs(UnmanagedType.LPStr)] string metaData = "");

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetDataCallback(Hash userID, Types.CallbackType data_type, Types.RealDataCallback callback, IntPtr pointer);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSimulateDataCallback(Hash userID, byte data_type);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetInteractable(Hash userID, [MarshalAs(UnmanagedType.I1)] bool canMakeSelections);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetSensitivity(Hash userID, byte value);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserUseEyeTrackingThresholds(Hash userID, bool enabled);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserRegisterEvent(Hash userID, Hash[] tagID, int size);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserRegisterTrainingEvent(Hash userID, Hash[] tagID, int size, [MarshalAs(UnmanagedType.I1)] bool attending);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserAcknowledgeSelection(Hash userID, Hash tagID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserClearEvents(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserSetCalibrationSequences(Hash userID, int calibrationSequences);

        [DllImport(NEURABLE_LIBRARY, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserModelExists(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserModelCalibrate(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserModelImport(Hash userID, [MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport(NEURABLE_LIBRARY, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UserModelExport(Hash userID, [MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UserResetArousalBaseline(Hash userID);

        /* USER DIAGNOSTICS */
        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern double UserDiagnosticsGetSampleRateEEG(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern double UserDiagnosticsGetSampleRateEye(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int UserDiagnosticsGetChannelCount(Hash userID);

        [DllImport(NEURABLE_LIBRARY, EntryPoint = "UserDiagnosticsGetChannelNames", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool userDiagnosticsGetChannelNames(Hash userID, out IntPtr buffer, out int length);
        public static String[] UserDiagnosticsGetChannelNames(Hash userID)
        {
            IntPtr buffer = IntPtr.Zero; int length = 0;
            userDiagnosticsGetChannelNames(userID, out buffer, out length);
            return Helpers.loadStringsFromBuffer(buffer, length);
        }

        [DllImport(NEURABLE_LIBRARY, EntryPoint = "UserDiagnosticsGetChannelColors", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool userDiagnosticsGetChannelColors(Hash userID, out IntPtr buffer, out int length, bool smartReset);
        public static String[] UserDiagnosticsGetChannelColors(Hash userID, bool smartReset)
        {
            IntPtr buffer = IntPtr.Zero; int length = 0;
            userDiagnosticsGetChannelColors(userID, out buffer, out length, smartReset);
            return Helpers.loadStringsFromBuffer(buffer, length);
        }

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UserDiagnosticsResetBaseline(Hash userID);

        [DllImport(NEURABLE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern double UserDiagnosticsGetResetCooldown(Hash userID);

        [DllImport(NEURABLE_LIBRARY, EntryPoint = "UserDiagnosticsGetChannelInstructions", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr userDiagnosticsGetChannelInstructions(Hash userID);
        public static String UserDiagnosticsGetChannelInstructions(Hash userID)
        {
            IntPtr cstr = userDiagnosticsGetChannelInstructions(userID);
            return Marshal.PtrToStringAnsi(cstr);
        }
    }
}