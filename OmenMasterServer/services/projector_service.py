from core.synchronous_service import Singleton
from core.micro_service import MicroService

import serial
from threading import Timer

class Projector(MicroService, metaclass=Singleton):

    PendingCommand = None
    IsBusy = False

    def __init__(self):
        super().__init__('Projector')

    def _reset_(self):
        self.log('Cooldown done')
        self.IsBusy = False
        if self.PendingCommand is not None:
            self.PendingCommand()
            self.PendingCommand = None

    def _trigger_(self):
        self.IsBusy = True
        self.log('Entered 30s cooldown')
        self.T = Timer(30.0, self._reset_)
        self.T.start()

    def TurnOn(self):
        if self.IsBusy:
            self.PendingCommand = self.TurnOn
            return True

        reply = self._SendCommand_('~0000 1')
        self.log('TurnOn ' + str(reply))

        self._trigger_()

        return reply

    def TurnOff(self):
        if self.IsBusy:
            self.PendingCommand = self.TurnOff
            return True

        reply = self._SendCommand_('~0000 0')
        self.log('TurnOff ' + str(reply))

        self._trigger_()

        return reply

    def IsPowered(self):
        reply = self._SendCommandWithReply_('~00124 1')
        power = reply == 'OK1'
        self.log('IsPowered ' + str(reply))
        return power

    def _SendCommand_(self, msg):
        try:
            ser = serial.Serial('/dev/ttyUSB0', 9600, timeout=1, bytesize=serial.EIGHTBITS, parity=serial.PARITY_NONE,
                                stopbits=serial.STOPBITS_ONE)
            msg = msg + '\r'
            ser.write(msg.encode('utf-8'))
            reply = ser.read(2)

            ser.close()

            if reply is None:
                return False
            else:
                return reply.decode('utf-8').strip('\n\r') == 'P'
        except:
            return False

    def _SendCommandWithReply_(self, msg, reply_len = 4):
        try:
            ser = serial.Serial('/dev/ttyUSB0', 9600, timeout=1, bytesize=serial.EIGHTBITS, parity=serial.PARITY_NONE,
                                stopbits=serial.STOPBITS_ONE)
            msg = msg + '\r'
            ser.write(msg.encode('utf-8'))
            reply = ser.read(reply_len)

            ser.close()

            return reply.decode('utf-8').strip('\n\r')
        except:
            return None

    def _on_start_(self):
        pass

    def _on_stop_(self):
        pass