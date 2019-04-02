package ro.bytemycode.omenmasterserverclient;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

public class Bootstrap extends BroadcastReceiver
{
    public void onReceive(Context context, Intent arg1)
    {
        Intent intent = new Intent(context, DaemonService.class);
        context.startService(intent);
    }
}
