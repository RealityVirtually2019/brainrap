/*
 * Copyright 2017 Neurable Inc.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Neurable.API
{
    public class User : Pointer
    {
        public User(string eegDevicePort, bool useEyeTracker = true)
        {
            pointer = Library.CreateUser(eegDevicePort, useEyeTracker);
        }

        public User(bool doNotUse)
        {
            /* TESTING ONLY - DO NOT USE */
        }

        ~User()
        {
            Library.DeleteUser(pointer);
        }

        protected System.Collections.Generic.List<Types.RealDataCallback> callbacks;

        public bool StartDataCollection(bool simulate = false)
        {
            return Library.UserStartDataCollection(pointer, simulate);
        }

        public bool StopDataCollection()
        {
            return Library.UserStopDataCollection(pointer);
        }

        public bool IsConnected()
        {
            return Library.UserIsConnected(pointer);
        }

        public bool IsConnectedEeg()
        {
            return Library.UserIsConnectedEEG(pointer);
        }

        public bool IsConnectedEye()
        {
            return Library.UserIsConnectedEye(pointer);
        }

        public bool IsReady()
        {
            return IsConnected();
        } // Deprecated

        public bool IsReadyEeg()
        {
            return IsConnectedEeg();
        } // Deprecated

        public bool IsReadyEye()
        {
            return IsConnectedEye();
        } // Deprecated

        public void SetCameraResolution(float x, float y)
        {
            Library.UserSetCameraResolution(pointer, x, y);
        }

        public void SetCameraFov(float x, float y, float nearClip, float farClip)
        {
            Library.UserSetCameraFOV(pointer, x, y, nearClip, farClip);
        }

        public void SetHmdMatrix(Types.ProjectionMatrix matrixId, ref Types.OpenGLMatrix newMatrix)
        {
            Library.UserSetHMDMatrix(pointer, (byte) matrixId, ref newMatrix);
        }

        public bool SetMetaDataHeader(string metaData)
        {
            return Library.UserSetMetaDataHeader(pointer, metaData);
        }

        public bool SetMetaData(string metaData)
        {
            return Library.UserSetMetaData(pointer, metaData);
        }

        public bool SetMetaDataFooter(string metaData)
        {
            return Library.UserSetMetaDataFooter(pointer, metaData);
        }

        public bool SetDataCallback(Types.DataCallback dataCallback,
                                    Types.CallbackType dataType = Types.CallbackType.RAW_DATA)
        {
            if (dataCallback == null) return SetDataCallback(dataType, null);
            if (callbacks == null) callbacks = new System.Collections.Generic.List<Types.RealDataCallback>();
            Types.RealDataCallback wrapped = (double timestamp, IntPtr titles, IntPtr values, int size, IntPtr ptr) =>
                                             {
                                                 String[] dataTitles =
                                                     Helpers.loadStringsFromBuffer(titles, size, false);
                                                 double[] data = new double[size];
                                                 Marshal.Copy(values, data, 0, size);
                                                 dataCallback(timestamp, dataTitles, data);
                                             };
            callbacks.Add(wrapped);
            return SetDataCallback(dataType, wrapped);
        }

        protected bool SetDataCallback(Types.CallbackType dataType, Types.RealDataCallback callback)
        {
            return Library.UserSetDataCallback(pointer, dataType, callback, new IntPtr());
        }

        public bool SimulateDataCallback(byte dataType)
        {
            return Library.UserSimulateDataCallback(pointer, dataType);
        }

        public bool SetInteractable(bool canMakeSelections)
        {
            return Library.UserSetInteractable(pointer, canMakeSelections);
        }

        public bool SetSensitivity(Types.Sensitivity value)
        {
            return Library.UserSetSensitivity(pointer, (byte) value);
        }

        public bool UseEyeTrackingThresholds(bool enabled)
        {
            return Library.UserUseEyeTrackingThresholds(pointer, enabled);
        }

        public bool RegisterEvent(Tag tag)
        {
            Tag[] tags = new[] {tag};
            return RegisterEvent(tags);
        }

        public bool RegisterEvent(Tag[] tags)
        {
            return Library.UserRegisterEvent(pointer, Helpers.createTagPointers(tags), tags.Length);
        }

        public bool RegisterTrainingEvent(Tag tag, bool attending)
        {
            Tag[] tags = new[] {tag};
            return RegisterTrainingEvent(tags, attending);
        }

        public bool RegisterTrainingEvent(Tag[] tags, bool attending)
        {
            return Library.UserRegisterTrainingEvent(pointer, Helpers.createTagPointers(tags), tags.Length, attending);
        }

        public bool AcknowledgeSelection(Tag tag)
        {
            return Library.UserAcknowledgeSelection(pointer, tag.GetPointer());
        }

        public bool SetCalibrationSequences(int sequences)
        {
            return Library.UserSetCalibrationSequences(pointer, sequences);
        }

        public bool ClearEvents()
        {
            return Library.UserClearEvents(pointer);
        }

        public bool HasModel()
        {
            return Library.UserModelExists(pointer);
        }

        public bool Calibrate()
        {
            return Library.UserModelCalibrate(pointer);
        }

        public bool ImportModel(string filename)
        {
            return Library.UserModelImport(pointer, filename);
        }

        public bool ExportModel(string filename)
        {
            return Library.UserModelExport(pointer, filename);
        }

        public bool ResetArousalBaseline()
        {
            return Library.UserResetArousalBaseline(pointer);
        }

        public double GetSampleRateEeg()
        {
            return Library.UserDiagnosticsGetSampleRateEEG(pointer);
        }

        public double GetSampleRateEye()
        {
            return Library.UserDiagnosticsGetSampleRateEye(pointer);
        }

        public int GetDiagnosticChannelCount()
        {
            return Library.UserDiagnosticsGetChannelCount(pointer);
        }

        public String[] GetDiagnosticChannelNames()
        {
            return Library.UserDiagnosticsGetChannelNames(pointer);
        }

        public String[] GetDiagnosticChannelColors(bool smartReset = true)
        {
            return Library.UserDiagnosticsGetChannelColors(pointer, smartReset);
        }

        public double GetDiagnosticCountdown()
        {
            return Library.UserDiagnosticsGetResetCooldown(pointer);
        }

        public bool ResetDiagnosticBaseline()
        {
            return Library.UserDiagnosticsResetBaseline(pointer);
        }

        public String GetDiagnosticChannelInstructions()
        {
            return Library.UserDiagnosticsGetChannelInstructions(pointer);
        }
    };
}
