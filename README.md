[![MMA8452Q](MMA8452Q_I2C.png)](https://www.controleverything.com/content/Accelorometer?sku=MMA8452Q_I2CS)
# MMA8452Q
MMA8452Q 3-Axis Digital Accelerometer
 
The MMA8452Q device, provides digital X, Y, Z-Axis Accelerometer.

This Device is available from ControlEverything.com [SKU: MMA8452Q_I2CS]

https://www.controleverything.com/content/Accelorometer?sku=MMA8452Q_I2CS

This Sample code can be used with Raspberry pi.

##Java 
Download and install pi4j library on Raspberry pi. Steps to install pi4j are provided at:

http://pi4j.com/install.html

Download (or git pull) the code in pi.

Compile the java program.
```cpp
$> pi4j MMA8452Q.java
```

Run the java program as.
```cpp
$> pi4j MMA8452Q
```

##Python 
Download and install smbus library on Raspberry pi. Steps to install smbus are provided at:

https://pypi.python.org/pypi/smbus-cffi/0.5.1

Download (or git pull) the code in pi. Run the program

```cpp
$> python MMA8452Q.py
```

The code output is raw value of acceleration in X, Y and Z Axis.

