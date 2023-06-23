namespace WASMSerialTerminal {
	/// <summary>
	/// The data model for the rows displayed in the data grid. <br/>
	/// Represents one byte of data received from the serial port.
	/// </summary>
	public class SerialByte {
		public byte DecimalValue { get; private set; }
		public string HexValue { get; private set; }
		public char Character { get; private set; }

		/// <summary>
		/// Create a new <see cref="SerialByte"/> object. <br/>
		/// The <paramref name="data"/> parameter is cast to a <see cref="char"/> and converted to a hex string for the other 2 properties.
		/// </summary>
		public SerialByte(byte data) {
			DecimalValue = data;
			HexValue = data.ToString("X");
			Character = (char)data;
		}
	}
}