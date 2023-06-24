namespace WASMSerialTerminal {
	internal class USBDevice {
		public required string ID { get; set; }
		public required string Name { get; set; }
	}

	internal class USBVendor {
		public required string ID { get; set; }
		public required string Name { get; set; }
		public required List<USBDevice> Devices { get; set; }
	}

	internal class USBVendorList {
		public required List<USBVendor> Vendors { get; set; }
		public required string Version { get; set; }
		public required string Date { get; set; }
	}
}