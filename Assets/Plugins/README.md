# Plugins Folder
This folder contains binary software libraries built from the x-IMU3 API in the repo: https://github.com/xioTechnologies/x-IMU3-Software

These software libraries act as [Unity Native Plugins](https://docs.unity3d.com/Manual/NativePlugins.html) which allow the use of the x-IMU3 API via C# in Unity. For the moment, only a Windows plugin exists as a `.dll` file. Because of this, Unity projects utilizing the x-IMU3 API can only be built and deployed on Windows.

The native plugin file formats required by Unity for other platforms are detailed here: https://docs.unity3d.com/Manual/PluginsForDesktop.html

An example of a Unity Native Plugin which supports all plaforms can be found here: https://github.com/Unity-Technologies/DesktopSamples/tree/master/SimplestPluginExample

## Building the Dynamic Link Library (.dll) for Windows
1. Clone https://github.com/xioTechnologies/x-IMU3-Software and follow the [README.md](https://github.com/xioTechnologies/x-IMU3-Software#development-setup) to setup development.
2. Navigate to the file `x-IMU3-API/Rust/Cargo.toml`.
3. Find `crate-type` and add the string `"dylib"` to its list so the line looks like the following:
```
crate-type = ["rlib", "staticlib", "dylib"]
```
4. Build the Rust-API target.
5. The file `ximu3.dll` will now exist in `x-IMU3-API/Rust/target/release`.