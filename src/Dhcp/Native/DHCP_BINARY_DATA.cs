﻿using System;
using System.Runtime.InteropServices;

namespace Dhcp.Native
{
    /// <summary>
    /// The DHCP_BINARY_DATA structure defines an opaque blob of binary data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct DHCP_BINARY_DATA
    {
        /// <summary>
        /// Specifies the size of Data, in bytes.
        /// </summary>
        public int DataLength;

        /// <summary>
        /// Pointer to an opaque blob of byte (binary) data.
        /// </summary>
        private IntPtr DataPointer;

        /// <summary>
        /// Blob of byte (binary) data.
        /// </summary>
        public byte[] Data
        {
            get
            {
                byte[] blob = new byte[this.DataLength];

                if (this.DataLength != 0)
                {
                    Marshal.Copy(this.DataPointer, blob, 0, this.DataLength);
                }

                return blob;
            }
        }
    }
}
