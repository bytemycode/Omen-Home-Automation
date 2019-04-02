package ro.bytemycode.omenmasterserverclient;

import android.app.Service;
import android.content.Context;
import android.net.ConnectivityManager;
import android.net.LinkAddress;
import android.net.LinkProperties;
import android.net.Network;
import android.net.NetworkCapabilities;

import java.io.IOException;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingDeque;

import ro.bytemycode.omen.OmenAPI;
import ro.bytemycode.omen.OmenAPIClient;
import ro.bytemycode.omen.OmenSettings;

public class ThreadedSession extends Thread
{
    private Service AppContext = null;
    private OmenAPIClient SessionClient = null;
    private OmenSettings Settings;
    private String MasterServerAddress = null;
    private boolean Stop = false;

    private BlockingQueue<Runnable> CommandQueue = new LinkedBlockingDeque<>();

    public ThreadedSession(OmenSettings settings, Service appContext)
    {
        super("OmenAPI Thread");
        Settings = settings;
        AppContext = appContext;
    }

    public boolean Connect()
    {
        return CommandQueue.offer(new Runnable() {
            @Override
            public void run() {
                if(MasterServerAddress == null)
                {
                    MasterServerAddress = FindMasterServer();
                }

                if(MasterServerAddress != null && SessionClient == null)
                {
                    SessionClient = new OmenAPIClient("ws://" + MasterServerAddress + ":666", Settings);
                    SessionClient.Connect();
                }

            }
        });
    }

    public boolean Disconnect()
    {
        return CommandQueue.offer(new Runnable() {
            @Override
            public void run() {
                if(SessionClient != null)
                {
                    SessionClient.Disconnect();
                    SessionClient = null;
                }
            }
        });
    }

    public boolean Kill()
    {
        return CommandQueue.offer(new Runnable() {
            @Override
            public void run() {
                if(SessionClient != null)
                {
                    SessionClient.Disconnect();
                    SessionClient = null;
                }

                Stop = true;
            }
        });
    }

    private String FindMasterServer()
    {
        ConnectivityManager cm = (ConnectivityManager)AppContext.getSystemService(Context.CONNECTIVITY_SERVICE);
        Network currentNetwork = cm.getActiveNetwork();
        NetworkCapabilities capabilities = cm.getNetworkCapabilities(currentNetwork);

        if( !capabilities.hasTransport(NetworkCapabilities.TRANSPORT_ETHERNET) &&
             !capabilities.hasTransport(NetworkCapabilities.TRANSPORT_WIFI))
        {
            return null;
        }

        LinkProperties linkInfo = cm.getLinkProperties(currentNetwork);
        LinkAddress address = null;

        for (LinkAddress addr : linkInfo.getLinkAddresses())
        {
            if(!addr.getAddress().toString().contains(":"))
            {
                address = addr;
                break;
            }
        }

        if(address == null)
        {
            return null;
        }

        try
        {
            return OmenAPI.FindMasterServer(address.getAddress());
        }
        catch (IOException e)
        {
            e.printStackTrace();
            return null;
        }
    }

    @Override
    public synchronized void start()
    {
        super.start();
        Connect();
    }

    @Override
    public void run()
    {
        super.run();
        while(!Stop)
        {
            try
            {
                Runnable toRun = CommandQueue.take();
                toRun.run();
            }
            catch (InterruptedException e)
            {
                if(SessionClient != null)
                {
                    SessionClient.Disconnect();
                    SessionClient = null;
                }

                return;
            }
        }
    }
}

