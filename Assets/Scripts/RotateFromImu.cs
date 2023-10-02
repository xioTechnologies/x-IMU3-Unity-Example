using System;
using UnityEngine;
public class RotateFromImu : MonoBehaviour
{
    private bool _connected = false;
    private IntPtr _connection;
    private static Ximu3.CallbackQuaternionDelegate _quaternionDelegateInstance;
    private readonly object _mutex = new();
    private Quaternion _sharedQuat;
    
    private void Start()
    {
        Debug.Log("Searching for x-IMU3 connections");

        var devices = Ximu3.PortScannerScanFilter(Ximu3.ConnectionType.Usb);

        if (devices.length == 0)
        {
            Debug.Log("No USB connections available");
            return;
        }

        var firstUsbDevice = devices.Get(0);
        Debug.Log("Found " + firstUsbDevice);

        var connectionInfo = firstUsbDevice.usbConnectionInfo;

        Ximu3.DevicesFree(devices);

        // Connect to first found USB device
        _connection = Ximu3.ConnectionNewUsb(connectionInfo);
        var connectionInfoDescription = Ximu3.ConnectionInfoToString(connectionInfo);

        // Register callback for quaternion rotation data
        _quaternionDelegateInstance = QuaternionCallback;
        Ximu3.ConnectionAddQuaternionCallback(_connection, _quaternionDelegateInstance, IntPtr.Zero);

        Debug.Log("Connecting to " + connectionInfoDescription);
        if (Ximu3.ConnectionOpen(_connection) != Ximu3.Result.Ok)
        {
            Debug.Log("Unable to open connection");
            Ximu3.ConnectionFree(_connection);
            return;
        }
        _connected = true;

        Debug.Log("Connection successful");

        // Send command to strobe LED
        string[] commands = { "{\"strobe\":null}" };
        var response = Ximu3.ConnectionSendCommands(_connection, commands, 1, 2, 500);
        Ximu3.CharArraysFree(response);
    }

    private void QuaternionCallback(Ximu3.QuaternionMessage data, IntPtr context)
    {
        Debug.Log(data.ToString());
        lock (_mutex)
        {
            _sharedQuat.x = data.x;
            _sharedQuat.y = data.y;
            _sharedQuat.z = data.z;
            _sharedQuat.w = data.w;
        }
    }

    private void OnDestroy()
    {
        if (!_connected) return;
        
        Debug.Log("Closing connection");
        Ximu3.ConnectionClose(_connection);
        Ximu3.ConnectionFree(_connection);
    }

    private void Update()
    {
        lock (_mutex)
        {
            transform.rotation = _sharedQuat;
        }
    }
}
