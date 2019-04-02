import socket
import struct
import time
import platform
import random
import socketserver
import sys

from services.__globals__ import *


class SSDPSearchHandler(socketserver.BaseRequestHandler):
    """
       RequestHandler object to deal with DIAL UPnP search requests.

       Note that per the SSD protocol, the server will sleep for up
       to the number of seconds specified in the MX value of the
       search request- this may cause the system to not respond if
       you are not using the multi-thread or forking mixin.
       """

    max_delay = 0

    def setup(self):
        self.max_delay = DELAY_DEFAULT

    def handle(self):
        """
        Reads data from the socket, checks for the correct
        search parameters and UPnP search target, and replies
        with the application URL that the server advertises.
        """
        data = self.request[0]
        data = data.decode('utf_8')
        data = data.strip().split('\r\n')

        if data[0] != UPNP_SEARCH:
            return
        else:
            service_match = False
            for line in data[1:]:
                try:
                    field, val = line.split(':', 1)
                except ValueError:
                    self.server.log('Invalid message from %s %s' % (str(self.client_address), data))
                    sys.stdout.flush()
                    return
                if field.strip() == 'ST' and val.strip() == SSDP_ST:
                    service_match = True
                elif field.strip() == 'MX':
                    try:
                        self.max_delay = int(val.strip())
                    except ValueError:
                        # Use default
                        pass

            if service_match:
                print('[SSDP] M-SEARCH from %s' % (str(self.client_address)))
                sys.stdout.flush()
                self._send_reply()

    def _send_reply(self):
        """Sends reply to SSDP search messages."""
        time.sleep(random.randint(0, self.max_delay))

        _socket = self.request[1]

        timestamp = time.strftime("%A, %d %B %Y %H:%M:%S GMT", time.gmtime())

        reply_data = SSDP_REPLY.format(self.server.location,
                                       self.server.cache_expire, self.server.os_id,
                                       self.server.os_version, self.server.product_id,
                                       self.server.product_version, timestamp, "uuid:" + str(self.server.uuid))

        reply_data = reply_data.encode()

        sent = 0
        while sent < len(reply_data):
            sent += _socket.sendto(reply_data, self.client_address)


class SocketSSDPServer(socketserver.ThreadingUDPServer):
    """
    Inherits from SocketServer.UDPServer to implement the SSDP
    portions of the Omen protocol- listening for search requests
    on port 1900 for messages to the Omen multicast group and
    replying with information on the URL used to request app
    actions from the server.

    Parameters:
         -device_url: Absolute URL of the device being advertised.
         -host: host/IP address to listen on
    """
    def __init__(self, host='0.0.0.0'):
        socketserver.ThreadingUDPServer.__init__(self, (host, SSDP_PORT), SSDPSearchHandler, False)
        self.allow_reuse_address = False

        self.server_activate()
        self.server_bind()

        mreq = struct.pack("=4sl", socket.inet_aton(SSDP_ADDR), socket.INADDR_ANY)
        self.socket.setsockopt(socket.IPPROTO_IP, socket.IP_ADD_MEMBERSHIP, mreq)

        self.product_id = PRODUCT
        self.product_version = VERSION
        self.os_id = platform.system()
        self.os_version = platform.release()
        self.cache_expire = CACHE_DEFAULT
        self.uuid = APP_UUID
        self.location = SocketSSDPServer.get_socket_ip()

    @staticmethod
    def get_socket_ip():
        return socket.gethostbyname(socket.gethostname())

    def update(self):
        self.serve_forever()

    def stop(self):
        self.shutdown()