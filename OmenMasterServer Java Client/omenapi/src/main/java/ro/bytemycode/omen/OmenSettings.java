package ro.bytemycode.omen;


public class OmenSettings
{
    public String Name = "Unknown";

    public MasterServerChannel Channel = MasterServerChannel.None;

    public final boolean WantsAudio = true; // Clients will always want audio

    public boolean WantsVideo = false;

    public OmenSettings(String name, MasterServerChannel channel, boolean wantsVideo) {
        Name = name;
        Channel = channel;
        WantsVideo = wantsVideo;
    }
}
