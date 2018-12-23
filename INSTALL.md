# Install hints

## External dependencies

* 6x12 raster font
* HidSharp.dll

#### Font

In my device I use [Dangen_charset_6x12.png](http://uzebox.org/wiki/File:Dangen_charset_6x12.png).
Since I don't know the licence type I can't upload it here.
If you want to use another font you need to rewrite the code.

#### HidSharp

App uses [HidSharp lib](https://www.zer7.com/software/hidsharp) to read key presses from remote control.
HidSharp.dll itself can run on both x86 and ARM CPUs.

## Autostart

Start on OS boot can be done with rc.local service. You can also create your own systemd service.
This is how I made it inside /etc/rc.local
```bash
pushd /radio_app/
./radio_appd.sh &
popd
```
Keep in mind that ```pushd```/```popd``` are __Bash__ commands. You can try to use ```cd``` here but I'm not sure if it's the right way.

## Hardware configuration

This app uses:
1) SPI
2) GPIO
3) Analog audio

#### SPI

Run ```armbian-config``` and activate SPI. Then add following lines to __/boot/armbianEnv.txt__
```
param_spidev_spi_bus=1
param_spidev_spi_cs=0
param_spidev_max_freq=10000000
```
Reboot to apply changes.

#### GPIO

GPIO is configured by app itself.

#### Analog audio

Run ```armbian-config``` and activate analog audio. Or you can use any other audio output device (USB, Bluetooth, etc.)

## Install folder

I use ```/radio_app/``` but you can use different one. Don't forget to modify scripts.
File list:
* Dangen_charset_6x12.png
* HidSharp.dll
* bootlogo.gif
* config.xml
* radio_app.exe
* radio_appd.sh
* time_font.png

## Configuration

App uses ```config.xml``` file. Sample file is provided inside repository. Add your streams inside it.
You can run ```ffplay``` in ssh session to test URLs. Look inside __Player.cs__ for arguments.
