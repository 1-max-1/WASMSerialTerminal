using Microsoft.JSInterop;

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
		public Task<bool> OpenPortSelectionDialog(uint baudRate, SerialPortDataBits dataBits, SerialPortFlowControl flowControl, SerialPortParity parity, SerialPortStopBits stopBits);
		
		/// <summary>
		/// Closes the serial port and stops receiving data. Does nothing if no ports are currently open.
		/// </summary>
		public Task ClosePort();

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

			// Reference to us so the JS code can call back.
			selfRef = DotNetObjectReference.Create(this);
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
				default:
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