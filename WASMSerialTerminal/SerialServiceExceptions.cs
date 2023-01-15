namespace WASMSerialTerminal {
	/// <summary>
	/// Exception thrown if a user permission prompt is denied while trying to open a serial port.
	/// </summary>
	public class SerialSecurityException : Exception {
		public SerialSecurityException(string message) : base(message) { }
	}

	/// <summary>
	/// Exception for when the attempt to open the port failed.
	/// </summary>
	public class SerialInitializationException : Exception {
		public SerialInitializationException(string message) : base(message) { }
	}
}
