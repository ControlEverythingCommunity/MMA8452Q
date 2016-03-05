// Distributed with a free-will license.
// Use it any way you want, profit or free, provided it fits in the licenses of its associated works.
// MMA8452Q
// This code is designed to work with the MMA8452Q_I2CS I2C Mini Module available from ControlEverything.com.
// https://www.controleverything.com/content/Accelorometer?sku=MMA8452Q_I2CS#tabs-0-product_tabset-2

import com.pi4j.io.i2c.I2CBus;
import com.pi4j.io.i2c.I2CDevice;
import com.pi4j.io.i2c.I2CFactory;
import java.io.IOException;

public class MMA8452Q
{
	public static void main(String args[]) throws Exception
	{
		// Create I2C bus
		I2CBus bus = I2CFactory.getInstance(I2CBus.BUS_1);
		// Get I2C device, MMA8452Q I2C address is 0x1C(28)
		I2CDevice device = bus.getDevice(0x1C);
		
		// Send standby command
		device.write(0x2A, (byte)0x00);
		// Send active command
		device.write(0x2A, (byte)0x01);
		// Set Range upto +/-2g
		device.write(0x0E, (byte)0x00);
		Thread.sleep(500);
		
		// Read 7 bytes of data from address 0x00(0)
		// Status, X msb, X lsb, Y msb, Y lsb, Z msb, Z lsb
		byte[] data = new byte[7];
		device.read(0x00, data, 0, 7); 
		
		// Convert the values
		int xAccl = (((data[1] & 0xFF) * 256) + (data[2] & 0xFF)) / 16;
		if (xAccl > 2047)
		{
			xAccl = xAccl - 4096;
		}
		
		int yAccl = (((data[3] & 0xFF) * 256) + (data[4] & 0xFF)) / 16;
		if (yAccl > 2047)
		{
			yAccl = yAccl - 4096;
		}
		
		int zAccl = (((data[5] & 0xFF) * 256) + (data[6] & 0xFF)) / 16;
		if (zAccl > 2047)
		{
			zAccl = zAccl - 4096;
		}
		// Output data to screen
		System.out.printf("X-Axis : %d %n", xAccl);
		System.out.printf("Y-Axis : %d %n", yAccl);
		System.out.printf("Z-Axis : %d %n", zAccl);
	}
}
