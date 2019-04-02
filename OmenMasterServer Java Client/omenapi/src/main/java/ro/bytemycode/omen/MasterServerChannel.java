package ro.bytemycode.omen;

import com.google.gson.JsonElement;
import com.google.gson.JsonParser;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;

import java.lang.reflect.Type;

public enum MasterServerChannel
{
    None        (  -1),
    CBLSAT      (0x01),
    DVD         (0x02),
    BluRay      (0x03), // PC
    Game        (0x04), // Switch
    MediaPlayer (0x05), // MiBox
    AUX         (0x06);

    /**
     * Value for this channel
     */
    public final int Value;

    MasterServerChannel(int value)
    {
        Value = value;
    }
}

class ObjectTypeSerializer implements JsonSerializer<MasterServerChannel>
{
    private static final JsonParser mParser = new JsonParser();

    public JsonElement serialize(MasterServerChannel object_,
                                 Type type_,
                                 JsonSerializationContext context_)
    {
        // that will convert enum object to its ordinal value and convert it to json element
        return mParser.parse(((Integer)object_.Value).toString());
    }
}
