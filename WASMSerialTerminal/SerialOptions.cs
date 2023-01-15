namespace WASMSerialTerminal {
	public enum SerialPortDataBits {
		SEVEN_DATA_BITS = 7,
		EIGHT_DATA_BITS = 8
	}

	public enum SerialPortFlowControl {
		FLOW_CONTROL_NONE,
		FLOW_CONTROL_HARDWARE
	}

	public enum SerialPortParity {
		PARITY_NONE,
		PARITY_EVEN,
		PARITY_ODD
	}

	public enum SerialPortStopBits {
		ONE_STOP_BIT = 1,
		TWO_STOP_BITS = 2
	}
}