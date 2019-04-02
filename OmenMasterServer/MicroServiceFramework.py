import signal
from core.daemon_executor import DaemonExecutor
from services.ssdp_service import SSDP
from services.masterserver_service import MasterServer
from services.avreceiver_service import AVReceiver
from services.projector_service import Projector

currentExecutor = None

def main():
    global currentExecutor

    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)

    services = [SSDP, MasterServer, AVReceiver, Projector]
    currentExecutor = DaemonExecutor(services)
    currentExecutor.start()
    currentExecutor.wait()

def signal_handler(sgn, frame):
    print("")
    currentExecutor.stop()
    print("Daemon shutting down")


if __name__ == "__main__":
    main()