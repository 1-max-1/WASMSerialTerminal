using Microsoft.JSInterop;
using System.Text.Json;

namespace WASMSerialTerminal {
	internal interface ISerialService {
		/// <summary>
		/// Sends a request to the browser to open a serial port. <br/>
		/// The browser will open a dialog for the user to select which port they wish to open. <br/>
		/// If the user selects a port, that port will be opened and data will begin to stream.
		/// </summary>
		/// <returns><see langword="true"/> if the user selected a port, <see langword="false"/> if they cancelled the operation.</returns>
		/// <remarks>Will throw <see cref="InvalidOperationException"/> if a port is already open.</remarks>
		/// <exception cref="InvalidOperationException" />
		/// <exception cref="SerialSecurityException" />
		/// <exception cref="SerialInitializationException" />
		/// <exception cref="ArgumentException" />
		public Task<bool> OpenPortSelectionDialog(uint baudRate, SerialPortDataBits dataBits, SerialPortFlowControl flowControl, SerialPortParity parity, SerialPortStopBits stopBits);

		/// <summary>
		/// Gets the number of already paired serial ports. <br/>
		/// Once a serial port has been open using OpenPortSelectionDialog it becomes "Paired" <br/>
		/// Users can open paired serial ports without prompting the user for permissions <br/>
		/// Disconnected devices will be ignored
		/// </summary>
		/// <returns>The number of already paired serial ports</returns>
		public Task<int> GetNumberOfPairedSerialPorts();

		/// <summary>
		/// Asks the browser for a list of paired serial devices. <br/>
		/// Only currently connected devices are considered <br/>
		/// </summary>
		/// <returns>An array of SerialPortDescription</returns>
		public Task<IEnumerable<SerialPortDescription>> GetPairedSerialPortsDescriptions();

		/// <summary>
		/// Connects to an already paired serial port. <br/>
		/// This will not trigger a user prompt
		/// </summary>
		/// <param name="serialPortIndex">Choose what port to connect to by giving its index in the GetPairedSerialPortsDescriptions() array</param>
		/// <returns><see langword="true"/> if the connection is successfull.</returns>
		/// <remarks>Will throw <see cref="InvalidOperationException"/> if a port is already open.</remarks>
		/// <exception cref="InvalidOperationException" />
		/// <exception cref="SerialSecurityException" />
		/// <exception cref="SerialInitializationException" />
		/// <exception cref="ArgumentException" />
		/// <exception cref="IndexOutOfRangeException" />
		public Task<bool> OpenPairedSerialPort(int serialPortIndex, uint baudRate, SerialPortDataBits dataBits, SerialPortFlowControl flowControl, SerialPortParity parity, SerialPortStopBits stopBits);

		/// <summary>
		/// Connects to an already paired serial port. <br/>
		/// This will not trigger a user prompt
		/// </summary>
		/// <param name="serialPortIndex">Choose what port to connect to by giving its description</param>
		/// <returns><see langword="true"/> if the connection is successfull.</returns>
		/// <remarks>Will throw <see cref="InvalidOperationException"/> if a port is already open.</remarks>
		/// <exception cref="InvalidOperationException" />
		/// <exception cref="SerialSecurityException" />
		/// <exception cref="SerialInitializationException" />
		/// <exception cref="ArgumentException" />
		/// <exception cref="SerialPortUnavailableException" />
		public Task<bool> OpenPairedSerialPort(SerialPortDescription serialPortDescription, uint baudRate, SerialPortDataBits dataBits, SerialPortFlowControl flowControl, SerialPortParity parity, SerialPortStopBits stopBits);


		/// <summary>
		/// Closes the serial port and stops receiving data. Does nothing if no ports are currently open.
		/// </summary>
		public Task ClosePort();

        /// <summary>
        /// Writes the given data to the serial port. Throws an exception if the port isn't open.
        /// </summary>
        /// <param name="data">The bytes to write to the serial port.</param>
        /// <exception cref="InvalidOperationException" />
		/// <exception cref="SerialTransmissionException" />
        public Task WriteData(byte[] data);

		/// <summary>
		/// Event raised when the serial port receives some data.
		/// </summary>
		public event Action<byte[]>? DataReceived;

		/// <summary>
		/// Event raised when the serial port throws a fatal error. If a fatal error is encountered, the serial port is closed automatically.
		/// </summary>
		public event Action? SerialError;

		public bool PortOpen { get; }
	}

	[Serializable]
	internal struct SerialPortDescription
	{
		// These properties must be spelled exactly like this or the serializer will fail to detect them
		public int usbProductId { get; set; }
		public int usbVendorId { get; set; }

		public override string ToString()
		{
			return $"USB Product ID: {usbProductId}, USB Vendor ID: {usbVendorId}";
		}
	}

	internal class SerialPortUnavailableException : Exception 
	{ 
		public SerialPortUnavailableException(string Message) : base(Message)
		{
			
		}
	}

	internal class SerialService : ISerialService {
		private readonly IJSRuntime js;
		private DotNetObjectReference<SerialService>? selfRef;

		public bool PortOpen { get; private set; }

		public SerialService(IJSRuntime js) {
			this.js = js;
		}

		public async Task<bool> OpenPortSelectionDialog(uint baudRate, SerialPortDataBits dataBits, SerialPortFlowControl flowControl, SerialPortParity parity, SerialPortStopBits stopBits) {
			if (PortOpen)
				throw new InvalidOperationException("Cannot open serial port because a serial port is already open.");

			selfRef = DotNetObjectReference.Create(this); // Reference to us so the JS code can call back.
			string flowControlString = flowControl == SerialPortFlowControl.FLOW_CONTROL_NONE ? "none" : "hardware";
			string parityString = parity switch {
				SerialPortParity.PARITY_EVEN => "even",
				SerialPortParity.PARITY_ODD => "odd",
				_ => "none"
			};
			int operationStatus = await js.InvokeAsync<int>("openPortSelectionDialog", selfRef, baudRate, 512, (int)dataBits, flowControlString, parityString, (int)stopBits);

			switch (operationStatus) {
				case 0:
					return false;
				case 2:
					throw new SerialSecurityException("Insufficient permissions for accessing serial ports.");
				case 3:
					throw new InvalidOperationException("Cannot open serial port because a serial port is already open.");
				case 4:
					throw new SerialInitializationException("Failed to open the serial port");
				case 5:
					throw new ArgumentException("One or more serial port parameters (baud rate, data bits, flow control, parity, stop bits or buffer size) are not valid! Please check that they conform to the specification at https://wicg.github.io/serial/.");
				default: // Will be 1
					PortOpen = true;
					return true;
			}
		}

        public async Task<int> GetNumberOfPairedSerialPorts()
		{
			return await js.InvokeAsync<int>("getNumberOfPairedSerialPorts");
		}

        public async Task<IEnumerable<SerialPortDescription>> GetPairedSerialPortsDescriptions()
		{
			string jsonObject = await js.InvokeAsync<string>("getPairedSerialPortsDescriptions");
			var listOfSerialPorts = JsonSerializer.Deserialize<IEnumerable<SerialPortDescription>>(jsonObject);
			if (listOfSerialPorts is null)
				return new SerialPortDescription[0];

			return listOfSerialPorts;
        }

		public async Task<bool> OpenPairedSerialPort(int serialPortIndex, uint baudRate, SerialPortDataBits dataBits, SerialPortFlowControl flowControl, SerialPortParity parity, SerialPortStopBits stopBits)
		{
			if (PortOpen)
				throw new InvalidOperationException("Cannot open serial port because a serial port is already open.");

			selfRef = DotNetObjectReference.Create(this); // Reference to us so the JS code can call back.
			string flowControlString = flowControl == SerialPortFlowControl.FLOW_CONTROL_NONE ? "none" : "hardware";
			string parityString = parity switch
			{
				SerialPortParity.PARITY_EVEN => "even",
				SerialPortParity.PARITY_ODD => "odd",
				_ => "none"
			};
			int operationStatus = await js.InvokeAsync<int>("openPairedSerialPort", serialPortIndex, selfRef, baudRate, 512, (int)dataBits, flowControlString, parityString, (int)stopBits);

			switch (operationStatus)
			{
				case 0:
					return false;
				case 2:
					throw new SerialSecurityException("Insufficient permissions for accessing serial ports.");
				case 3:
					throw new InvalidOperationException("Cannot open serial port because a serial port is already open.");
				case 4:
					throw new SerialInitializationException("Failed to open the serial port");
				case 5:
					throw new ArgumentException("One or more serial port parameters (baud rate, data bits, flow control, parity, stop bits or buffer size) are not valid! Please check that they conform to the specification at https://wicg.github.io/serial/.");
				case 6:
					throw new IndexOutOfRangeException("The provided serial port index is outside the bounds of the paired ports array.");
				default: // Will be 1
					PortOpen = true;
					return true;
			}
		}

		public async Task<bool> OpenPairedSerialPort(SerialPortDescription serialPortDescription, uint baudRate, SerialPortDataBits dataBits, SerialPortFlowControl flowControl, SerialPortParity parity, SerialPortStopBits stopBits)
		{
			if (PortOpen)
				throw new InvalidOperationException("Cannot open serial port because a serial port is already open.");

			selfRef = DotNetObjectReference.Create(this); // Reference to us so the JS code can call back.
			string flowControlString = flowControl == SerialPortFlowControl.FLOW_CONTROL_NONE ? "none" : "hardware";
			string parityString = parity switch
			{
				SerialPortParity.PARITY_EVEN => "even",
				SerialPortParity.PARITY_ODD => "odd",
				_ => "none"
			};
			int operationStatus = await js.InvokeAsync<int>("openPairedSerialPortByDescription", serialPortDescription.usbProductId, serialPortDescription.usbVendorId, selfRef, baudRate, 512, (int)dataBits, flowControlString, parityString, (int)stopBits);

			switch (operationStatus)
			{
				case 0:
					return false;
				case 2:
					throw new SerialSecurityException("Insufficient permissions for accessing serial ports.");
				case 3:
					throw new InvalidOperationException("Cannot open serial port because a serial port is already open.");
				case 4:
					throw new SerialInitializationException("Failed to open the serial port");
				case 5:
					throw new ArgumentException("One or more serial port parameters (baud rate, data bits, flow control, parity, stop bits or buffer size) are not valid! Please check that they conform to the specification at https://wicg.github.io/serial/.");
				case 7:
					throw new SerialPortUnavailableException("The provided serial port could not be found. This can mean the user revoked access or the device is not currently connected.");
				default: // Will be 1
					PortOpen = true;
					return true;
			}
		}


		public async Task ClosePort() {
			if (!PortOpen) return;

			await js.InvokeVoidAsync("closePort");
			selfRef!.Dispose();
			PortOpen = false;
		}

        public async Task WriteData(byte[] data) {
			if (!PortOpen)
				throw new InvalidOperationException("Cannot write to the serial port because no port has been opened.");

			try {
				await js.InvokeVoidAsync("writeData", new object[] { data });
			}
			catch (JSException ex) {
				throw new SerialTransmissionException("Failed to write data to serial port: " + ex.Message);
			}
		}

        public event Action<byte[]>? DataReceived;
		[JSInvokable]
		public void OnDataReceived(byte[] data) {
			DataReceived?.Invoke(data);
		}

		public event Action? SerialError;
		[JSInvokable]
		public void OnSerialError() {
			selfRef!.Dispose();
			PortOpen = false;
			SerialError?.Invoke();
		}
	}
}