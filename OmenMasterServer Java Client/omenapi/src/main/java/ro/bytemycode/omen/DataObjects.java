package ro.bytemycode.omen;

import com.google.gson.JsonElement;
import com.google.gson.JsonParser;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;

import java.lang.reflect.Type;

public class DataObjects
{
    public enum FocusAction
    {
        ActivateVideo   (0x01),
        DeactivateVideo (0x02),
        ForceFocus      (0x03);

        /**
         * FocusAction type
         */
        public final int Value;

        FocusAction(int value)
        {
            Value = value;
        }
    }

    static class FocusActionSerializer implements JsonSerializer<FocusAction>
    {
        private final JsonParser mParser = new JsonParser();

        public JsonElement serialize(FocusAction object_,
                                     Type type_,
                                     JsonSerializationContext context_)
        {
            // that will convert enum object to its ordinal value and convert it to json element
            return mParser.parse(((Integer)object_.Value).toString());
        }
    }


    static abstract class IOmenCommand
    {
        private final String Mnemonic;

        public IOmenCommand(String mnemonic)
        {
            Mnemonic = mnemonic;
        }
    }

    static class VolumeCommand extends IOmenCommand
    {

        public float Delta;

        public VolumeCommand()
        {
            super("vol");
        }

        public VolumeCommand(float delta) {
            this();
            Delta = delta;
        }
    }

    static class GetVolumeCommand extends IOmenCommand
    {
        public GetVolumeCommand()
        {
            super("get_vol");
        }
    }

    static class VolumeCommandResponse
    {
        public final String Type;

        public final float CurrentVolume;

        public VolumeCommandResponse(String type, float currentVolume)
        {
            Type = type;
            CurrentVolume = currentVolume;
        }
    }

    static class FocusCommand extends IOmenCommand
    {
        public FocusAction Action;

        public MasterServerChannel NewChannel = MasterServerChannel.None;

        public FocusCommand()
        {
            super("focus");
        }

        public FocusCommand(FocusAction action)
        {
            this();
            Action = action;
        }

        public FocusCommand(FocusAction action, MasterServerChannel newChannel)
        {
            this(action);
            NewChannel = newChannel;
        }
    }

}
