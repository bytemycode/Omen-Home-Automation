import asyncio
import json
import threading

import sys

HEADER_TIMEOUT = 10

class MasterServerClient:
    """An object representing a client of the masterserver"""

    _Websocket = None
    _Name = "Unknown"
    _WantsVideoFocus = False
    _WantsAudioFocus = False
    _Channel = -1

    def __init__(self, websocket):
        self._Websocket = websocket

    async def send(self, message):
        if self._Websocket is not None:
            # should this be synchronous?
            await self._Websocket.send(message)

    def set_wants_video(self, val):
        self._WantsVideoFocus= val

    def wants_video_focus(self):
        return self._WantsVideoFocus

    def wants_audio_focus(self):
        return self._WantsVideoFocus or self._WantsAudioFocus

    def get_channel(self):
        if self._Channel >= 0:
            return self._Channel
        return None

    def is_valid(self):
        return (self._Channel >= 0) and (self._Name is not "Unknown")

    def name(self):
        return self._Name

    def __str__(self):
        return "{} [a:{}][v:{}][c:{}]".format(self._Name, self._WantsAudioFocus, self._WantsVideoFocus, self._Channel)

    async def __parse__(self):
        try:
            message = await asyncio.wait_for(self._Websocket.recv(), HEADER_TIMEOUT)
        except asyncio.TimeoutError:
            print("[MasterServerClient] timeout on header from {}".format(self._Websocket.remote_address))
            return

        header = json.loads(message)

        self._Name = header['Name']
        self._Channel = header['Channel']
        self._WantsVideoFocus = header['WantsVideo']
        self._WantsAudioFocus = header['WantsAudio']

class Arbiter:
    """System Arbiter"""

    Focus = None
    OnFocusChanged = None
    Lock = threading.RLock()

    def __init__(self, on_focus_changed = None):
        self.OnFocusChanged = on_focus_changed

    def gain_focus(self, client):
        self.Lock.acquire()
        if self.Focus is not client:
            self.__on_focus_changed__(self.Focus, client)
            self.Focus = client
        else:
            self.__force_focus__(client)
        self.Lock.release()

    def remove_client(self, client, clients):
        self.Lock.acquire()

        # check if the focused client disconnected
        if self.Focus is client:

            replacement = None
            had_video_focus = client.wants_video_focus()

            # if we had a video request try to get another client which wants video
            if had_video_focus:
                for cl in clients:
                    if cl.wants_video_focus():
                        replacement = cl
                        break

            # get any replacement client if the first search failed
            if replacement is None:
                try:
                    replacement = next(iter(clients))
                except StopIteration:
                    replacement = None

            self.__on_focus_changed__(self.Focus, replacement)
            self.Focus = replacement
        self.Lock.release()

    def  __force_focus__(self, focusObj):
        if self.OnFocusChanged is not None:
                self.OnFocusChanged(focusObj, focusObj)

    def __on_focus_changed__(self, old, new):
        if new is not None:
            if old is not None:
                print("[Arbiter] %s lost focus to %s" % (old.name(), new.name()))
            else:
                print("[Arbiter] %s gained focus" % (new.name()))
        else:
            if old is not None:
                print("[Arbiter] %s lost focus" % (old.name()))
            else:
                print("[Arbiter] No focus")

        sys.stdout.flush()

        if self.OnFocusChanged is not None:
                self.OnFocusChanged(old, new)