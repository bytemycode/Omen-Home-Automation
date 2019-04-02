package ro.bytemycode.omenmasterserverclient;

import android.content.Intent;
import android.os.Bundle;
import android.support.v4.app.FragmentActivity;

/*
 * MainActivity class that loads {@link MainFragment}.
 */
public class Launcher extends FragmentActivity {

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_launcher);

        // Start service if not already started
        Intent startIntent = new Intent(this, DaemonService.class);
        startService(startIntent);


    }
}
