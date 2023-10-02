using System;
using System.Runtime.InteropServices;
using System.Text;

public static class Ximu3
{
    const int Ximu3CharArraySize = 256;

    public enum ConnectionType
    {
        Usb,
        Serial,
        Tcp,
        Udp,
        Bluetooth,
        File,
    }

    public enum Result
    {
        Ok,
        Error
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CharArrays
    {
        public IntPtr array; // dynamic array of pointers to char arrays each of size Ximu3CharArraySize
        public UInt32 length;
        public UInt32 capacity;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UsbConnectionInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Ximu3CharArraySize)]
        public byte[] portName;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct SerialConnectionInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Ximu3CharArraySize)]
        public byte[] portName;

        public UInt32 baudRate;

        public bool rtsCtsEnabled;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct BluetoothConnectionInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Ximu3CharArraySize)]
        public byte[] portName;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct Device
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Ximu3CharArraySize)]
        public byte[] name;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Ximu3CharArraySize)]
        public byte[] serialNumber;

        public ConnectionType connectionType;

        public UsbConnectionInfo usbConnectionInfo;
        public SerialConnectionInfo serialConnectionInfo;
        public BluetoothConnectionInfo bluetoothConnectionInfo;

        public override string ToString()
        {
            var nameStr = Encoding.ASCII.GetString(name).TrimEnd((char)0);
            var serialStr = Encoding.ASCII.GetString(serialNumber).TrimEnd((char)0);
            return "{ Name: " + nameStr + ", Serial: " + serialStr + " }";
        }
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Devices
    {
        public IntPtr array; // dynamic array of Device structs
        public UInt32 length;
        public UInt32 capacity;

        public Device Get(int index)
        {
            if (index >= 0 && index < length)
            {
                // TODO: Use index variable to get IntPtr to correct Device struct offset in array, for now just grab first device
                Device device = (Device)Marshal.PtrToStructure(array, typeof(Device));
                return device;
            }
            
            return new Device();
        }
        
        public override string ToString()
        {
            return "{ Length: " + length + ", Capacity: " + capacity + " }";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct QuaternionMessage
    {
        public UInt64 timestamp;
        public float w, x, y, z;

        public override string ToString()
        {
            return "{ w: " + w + ", x: " + x + ", y: " + y + ", z: " + z + " }";
        }
    }

    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_char_arrays_free")]
    public static extern void CharArraysFree(CharArrays charArrays);

    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_port_scanner_scan_filter")]
    public static extern Devices PortScannerScanFilter(ConnectionType connectionType);

    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_devices_free")]
    public static extern void DevicesFree(Devices devices);

    public static string ResultToString(Result result)
    {
        return Marshal.PtrToStringAnsi(ResultToString_Native(result));
    }
    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "XIMU3_result_to_string")]
    public static extern IntPtr ResultToString_Native(Result result);

    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_connection_new_usb")]
    public static extern IntPtr ConnectionNewUsb(UsbConnectionInfo connectionInfo);

    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_connection_open")]
    public static extern Result ConnectionOpen(IntPtr connection);

    public static string ConnectionInfoToString(UsbConnectionInfo connectionInfo)
    {
        return Marshal.PtrToStringAnsi(ConnectionInfoToString_Native(connectionInfo));
    }
    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_usb_connection_info_to_string")]
    public static extern IntPtr ConnectionInfoToString_Native(UsbConnectionInfo connectionInfo);

    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_connection_send_commands")]
    public static extern CharArrays ConnectionSendCommands(IntPtr connection, [MarshalAs(UnmanagedType.LPStr)] string[] commands, UInt32 length, UInt32 retries, UInt32 timeout);

    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_connection_close")]
    public static extern void ConnectionClose(IntPtr connection);

    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_connection_free")]
    public static extern void ConnectionFree(IntPtr connection);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CallbackQuaternionDelegate(QuaternionMessage data, IntPtr context);

    [DllImport("ximu3", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XIMU3_connection_add_quaternion_callback")]
    public static extern UInt64 ConnectionAddQuaternionCallback(IntPtr connection, CallbackQuaternionDelegate callback, IntPtr context);
}