# radio_pi_0
Device for playing Internet radio stations.

## Hardware
#### Orange Pi Zero
[Orange Pi Zero](http://www.orangepi.org/orangepizero/) or [Orange Pi Zero Plus](http://www.orangepi.org/OrangePiZeroPlus/) board.  H2+ and 256MB RAM should be enough but it's up to you which board to choose.

R1 and Plus2 have no USB A connector. You need to modify interconnection PCB to use these boards.

#### SPI OLED
[diymore.cc 2.42" 128x64 SPI OLED](https://www.diymore.cc/products/2-42-inch-12864-oled-display-module-iic-i2c-spi-serial-for-arduino-c51-stm32-green-white-blue-yellow).
Widely available on Aliexpress.

There is also similar "blue PCB" OLED. I didn't tested it but PCB is different from my black one. Be careful.

#### Remote control
I use MX3 Air Mouse (no backlight, no voice key). And any other RC won't work! If you want to use another one you have to rewrite the code.

#### Interconnection PCB
It's 2 layer PCB. Making this board at home will be pretty hard, so I use JLCPCB service.

All sockets/headers are 2.54mm. Audio connector is Dragon City ST-033.

## Software
Main app is written in C#. It uses ffplay to play audio and HidSharp to get keys from RC.
App was tested on Armbian (based on Ubuntu 18.04).

#### Controls
* Play/pause - starts or stops playing
* Pg+/Pg- - changes station
* Vol+/Vol-/Mute - you know...
* Power key shuts system down in some strange way. NEVER USE IT! Disconnect power source if you did. In future releases I want to make it Reset key.
