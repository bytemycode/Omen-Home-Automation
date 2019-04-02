

class DaemonExecutor(object):
    """A bootstrap manager for MicroService apps"""

    MicroServiceClassList = []
    MicroServices = []
    IsRunning = False

    """ Constructor, optional classList args to restrict bootstrap scope """
    def __init__(self, class_list = None):
        assert class_list is not None
        assert hasattr(class_list, '__iter__')

        self.MicroServiceClassList = class_list

    def start(self):
        assert not self.IsRunning
        for cls in self.MicroServiceClassList:
            service = cls()
            service.start()
            self.MicroServices.append(service)

    def wait(self):
        for service in self.MicroServices:
            service.join()

    def stop(self):
        for service in self.MicroServices:
            service.stop()