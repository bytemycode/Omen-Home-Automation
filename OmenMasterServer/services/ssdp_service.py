from core.micro_service import MicroService
from services.__ssdp_server__ import SocketSSDPServer


class SSDP(MicroService):
    """SSDP server"""

    Server = None

    def __init__(self):
        super().__init__('SSDP')

    def _update_(self):
        self.Server.update()

    def _on_start_(self):
        self.Server = SocketSSDPServer()

    def _on_stop_(self):
        self.Server.stop()