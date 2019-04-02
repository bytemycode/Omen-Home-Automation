package ro.bytemycode.omen;

import java.io.IOException;
import java.net.*;
import java.nio.charset.StandardCharsets;
import java.util.Formatter;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class OmenAPI {

    private static int      SSDP_PORT = 1900;
    private static String   SSDP_GROUP = "239.255.255.250";
    private static String   SSDP_ST = "upnp:omen-masterserver";
    private static int      SSDP_MX = 0;
    private static String   SSDP_REQUEST =  "M-SEARCH * HTTP/1.1\r\n" +
                                            "HOST: %s:%s\r\n" +
                                            "MAN: \"ssdp:discover\"\r\n" +
                                            "ST: %s\r\n" +
                                            "MX: %s\r\n";

    private static String GetRequest()
    {
        StringBuilder sbuf = new StringBuilder();
        Formatter fmt = new Formatter(sbuf);
        fmt.format(SSDP_REQUEST, SSDP_GROUP, SSDP_PORT, SSDP_ST, SSDP_MX);

        return sbuf.toString();
    }

    public static String FindMasterServer() throws IOException
    {
        return FindMasterServer(InetAddress.getLocalHost());
    }

    public static String FindMasterServer(InetAddress localInAddress) throws IOException
    {
        NetworkInterface netIf = NetworkInterface.getByInetAddress(localInAddress);

        // Prepare network
        SocketAddress mSSDPMulticastGroup = new InetSocketAddress(SSDP_GROUP, SSDP_PORT);
        MulticastSocket mSSDPSocket = new MulticastSocket(new InetSocketAddress(localInAddress,0));

        mSSDPSocket.joinGroup(mSSDPMulticastGroup, netIf);

        // Send a discovery request
        String request = GetRequest();
        DatagramPacket dp = new DatagramPacket(request.getBytes(), request.length(), mSSDPMulticastGroup);
        mSSDPSocket.send(dp);

        // Wait for a reply
        byte[] buf = new byte[1024];
        DatagramPacket recvDP = new DatagramPacket(buf, buf.length);
        mSSDPSocket.receive(recvDP);

        // Close socket
        mSSDPSocket.close();

        // Decode and parse reply
        String raw = new String(recvDP.getData(), StandardCharsets.UTF_8);
        Pattern p = Pattern.compile("^LOCATION:\\s*(\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3})$", Pattern.MULTILINE);

        Matcher matcher = p.matcher(raw);
        if (matcher.find())
        {
            // Successfully parsed
            return matcher.group(1);
        }

        return null;
    }
}
