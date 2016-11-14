// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace MMA8452Q
{
	struct Acceleration
	{
		public double X;
		public double Y;
		public double Z;
	};

	// App that reads data over I2C from a MMA8452Q, 3-Axis Digital Accelerometer
	public sealed partial class MainPage : Page
	{
		private const byte ACCEL_I2C_ADDR = 0x1C;		// I2C address of the MMA8452Q
		private const byte ACCEL_REG_CONFIG = 0x0E;		// Data Format register
		private const byte ACCEL_REG_CONTROL1 = 0x2A;		// Control 1 register
		private const byte ACCEL_REG_STAT = 0x00;		// Status data register
		private const byte ACCEL_REG_X = 0x01;			// X Axis MSB data register
		private const byte ACCEL_REG_Y = 0x03;			// Y Axis MSB data register
		private const byte ACCEL_REG_Z = 0x05;			// Z Axis MSB data register

		private I2cDevice I2CAccel;
		private Timer periodicTimer;

		public MainPage()
		{
			this.InitializeComponent();

			// Register for the unloaded event so we can clean up upon exit
			Unloaded += MainPage_Unloaded;

			// Initialize the I2C bus, 3-Axis Digital Accelerometer, and timer
			InitI2CAccel();
		}

		private async void InitI2CAccel()
		{
			string aqs = I2cDevice.GetDeviceSelector();		// Get a selector string that will return all I2C controllers on the system
			var dis = await DeviceInformation.FindAllAsync(aqs);	// Find the I2C bus controller device with our selector string
			if (dis.Count == 0)
			{
				Text_Status.Text = "No I2C controllers were found on the system";
				return;
			}

			var settings = new I2cConnectionSettings(ACCEL_I2C_ADDR);
			settings.BusSpeed = I2cBusSpeed.FastMode;
			I2CAccel = await I2cDevice.FromIdAsync(dis[0].Id, settings);	// Create an I2C Device with our selected bus controller and I2C settings
			if (I2CAccel == null)
			{
				Text_Status.Text = string.Format(
					"Slave address {0} on I2C Controller {1} is currently in use by " +
					"another application. Please ensure that no other applications are using I2C.",
				settings.SlaveAddress,
				dis[0].Id);
				return;
			}

			/*
				Initialize the 3-Axis Digital Accelerometer
				For this device, we create 2-byte write buffers
				The first byte is the register address we want to write to
				The second byte is the contents that we want to write to the register
			*/
			byte[] WriteBuf_StandCtrl1 = new byte[] { ACCEL_REG_CONTROL1, 0x00 };		// 0x00 sets SLEEP Mode Rate to 50 Hz, ODR to 800 Hz, Normal full dynamic range mode, Normal Read mode, Standby Mode
			byte[] WriteBuf_ActiveCtrl1 = new byte[] { ACCEL_REG_CONTROL1, 0x01 };		// 0x01 sets SLEEP Mode Rate to 50 Hz, ODR to 800 Hz, Normal full dynamic range mode, Normal Read mode, Active Mode
			byte[] WriteBuf_Config = new byte[] { ACCEL_REG_CONFIG, 0x00 };			// 0x00 disables High-Pass output data, sets Full Scale Range to 2g

			// Write the register settings
			try
			{
				I2CAccel.Write(WriteBuf_StandCtrl1);
				I2CAccel.Write(WriteBuf_ActiveCtrl1);
				I2CAccel.Write(WriteBuf_Config);
			}
			// If the write fails display the error and stop running
			catch (Exception ex)
			{
				Text_Status.Text = "Failed to communicate with device: " + ex.Message;
				return;
			}

			// Create a timer to read data every 500ms
			periodicTimer = new Timer(this.TimerCallback, null, 0, 500);
		}

		private void MainPage_Unloaded(object sender, object args)
		{
			// Cleanup
			I2CAccel.Dispose();
		}

		private void TimerCallback(object state)
		{
			string xText, yText, zText;
			string addressText, statusText;

			// Read and format 3-Axis Digital Accelerometer data
			try
			{
				Acceleration accel = ReadI2CAccel();
				addressText = "I2C Address of the 3-Axis Digital Accelerometer MMA8452Q: 0x1C";
				xText = String.Format("X Axis: {0:F0}", accel.X);
				yText = String.Format("Y Axis: {0:F0}", accel.Y);
				zText = String.Format("Z Axis: {0:F0}", accel.Z);
				statusText = "Status: Running";
			}
			catch (Exception ex)
			{
				xText = "X Axis: Error";
				yText = "Y Axis: Error";
				zText = "Z Axis: Error";
				statusText = "Failed to read from 3-Axis Digital Accelerometer: " + ex.Message;
			}

			// UI updates must be invoked on the UI thread
			var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				Text_X_Axis.Text = xText;
				Text_Y_Axis.Text = yText;
				Text_Z_Axis.Text = zText;
				Text_Status.Text = statusText;
			});
		}

		private Acceleration ReadI2CAccel()
		{
			byte[] RegAddrBuf = new byte[] { ACCEL_REG_STAT };	// Read data from the register address
			byte[] ReadBuf = new byte[7];				// We read 7 bytes sequentially to get status and all 3 two-byte axes registers in one read

			/*
				Read from the 3-Axis Digital Accelerometer 
				We call WriteRead() so we first write the address of the Status I2C register, then read all 3 axes
			*/
			I2CAccel.WriteRead(RegAddrBuf, ReadBuf);
			
			/*
				In order to get the raw 12-bit data values, we need to concatenate two 8-bit bytes from the I2C read for each axis
			*/
			int AccelRawX = (int)((ReadBuf[1] & 0xFF) * 256);
			AccelRawX |= (int)(ReadBuf[2] & 0xFF);
			AccelRawX = AccelRawX / 16;
			if (AccelRawX > 2047)
			{
				AccelRawX = AccelRawX -4096;
			}
			int AccelRawY = (int)((ReadBuf[3] & 0xFF) * 256);
			AccelRawY |= (int)(ReadBuf[4] & 0xFF);
			AccelRawY = AccelRawY / 16;
			if (AccelRawY > 2047)
			{
				AccelRawY = AccelRawY -4096;
			}
			int AccelRawZ = (int)((ReadBuf[5] & 0xFF) * 256);
			AccelRawZ |= (int)(ReadBuf[6] & 0xFF);
			AccelRawZ = AccelRawZ / 16;
			if (AccelRawZ > 2047)
			{
				AccelRawZ = AccelRawZ -4096;
			}

			Acceleration accel;
			accel.X = AccelRawX;
			accel.Y = AccelRawY;
			accel.Z = AccelRawZ;

			return accel;
		}
	}
}
