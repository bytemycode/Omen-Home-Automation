package ro.bytemycode.omen;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import okhttp3.*;

import java.util.LinkedList;
import java.util.Queue;
import java.util.concurrent.TimeUnit;

public class OmenAPIClient extends WebSocketListener {

    private class FutureResponse
    {
        public String Message;
    }

    private String EndpointURI;
    private OmenSettings Settings;
    private Gson gson;

    private OkHttpClient WSClient = null;
    private WebSocket Session = null;
    private Queue<FutureResponse> waitingQueue = new LinkedList<>();


    public OmenAPIClient(String endpointURI, OmenSettings settings)
    {
        EndpointURI = endpointURI;
        Settings = settings;

        // creation of gson builder
        GsonBuilder gsonBuilder = new GsonBuilder();

        // registration of type hierarchy adapter
        gsonBuilder.registerTypeHierarchyAdapter(MasterServerChannel.class, new ObjectTypeSerializer());
        gsonBuilder.registerTypeHierarchyAdapter(DataObjects.FocusAction.class, new DataObjects.FocusActionSerializer());

        gson = gsonBuilder.create();
    }

    public boolean Connect()
    {
        WSClient = new OkHttpClient.Builder()
                .readTimeout(0,  TimeUnit.MILLISECONDS)
                .build();

        Request request = new Request.Builder()
                .url(EndpointURI)
                .build();

        WSClient.newWebSocket(request, this);

        synchronized (this)
        {
            try {
                this.wait(1000);
            } catch (InterruptedException e) {
                return false;
            }
        }

        return true;
    }

    public boolean Disconnect()
    {
        if(Session == null)
        {
            return false;
        }

        Session.close(1000, null);

        // Trigger shutdown of the dispatcher's executor so this process can exit cleanly.
        WSClient.dispatcher().executorService().shutdown();

        WSClient = null;
        Session = null;

        return true;
    }

    public void ActivateVideo()
    {
        DataObjects.FocusCommand command = new DataObjects.FocusCommand
        (
                DataObjects.FocusAction.ActivateVideo
        );

        SendMessage(command);
    }

    public void DeactivateVideo()
    {
        DataObjects.FocusCommand command = new DataObjects.FocusCommand
        (
            DataObjects.FocusAction.DeactivateVideo
        );

        SendMessage(command);
    }

    public void ForceFocus()
    {
        ForceFocus(MasterServerChannel.None);
    }

    public void ForceFocus(MasterServerChannel newChannel)
    {
        DataObjects.FocusCommand command = new DataObjects.FocusCommand
        (
                DataObjects.FocusAction.ForceFocus
        );

        if(newChannel != MasterServerChannel.None)
        {
            command.NewChannel = newChannel;
        }

        SendMessage(command);
    }

    public float GetVolume()
    {
        DataObjects.GetVolumeCommand command = new DataObjects.GetVolumeCommand();
        DataObjects.VolumeCommandResponse response = SendMessageWithReply(command, DataObjects.VolumeCommandResponse.class);

        return response.CurrentVolume;
    }

    public void SetVolume(float delta)
    {
        if(delta != 0.0f)
        {
            DataObjects.VolumeCommand command = new DataObjects.VolumeCommand(delta);
            SendMessage(command);
        }
    }

    private <T> void  SendMessage(T object)
    {
        String json = gson.toJson(object);
        Session.send(json);
    }

    private <T, Y> Y SendMessageWithReply(T message, Class<Y> classOfY)
    {
        FutureResponse response = new FutureResponse();
        waitingQueue.add(response);

        synchronized (response)
        {
            SendMessage(message);

            try
            {
                response.wait();
            }
            catch (InterruptedException e)
            {
                e.printStackTrace();
                return null;
            }
        }

        return gson.fromJson(response.Message, classOfY);
    }

    @Override
    public void onOpen(WebSocket webSocket, Response response)
    {
        Session = webSocket;

        // Send our hello
        SendMessage(Settings);

        // Open for business
        synchronized(this)
        {
            this.notifyAll();;
        }
    }

    @Override
    public void onMessage(WebSocket webSocket, String text)
    {
        if(!waitingQueue.isEmpty())
        {
            FutureResponse response = waitingQueue.remove();
            synchronized (response)
            {
                response.Message = text;
                response.notifyAll();;
            }
        }
    }

    @Override
    public void onClosing(WebSocket webSocket, int code, String reason)
    {
        webSocket.close(1000, null);
    }

    @Override
    public void onFailure(WebSocket webSocket, Throwable t, Response response)
    {
        t.printStackTrace();
    }
}
