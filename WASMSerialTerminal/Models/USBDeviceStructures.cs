using System.Collections.Immutable;

namespace WASMSerialTerminal {
	internal class USBDevice {
		public string ID { get; private set; }
		public string Name { get; private set; }

		public USBDevice(string id, string name) {
			ID = id;
			Name = name;
		}
	}

	internal class USBVendor {
		public string ID { get; private set; }
		public string Name { get; private set; }
		public ImmutableArray<USBDevice> Devices { get; private set; }

		public USBVendor(string iD, string name, ImmutableArray<USBDevice> devices) {
			ID = iD;
			Name = name;
			Devices = devices;
		}
	}

	internal class USBVendorList {
		public ImmutableArray<USBVendor> Vendors { get; private set; }
		public string Version { get; private set; }
		public string Date { get; private set; }

		public USBVendorList(ImmutableArray<USBVendor> vendors, string version, string date) {
			Vendors = vendors;
			Version = version;
			Date = date;
		}
	}
}