from core.micro_service import MicroService
from services.__master_server_helpers__ import Arbiter, MasterServerClient

from services.avreceiver_service import AVReceiver, PowerState
from services.projector_service import Projector

import websockets
import asyncio
import json

class MasterServer(MicroService):
    """MasterServer service"""

    ServerArbiter = Arbiter()
    Clients = set()
    ServerEntryPoint = None
    Mappings = {}

    def __init__(self):
        super().__init__('MasterServer')
        self.Mappings['vol']     = self._setVolume_
        self.Mappings['get_vol'] = self._getVolume_
        self.Mappings['focus']   = self._focusCommand_


    def __onfocus__(self, old, new):
        projectorOn = Projector().IsPowered()

        if new is None:
            AVReceiver().SetPower(PowerState.Off)
            if projectorOn:
                Projector().TurnOff()
        elif old is new:
            if new.wants_video_focus() and not projectorOn:
                Projector().TurnOn()
            elif not new.wants_video_focus() and projectorOn:
                Projector().TurnOff()
        else:
            AVReceiver().SetInput(new.get_channel())

            if new.wants_video_focus():
                if not projectorOn:
                    Projector().TurnOn()
            else:
                if projectorOn:
                    Projector().TurnOff()

    async def _setVolume_(self, params, client):
        delta = params['Delta']
        delta = float(delta)

        AVReceiver().SetVolume(delta)

    async def _getVolume_(self, params, client):
        currentVol = AVReceiver().GetVolume()
        message = { 'Type': 'vol_response', 'CurrentVolume': currentVol }
        rez = json.dumps(message)
        await client.send(rez)

    async def _focusCommand_(self, params, client):
        type = int(params['Action'])
        if type == 1:
            client.set_wants_video(True)
            self.ServerArbiter.gain_focus(client)
        elif type == 2:
            client.set_wants_video(False)
            self.ServerArbiter.gain_focus(client)
        elif type == 3:
            self.ServerArbiter.gain_focus(client)

    async def __parser__(self, message, client):
        packet = json.loads(message)
        mnemonic = packet['Mnemonic']
        if mnemonic in self.Mappings:
            await self.Mappings[mnemonic](packet, client)

    async def __handler__(self, websocket, path):
        client = MasterServerClient(websocket)
        await client.__parse__()

        if client.is_valid():
            self.Clients.add(client)
        else:
            # failure => end connection!
            return

        self.log("Connection from {}".format(client))

        #latest client should always gain focus video focus if wanted
        self.ServerArbiter.gain_focus(client)

        try:
            while True:
                message = await websocket.recv()
                await self.__parser__(message, client)
        except websockets.ConnectionClosed:
            pass
        finally:
            self.Clients.remove(client)
            self.log("Closing connection {}".format(client))
            self.ServerArbiter.remove_client(client, self.Clients)

    def _update_(self):
        self.EventLoop.run_until_complete(asyncio.sleep(1))

    def _on_start_(self):
        self.EventLoop = asyncio.new_event_loop()
        asyncio.set_event_loop(self.EventLoop)
        self.ServerArbiter.OnFocusChanged = self.__onfocus__

        self.StopEvent = asyncio.futures.Future()
        coroutine = websockets.serve(self.__handler__, '0.0.0.0', 666)
        self.Server = self.EventLoop.run_until_complete(coroutine)

    def _on_stop_(self):
        self.Server.close()
        asyncio.get_event_loop().run_until_complete(self.Server.wait_closed())