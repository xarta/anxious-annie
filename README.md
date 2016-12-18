# anxious-annie
.Net C# intention to be cross-platform for 3G Dongle SMS (Raspberry Pi Jessie/Mono, and Windows 10 Compute Stick) - part of larger IoT plans

Specific Aims
- run on Raspberry Pi 2 (Wheezy) and Pi 3 (Jessie)
- run on Windows 10 desktop including Compute Sticks
- tie in with broader IoT project (including MQTT-cross-platform, ASP.Net RESTful endpoints,  
  and local 868MHz encrypted radio network/Arduino/Sensor/Actuator bridge) including possibly 
  other serial ports using this code
- provide means for an IoT back-up channel (if internet connectivity is lost)

  ... loading assembly for Windows Management Query - entities for example, to find COM ports with specific devices,
  ... or look at UDev on the Pi to find specific device file paths
  
- provide a common interface for bespoke implementation serial-port classes, suitable for dependancy injection:
  * WriteAT           e.g. GSM, or other serial port for IoT device comms
  * ReceiveAT
  * SendData
  * ReceiveData       e.g. ASCII SMS for short IoT messages only
  * EventHandler      e.g. received SMS, error-sending-SMS etc.
  
- eventually make robust e.g. gracefully deal with lost/intermittent resources e.g. USB device removed, lost signals etc.

- provide a preshared-key-generated-token for SMS messages e.g. intention to make a simple Xamarin Android app initially to test secure SMS
  * e.g. think Google Authenticator - app generates the code and adds to SMS message sent
  * requires common time-reference either end (Raspberry Pi doesn't have a Real Time Clock - I'm thinking about it - might add one)
  * will have test actuator code e.g. relay on/off, on the Pi 


This is my first .Net project, trying to put into practice what I've been teaching myself in recent months, so development might be a little slow and jerky with plenty of design mistakes/corrections along the way. Very iterative. Better use of threading/Async/Task etc. and attributes as I become more familiar in those topics.  Edges of this sub-project might overlap with larger project as I feel my way.

Planning to blog at https://xarta.co.uk  including installing Debian later release of Mono on Raspbian-Jessie, and hardware used.
Contact:            dave@xarta.co.uk
