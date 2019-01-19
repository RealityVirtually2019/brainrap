using System;
using System.Runtime.InteropServices;
using Hash = System.Int32;

namespace Neurable.API
{
    public class Pointer
    {
        public Hash GetPointer()
        {
            return pointer;
        }

        protected Hash pointer = -1;
    };

    /* DataTypes for Neurable API */
    public static class Types
    {
        public delegate void DataCallback(double timestamp, string[] titles, double[] values);

        public delegate void RealDataCallback(double timestamp,
                                              IntPtr titles,
                                              IntPtr data,
                                              int size,
                                              IntPtr targetPointer);

        public delegate void TagCallback(int returnedID, IntPtr returnedDescription, IntPtr targetPointer);

        public enum CallbackType : byte
        {
            RAW_DATA = 1,
            FILTERED_DATA = 2,
            MENTAL_STATE = 3
        }

        public enum CommunicationProtocol : byte
        {
            TCP = 1,
            UDP = 2,
            LSL = 3
        }

        public enum ProjectionMatrix : byte
        {
            LeftProjectionFromLeftEye = 100,
            LeftEyeFromLeftProjection = 101,
            LeftEyeFromHead = 110,
            HeadFromLeftEye = 111,
            RightProjectionFromRightEye = 150,
            RightEyeFromRightProjection = 151,
            RightEyeFromHead = 160,
            HeadFromRightEye = 161,
            BinocularProjectionFromHead = 200,
            HeadFromBinocularProjection = 201,
            HeadFromWorld = 210,
            WorldFromHead = 211
        }

        public enum Sensitivity : byte
        {
            LOW = 5,
            MEDIUM = 10,
            HIGH = 15,
            VERY_HIGH = 20,
            EXTREME = 25
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
        public struct OpenGLMatrix
        {
            public float m00,
                         m01,
                         m02,
                         m03,
                         m10,
                         m11,
                         m12,
                         m13,
                         m20,
                         m21,
                         m22,
                         m23,
                         m30,
                         m31,
                         m32,
                         m33;
        }
    }

    /* Used to facilitate class-to-API communcations */
    internal static class Helpers
    {
        public static int[] createTagPointers(Tag[] tags)
        {
            var size = tags.Length;
            var t = new int[size];
            for (var i = 0; i < size; ++i) t[i] = tags[i].GetPointer();
            return t;
        }

        public static int[] createTagPointers(Tag tag)
        {
            return new[] {tag.GetPointer()};
        }

        public static string[] loadStringsFromBuffer(IntPtr buffer, int length, bool andFree = true)
        {
            var output = new string[length];
            var buffer2 = new IntPtr[length];
            Marshal.Copy(buffer, buffer2, 0, length);
            for (var i = 0; i < length; i++)
            {
                output[i] = Marshal.PtrToStringAnsi(buffer2[i]);
                if (andFree) Marshal.FreeCoTaskMem(buffer2[i]);
            }

            if (andFree) Marshal.FreeCoTaskMem(buffer);
            return output;
        }

        public static IntPtr[] storeStringsInBuffer(string[] strings)
        {
            var buffer = new IntPtr[strings.Length];
            for (var i = 0; i < strings.Length; i++) buffer[i] = Marshal.StringToHGlobalAnsi(strings[i]);
            return buffer;
        }
    }
}
