var keepReading = false;
var reader;
var writer;
var closePromise;

async function callOnDataReceived(data, serialService) {
	try {
		await serialService.invokeMethodAsync("OnDataReceived", data);
	}
	catch (ex) {
		console.log("Error calling .NET method OnDataReceived: " + ex.message);
	}
}

async function readUntilClosed(port, serialService) {
	// If a non-fatal error is encountered (i.e. port.readable still true), we just loop again, get a new reader and continue to read
	while (port.readable && port.writable && keepReading) {
		reader = port.readable.getReader();
		writer = port.writable.getWriter();
		try {
			while (true) {
				let { value, done } = await reader.read();
				if (done) break;
				await callOnDataReceived(value, serialService);
			}
		}
		catch (error) {
			console.log("Serial error: " + error.message);
		}
		finally {
			reader.releaseLock();
			writer.releaseLock();
		}
	}

	await port.close();

	// If keepReading is true and we are here, then port.readable must have been false which means the port threw a fatal error
	if (keepReading)
		serialService.invokeMethodAsync("OnSerialError")
}

window.openPortSelectionDialog = async (serialService, baudRate, bufferSize, dataBits, flowControl, parity, stopBits) => {
	try {
		// C# code will detect if navigator.serial is not supported - we want to write as little JS as possible
		let port = await navigator.serial.requestPort();
		await port.open({ baudRate: baudRate, bufferSize: bufferSize, dataBits: dataBits, flowControl: flowControl, parity: parity, stopBits: stopBits });
		keepReading = true;
		closePromise = readUntilClosed(port, serialService);
		return 1;
	}
	catch (ex) {
		if (ex.name == "SecurityError")
			return 2;
		else if (ex.name == "InvalidStateError")
			return 3;
		else if (ex.name == "NetworkError")
			return 4;
		else if (ex.name == "TypeError")
			return 5;
		// Otherwise will be a NotFoundError indicating that the user cancelled the operation
		return 0;
	}
}

window.openPairedSerialPort = async (index, serialService, baudRate, bufferSize, dataBits, flowControl, parity, stopBits) => {
	try {
		// C# code will detect if navigator.serial is not supported - we want to write as little JS as possible
		const pairedPorts = await navigator.serial.getPorts();
		if (index < 0 || index >= pairedPorts.length)
			return 6;

		let port = pairedPorts[index];
		await port.open({ baudRate: baudRate, bufferSize: bufferSize, dataBits: dataBits, flowControl: flowControl, parity: parity, stopBits: stopBits });
		keepReading = true;
		closePromise = readUntilClosed(port, serialService);
		return 1;
	}
	catch (ex) {
		if (ex.name == "SecurityError")
			return 2;
		else if (ex.name == "InvalidStateError")
			return 3;
		else if (ex.name == "NetworkError")
			return 4;
		else if (ex.name == "TypeError")
			return 5;
		// Otherwise will be a NotFoundError indicating that the user cancelled the operation
		return 0;
	}
}
window.openPairedSerialPortByDescription = async (usbProductId, usbVendorId, serialService, baudRate, bufferSize, dataBits, flowControl, parity, stopBits) => {
	try {
		// C# code will detect if navigator.serial is not supported - we want to write as little JS as possible
		const pairedPorts = await navigator.serial.getPorts();
		var port = pairedPorts.find(function (port) {
			let info = port.getInfo();
			return info.usbVendorId = usbVendorId && info.usbProductId == usbProductId;
		});

		if (port === undefined)
			return 7;

		await port.open({ baudRate: baudRate, bufferSize: bufferSize, dataBits: dataBits, flowControl: flowControl, parity: parity, stopBits: stopBits });
		keepReading = true;
		closePromise = readUntilClosed(port, serialService);
		return 1;
	}
	catch (ex) {
		if (ex.name == "SecurityError")
			return 2;
		else if (ex.name == "InvalidStateError")
			return 3;
		else if (ex.name == "NetworkError")
			return 4;
		else if (ex.name == "TypeError")
			return 5;
		// Otherwise will be a NotFoundError indicating that the user cancelled the operation
		return 0;
	}
}



window.getNumberOfPairedSerialPorts = async () => {
	return (await navigator.serial.getPorts()).length;
}

window.getPairedSerialPortsDescriptions = async () => {
	var obj = [];
	const ports = await navigator.serial.getPorts();
	for (i = 0; i < ports.length; i++) {
		obj[i] = ports[i].getInfo();
	}
	return JSON.stringify(obj);
}

window.closePort = async () => {
	keepReading = false;
	reader.cancel(); // Will cause done = true so the while loop will break, and keepreading = false so the function exits
	await closePromise; // Wait for the loop to break
}

window.writeData = async (data) => {
	await writer.write(data);
}