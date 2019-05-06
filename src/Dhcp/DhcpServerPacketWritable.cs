﻿using System;
using System.Runtime.InteropServices;

namespace Dhcp
{
    internal class DhcpServerPacketWritable : DhcpServerPacket, IDhcpServerPacketWritable, IDhcpServerPacketRawWritable
    {
        internal DhcpServerPacketWritable(IntPtr pointer, int size) : base(pointer, size)
        {
            BufferModified = false;
        }

        public bool BufferModified { get; private set; }

        internal void WriteBuffer(IntPtr packet, ref uint packetSize)
        {
            if (buffer != null && BufferModified)
            {
                if (buffer.Length > maxBufferLength)
                    throw new OutOfMemoryException("Packet buffer is too large");

                Marshal.Copy(buffer, 0, packet, buffer.Length);
                packetSize = (uint)buffer.Length;
            }
        }

        /// <inheritdoc />
        public bool TryAdjustBuffer(int index, int adjustmentAmount)
        {
            var buffer = base.buffer ?? Buffer;
            var length = GetLength();

            if (adjustmentAmount == 0)
                return true; // do nothing
            if (adjustmentAmount < 0)
            {
                // shrink buffer
                var shrinkBytes = Math.Abs(adjustmentAmount);

                if (length - index < shrinkBytes)
                    return false;

                // shrink buffer
                length -= shrinkBytes;
                base.length = length;
                if (index < length)
                    Array.Copy(buffer, index + shrinkBytes, buffer, index, length - index);
                Array.Clear(buffer, length, shrinkBytes);

                BufferModified = true;
            }
            else
            {
                // increase buffer
                if (!TryEnsureCapacity(length + adjustmentAmount))
                    return false;

                if (index < length)
                    Array.Copy(buffer, index, buffer, index + adjustmentAmount, length - index);
                base.length = length + adjustmentAmount;
                Array.Clear(buffer, index, adjustmentAmount);

                BufferModified = true;
            }
            return true;
        }

        private bool TryEnsureCapacity(int minimumLength)
        {
            if (minimumLength > maxBufferLength)
                return false;

            var buffer = base.buffer ?? Buffer;

            if (buffer.Length < minimumLength)
            {
                var length = GetLength();
                var destination = new byte[minimumLength];
                if (length > 0)
                    Array.Copy(buffer, 0, destination, 0, length);
                buffer = destination;
            }

            return true;
        }

        #region Packet Fields

        /// <inheritdoc />
        MessageTypes IDhcpServerPacketWritable.MessageType
        {
            get => MessageType;
            set => Buffer[OpOffset] = (byte)value;
        }

        /// <inheritdoc />
        DhcpServerHardwareType IDhcpServerPacketWritable.HardwareAddressType
        {
            get => HardwareAddressType;
            set => Buffer[HtypeOffset] = (byte)value;
        }

        /// <inheritdoc />
        byte IDhcpServerPacketWritable.HardwareAddressLength
        {
            get => HardwareAddressLength;
            set
            {
                if (value > 16)
                    throw new ArgumentOutOfRangeException(nameof(HardwareAddressLength) + " cannot be greater than 16");

                Buffer[HlenOffset] = value;
            }
        }

        byte IDhcpServerPacketWritable.GatewayHops
        {
            get => GatewayHops;
            set => Buffer[HopsOffset] = value;
        }

        int IDhcpServerPacketWritable.TransactionId
        {
            get => TransactionId;
            set => BitHelper.Write(Buffer, XidOffset, value);
        }

        ushort IDhcpServerPacketWritable.SecondsElapsed
        {
            get => SecondsElapsed;
            set => BitHelper.Write(Buffer, SecsOffset, value);
        }

        PacketFlags IDhcpServerPacketWritable.Flags
        {
            get => Flags;
            set => BitHelper.Write(Buffer, FlagsOffset, (ushort)value);
        }

        DhcpServerIpAddress IDhcpServerPacketWritable.ClientIpAddress
        {
            get => ClientIpAddress;
            set => BitHelper.Write(Buffer, CiaddrOffset, value);
        }

        DhcpServerIpAddress IDhcpServerPacketWritable.YourIpAddress
        {
            get => YourIpAddress;
            set => BitHelper.Write(Buffer, YiaddrOffset, value);
        }

        DhcpServerIpAddress IDhcpServerPacketWritable.NextServerIpAddress
        {
            get => NextServerIpAddress;
            set => BitHelper.Write(Buffer, SiaddrOffset, value);
        }

        DhcpServerIpAddress IDhcpServerPacketWritable.RelayAgentIpAddress
        {
            get => RelayAgentIpAddress;
            set => BitHelper.Write(Buffer, GiaddrOffset, value);
        }

        DhcpServerHardwareAddress IDhcpServerPacketWritable.ClientHardwareAddress
        {
            get => ClientHardwareAddress;
            set => value.ToNative(Buffer, ChaddrOffset, 16);
        }

        string IDhcpServerPacketWritable.ServerHostName
        {
            get => ServerHostName;
            set => BitHelper.WriteAscii(Buffer, SnameOffset, value, 64);
        }

        string IDhcpServerPacketWritable.FileName
        {
            get => FileName;
            set => BitHelper.WriteAscii(Buffer, FileOffset, value, 128);
        }

        int IDhcpServerPacketWritable.OptionsMagicCookie
        {
            get => OptionsMagicCookie;
            set => BitHelper.Write(Buffer, MagicCookieOffset, value);
        }

        #endregion

        #region TryAddOption

        public bool TryAddOption(OptionTags tag, byte[] data)
        {
            if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                endIndex = GetLength();
            return TryWriteOption(tag, data, endIndex, isUpdate: false);
        }

        public bool TryAddOption(OptionTags tag, long data)
        {
            if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                endIndex = GetLength();
            return TryWriteOption(tag, data, endIndex, isUpdate: false);
        }

        public bool TryAddOption(OptionTags tag, int data)
        {
            if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                endIndex = GetLength();
            return TryWriteOption(tag, data, endIndex, isUpdate: false);
        }

        public bool TryAddOption(OptionTags tag, short data)
        {
            if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                endIndex = GetLength();
            return TryWriteOption(tag, data, endIndex, isUpdate: false);
        }

        public bool TryAddOption(OptionTags tag, byte data)
        {
            if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                endIndex = GetLength();
            return TryWriteOption(tag, data, endIndex, isUpdate: false);
        }

        public bool TryAddOption(OptionTags tag, string data)
        {
            if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                endIndex = GetLength();
            return TryWriteOption(tag, data, endIndex, isUpdate: false);
        }

        public bool TryAddOption(OptionTags tag, DhcpServerIpAddress data) 
            => TryAddOption(tag, (int)data.Native);

        #endregion

        #region TryAddOrUpdateOption

        public bool TryAddOrUpdateOption(OptionTags tag, byte[] data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, isUpdate: true);
            else
            {
                if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                    endIndex = GetLength();
                return TryWriteOption(tag, data, endIndex, isUpdate: false);
            }
        }

        public bool TryAddOrUpdateOption(OptionTags tag, long data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, isUpdate: true);
            else
            {
                if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                    endIndex = GetLength();
                return TryWriteOption(tag, data, endIndex, isUpdate: false);
            }
        }

        public bool TryAddOrUpdateOption(OptionTags tag, int data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, isUpdate: true);
            else
            {
                if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                    endIndex = GetLength();
                return TryWriteOption(tag, data, endIndex, isUpdate: false);
            }
        }

        public bool TryAddOrUpdateOption(OptionTags tag, short data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, isUpdate: true);
            else
            {
                if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                    endIndex = GetLength();
                return TryWriteOption(tag, data, endIndex, isUpdate: false);
            }
        }

        public bool TryAddOrUpdateOption(OptionTags tag, byte data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, isUpdate: true);
            else
            {
                if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                    endIndex = GetLength();
                return TryWriteOption(tag, data, endIndex, isUpdate: false);
            }
        }

        public bool TryAddOrUpdateOption(OptionTags tag, string data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, isUpdate: true);
            else
            {
                if (!TryGetOptionIndex(OptionTags.End, out var endIndex))
                    endIndex = GetLength();
                return TryWriteOption(tag, data, endIndex, isUpdate: false);
            }
        }

        public bool TryAddOrUpdateOption(OptionTags tag, DhcpServerIpAddress data)
            => TryAddOrUpdateOption(tag, (int)data.Native);

        #endregion

        #region TryUpdateOption

        public bool TryUpdateOption(OptionTags tag, byte[] data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, true);

            return false;
        }

        public bool TryUpdateOption(OptionTags tag, long data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, true);

            return false;
        }

        public bool TryUpdateOption(OptionTags tag, int data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, true);

            return false;
        }

        public bool TryUpdateOption(OptionTags tag, short data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, true);

            return false;
        }

        public bool TryUpdateOption(OptionTags tag, byte data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, true);

            return false;
        }

        public bool TryUpdateOption(OptionTags tag, string data)
        {
            if (TryGetOptionIndex(tag, out var index))
                return TryWriteOption(tag, data, index, true);

            return false;
        }

        public bool TryUpdateOption(OptionTags tag, DhcpServerIpAddress data)
            => TryUpdateOption(tag, (int)data.Native);

        #endregion

        #region TryWriteOption

        private bool TryPrepWriteOption(OptionTags tag, int dataLength, int index, bool isUpdate, out int dataIndex)
        {
            var buffer = base.buffer ?? Buffer;
            dataIndex = -1;

            switch (tag)
            {
                case OptionTags.Pad:
                case OptionTags.End:
                    // 0-byte fixed length
                    if (dataLength != 0)
                        return false;

                    if (!isUpdate)
                    {
                        if (!TryAdjustBuffer(index, 1))
                            return false;

                        buffer[index] = (byte)tag;
                    }

                    dataIndex = ++index;
                    return true;
                case OptionTags.SubnetMask:
                case OptionTags.TimeOffset:
                    // 4-byte fixed length
                    if (dataLength != 4)
                        return false;

                    if (!isUpdate)
                    {
                        if (!TryAdjustBuffer(index, 5))
                            return false;

                        buffer[index] = (byte)tag;
                    }

                    dataIndex = ++index;
                    BufferModified = true;
                    return true;
                default:
                    // variable length

                    if (!isUpdate)
                    {
                        if (dataLength > byte.MaxValue)
                            return false;

                        if (!TryAdjustBuffer(index, 2 + dataLength))
                            return false;

                        buffer[index++] = (byte)tag;
                        buffer[index++] = (byte)dataLength;
                        dataIndex = index;
                        BufferModified = true;
                        return true;
                    }
                    else
                    {
                        var existingLength = buffer[++index];

                        if (dataLength == 0)
                        {
                            if (existingLength == 0)
                                return true; // no update
                            else
                                return TryAdjustBuffer(++index, -existingLength); // remove data
                        }
                        else if (dataLength > byte.MaxValue)
                            return false;
                        else
                        {
                            if (existingLength == dataLength)
                            {
                                dataIndex = ++index;
                                BufferModified = true;
                                return true;
                            }
                            else
                            {
                                // update option length
                                buffer[index++] = (byte)dataLength;

                                if (!TryAdjustBuffer(index, dataLength - existingLength))
                                    return false;

                                dataIndex = index;
                                return true;
                            }
                        }
                    }
            }
        }

        private bool TryWriteOption(OptionTags tag, byte[] data, int index, bool isUpdate)
        {
            var dataLength = data?.Length ?? 0;

            if (TryPrepWriteOption(tag, dataLength, index, isUpdate, out var dataIndex))
            {
                if (dataLength > 0)
                    Array.Copy(data, 0, buffer ?? Buffer, dataIndex, dataLength);

                return true;
            }

            return false;
        }

        private bool TryWriteOption(OptionTags tag, long data, int index, bool isUpdate)
        {
            if (TryPrepWriteOption(tag, BitHelper.Int64Size, index, isUpdate, out var dataIndex))
            {
                BitHelper.Write(buffer ?? Buffer, dataIndex, data);
                return true;
            }

            return false;
        }

        private bool TryWriteOption(OptionTags tag, int data, int index, bool isUpdate)
        {
            if (TryPrepWriteOption(tag, BitHelper.Int32Size, index, isUpdate, out var dataIndex))
            {
                BitHelper.Write(buffer ?? Buffer, dataIndex, data);
                return true;
            }

            return false;
        }

        private bool TryWriteOption(OptionTags tag, short data, int index, bool isUpdate)
        {
            if (TryPrepWriteOption(tag, BitHelper.Int16Size, index, isUpdate, out var dataIndex))
            {
                BitHelper.Write(buffer ?? Buffer, dataIndex, data);
                return true;
            }

            return false;
        }

        private bool TryWriteOption(OptionTags tag, byte data, int index, bool isUpdate)
        {
            if (TryPrepWriteOption(tag, BitHelper.ByteSize, index, isUpdate, out var dataIndex))
            {
                BitHelper.Write(buffer ?? Buffer, dataIndex, data);
                return true;
            }

            return false;
        }

        private bool TryWriteOption(OptionTags tag, string data, int index, bool isUpdate)
        {
            if (TryPrepWriteOption(tag, data.Length, index, isUpdate, out var dataIndex))
            {
                BitHelper.WriteAscii(buffer ?? Buffer, dataIndex, data);
                return true;
            }

            return false;
        }

        #endregion

    }
}