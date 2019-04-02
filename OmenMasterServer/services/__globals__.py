# This apps uuid
APP_UUID = 'c8268c78-f362-11e7-bd48-f828198a0f8e'

# SSDP Multicast config
SSDP_ADDR = "239.255.255.250"
SSDP_PORT = 1900
# This is used during SSDP discovery- servers will wait up to this
# many seconds before replying to prevent flooding
SSDP_MX = 3
# The server search target specified by the protocol
SSDP_ST = "upnp:omen-masterserver"

UPNP_SEARCH = 'M-SEARCH * HTTP/1.1'
# If we get a M-SEARCH with no or invalid MX value, wait up
# to this many seconds before responding to prevent flooding
CACHE_DEFAULT = 1800
DELAY_DEFAULT = 5
PRODUCT = 'RPi'
VERSION = '1.0'

SSDP_REPLY = 'HTTP/1.1 200 OK\r\n' + \
               'LOCATION: {}\r\n' + \
               'CACHE-CONTROL: max-age={}\r\n' + \
               'EXT:\r\n' + \
               'BOOTID.UPNP.ORG: 1\r\n' + \
               'SERVER: {}/{} UPnP/1.1 {}/{}\r\n' + \
               'ST: {}\r\n'.format(SSDP_ST) + \
               'DATE: {}\r\n' + \
               'USN: {}' + \
               '::{}\r\n'.format(SSDP_ST) + '\r\n'

RECEIVER_ADDR       = '192.168.1.15' # Static IP - hardcode for now
RECEIVER_PORT       = 23
RECEIVER_VOLRANGE   = 980