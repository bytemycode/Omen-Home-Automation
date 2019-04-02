from core.synchronous_service import SynchronousService, Singleton
from services.__globals__ import RECEIVER_ADDR, RECEIVER_PORT, RECEIVER_VOLRANGE
import enum

import socket

class PowerState(enum.Enum):
    On  = 1
    Off = 2

class AVInput(enum.IntEnum):
    Unknown     =   -1,
    CBLSAT      = 0x01,
    DVD         = 0x02,
    BluRay      = 0x03, # PC
    Game        = 0x04, # Switch
    MediaPlayer = 0x05, # MiBox
    AUX         = 0x06,

    @staticmethod
    def getLookup():
        LookupTable = {
            AVInput.CBLSAT:         'SAT/CBL',
            AVInput.DVD:            'DVD',
            AVInput.BluRay:         'BD',
            AVInput.Game:           'GAME',
            AVInput.MediaPlayer:    'MPLAY',
            AVInput.AUX:            'AUX',
        }

        return LookupTable

    @staticmethod
    def valToString(input):
        table = AVInput.getLookup()
        if input in table:
            return table[input]

        return None

    @staticmethod
    def stringToVal(input):
        table = AVInput.getLookup()
        for key, val in table.items():
            if val == input:
                return key
        return None

class AVReceiver(SynchronousService, metaclass=Singleton):

    Socket = None

    def __init__(self):
        super().__init__('AVReceiver')

    def SetPower(self, powerState):
        if powerState is PowerState.On:
            self._SendCommandWithReply_('ZMON')
        elif powerState is PowerState.Off:
            self._SendCommandWithReply_('ZMOFF')

    def GetPower(self):
        response = self._SendCommandWithReply_('ZM?')
        if response == 'ZMON':
            return PowerState.On
        elif response == 'ZMOFF':
            return  PowerState.Off
        else:
            return None

    def SetInput(self, input):
        strInput = AVInput.valToString(AVInput(input))
        if strInput is not None:
            self._SendCommand_('SI' + strInput)

    def GetInput(self):
        response = self._SendCommandWithReply_('SI?')
        if response is not None:
            response = response[2:] # cut SI
            return AVInput.stringToVal(response)
        return  None

    def SetVolume(self, delta):
        currentVol = self.GetVolume()
        if currentVol is not None:
            toSet = int((currentVol + delta) * RECEIVER_VOLRANGE)
            toSet = int(toSet / 10)
            ret = self._SendCommandWithReply_('MV' + str(toSet))
            return toSet * 10 / RECEIVER_VOLRANGE

    def GetVolume(self):
        response = self._SendCommandWithReply_('MV?')
        if response is not None:
            response = response[2:]
            if len(response) == 2:
                response += '0'
            response = int(response)
            return response / RECEIVER_VOLRANGE

    def _SendCommand_(self, msg):
        if self._connect_():
            msg = msg + '\r'
            self.Socket.send(msg.encode('utf-8'))
            self.__disconnect_()

    def _SendCommandWithReply_(self, msg):
        reply = None
        if self._connect_():
            msg = msg + '\r'
            self.Socket.send(msg.encode('utf-8'))
            reply = self.Socket.recv(128)
            self.__disconnect_()

        if reply is None:
            return None
        else:
            return reply.decode('utf-8').strip('\n\r')

    def _connect_(self):
        self.Socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        try:
            self.Socket.connect((RECEIVER_ADDR, RECEIVER_PORT))
            return True
        except:
            self.Socket.close()
            return False

    def __disconnect_(self):
        try:
            self.Socket.close()
        except:
            return

    def _on_start_(self):
        pass

    def _on_stop_(self):
        self.Socket = None