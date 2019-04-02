from threading import Thread

import sys


class MicroService(Thread):
    """Simple interface modelling a microservice"""

    IsRunning = False
    Started = False

    def __init__(self, name):
        super().__init__(None, None, name)

    def _update_(self):
        pass

    def _on_start_(self):
        pass

    def _on_stop_(self):
        pass

    def start(self):
        self.IsRunning = True
        super().start()
        self.log('Started')

    def run(self):
        if not self.Started:
            self.Started = True
            self._on_start_()

        while self.IsRunning:
            self._update_()

    def stop(self):
        if self.IsRunning:
            self.IsRunning = False
            self._on_stop_()
            self.log('Stopped')

    def log(self, msg):
        print("[%s] %s" % (self.getName(), msg))
        sys.stdout.flush()
