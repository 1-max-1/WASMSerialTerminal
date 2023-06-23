# WASM Serial Terminal
A simple offline [blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) PWA for quick analysis of data from a serial device.

![demo-preview](https://user-images.githubusercontent.com/44454544/248429563-2e7cacde-e968-4b0f-bc7d-9e54ac979e6e.png)

This tool allows you to connect to a serial port and view the incoming data.
Data is displayed in both textual format and byte by byte format - the latter resides in a grid with each row representing one byte.

You can also send data to the serial device in either text or individual byte format.

The HTML is somewhat responsive, but UI for the web is definitely not my strong point so it may still be dodgy on mobile-sized screens (laptops should be fine).
However, at the time of writing this, the web serial API is not supported on mobile browsers, so this app isn't currently usable on mobile anyway.

# Installation
This app uses [Radzen's blazor components](https://www.radzen.com/blazor-components/). However, all dependencies are from nuget, so there is no need to install anything extra. Just clone this repo and restore packages.

# Code notes
This project uses [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor), which does not support the regular .NET method of accessing serial ports through `System.IO.Ports`. Instead, we must use the [web serial API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Serial_API).
In this project, the `blazor-serial.js` file in `wwwroot` contains the JS code for connecting to and communicating with serial ports using the web serial API.
The `SerialService.cs` file contains the C# service that interacts with this JS code and exposes its serial functions to the rest of the project.

In the main page (`Index.razor`) some parts of the UI have been separated into their own components and these components are located in the `Shared` folder.

The `Shared` folder also contains a component called `WebSerialSupportChecker.razor`. This component is for use in `App.razor`. It checks if the users browser supports the web serial API.
If it is not supported, then an error message is displayed. Otherwise, the component lets `App.razor` render the rest of its regular content.

The `wwroot` folder contains a list of USB device ID's in JSON format (obtained from [https://github.com/1-max-1/usb_ids_api](https://github.com/1-max-1/usb_ids_api)). The app attempts to identify the connected device by looking through this list.

# Github pages
This project is currently hosted on github pages [here](https://1-max-1.github.io/WASMSerialTerminal).
On every push to the `main` branch, a github action builds and deploys the project.