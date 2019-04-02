# Omen Home Automation

   Simple host server used for in home automation.

# Summary

 - The Server is discoverable using SSDP
 
	Currently SSDP uses these params
	- SSDP_ADDR = "239.255.255.250"
	- SSDP_PORT = 1900
	- SSDP_ST = "upnp:omen-masterserver"
 
 - Services run in a micro service like framework,
   however there isn't an abstraction layer between the services.
  
 - Repository also includes C# and Android TV clients with focus change functionality.
 
