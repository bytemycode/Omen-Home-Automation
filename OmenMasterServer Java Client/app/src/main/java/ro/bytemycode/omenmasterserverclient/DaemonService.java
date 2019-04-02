package ro.bytemycode.omenmasterserverclient;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Build;
import android.os.IBinder;
import android.support.v4.app.NotificationCompat;

import ro.bytemycode.omen.MasterServerChannel;
import ro.bytemycode.omen.OmenSettings;

public class DaemonService extends Service
{
    OmenSettings Settings = new OmenSettings("MiBox", MasterServerChannel.MediaPlayer, true);
    ThreadedSession Session;
    BroadcastReceiver Receiver;

    public class ScreenReceiver extends BroadcastReceiver
    {
        @Override
        public void onReceive(Context context, Intent intent)
        {
            if (intent.getAction().equals(Intent.ACTION_SCREEN_OFF))
            {
                DaemonService.this.Session.Disconnect();
            }
            else if (intent.getAction().equals(Intent.ACTION_SCREEN_ON))
            {
                DaemonService.this.Session.Connect();
            }
        }
    }

    public DaemonService()
    {
        Session = new ThreadedSession(Settings, this);
    }

    @Override
    public void onCreate()
    {
        super.onCreate();

        Session.start();

        IntentFilter filter = new IntentFilter();
        filter.addAction(Intent.ACTION_SCREEN_OFF);
        filter.addAction(Intent.ACTION_SCREEN_ON);
        Receiver = new ScreenReceiver();
        registerReceiver(Receiver, filter);

        Intent notificationIntent = new Intent(this, Launcher.class);

        PendingIntent pendingIntent = PendingIntent.getActivity(this, 0,
                notificationIntent, 0);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O)
        {
                NotificationChannel androidChannel = new NotificationChannel("OmenID",
                        "Omen Notificaiont", NotificationManager.IMPORTANCE_DEFAULT);

                NotificationManager notificationManager = getSystemService(NotificationManager.class);
                notificationManager.createNotificationChannel(androidChannel);
        }

        Notification notification = new NotificationCompat.Builder(this, "OmenID")
                .setSmallIcon(R.mipmap.ic_launcher)
                .setContentTitle("Omen Watchdog")
                .setContentText("Running")
                .setContentIntent(pendingIntent).build();

        startForeground(1337, notification);
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        return START_STICKY;
    }

    @Override
    public void onDestroy()
    {
        super.onDestroy();
        Session.Kill();
        unregisterReceiver(Receiver);
    }

    @Override
    public IBinder onBind(Intent intent)
    {
        throw new UnsupportedOperationException("Not yet implemented");
    }
}
