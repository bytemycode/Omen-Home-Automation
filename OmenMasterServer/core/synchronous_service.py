import sys


class Singleton(type):
    _instances = {}
    def __call__(cls, *args, **kwargs):
        if cls not in cls._instances:
            cls._instances[cls] = super(Singleton, cls).__call__(*args, **kwargs)
        return cls._instances[cls]


class SynchronousService(object):
    """Simple interface modelling of a synchronous microservice"""

    IsRunning = False
    Started = False
    Name = None

    def __init__(self, name):
        self.Name = name

    def _on_start_(self):
        pass

    def _on_stop_(self):
        pass

    def start(self):
        self.IsRunning = True
        self.log('Started')
        self.Started = True
        self._on_start_()

    def stop(self):
        if self.IsRunning:
            self.log('Stopped')
            self.IsRunning = False
            self._on_stop_()

    def join(self):
        pass

    def getName(self):
        return self.Name

    def __str__(self):
        return self.getName()

    def log(self, msg):
        print("[%s] %s" % (self.getName(), msg))
        sys.stdout.flush()
