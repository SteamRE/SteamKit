package steamkit.util;

import steamkit.steam3.SteamLanguage.EMsg;

public class MsgUtil
{
    private static final int ProtoMask = 0x80000000;
    private static final int EMsgMask = ~ProtoMask;

    public static EMsg GetMsg(int integer)
    {
        return EMsg.lookup(integer & EMsgMask);
    }
    
    public static EMsg GetMsg(EMsg msg)
    {
        return GetMsg(msg.getCode());
    }

    public static Boolean IsProtoBuf(int integer)
    {
        return (integer & ProtoMask) > 0;
    }

    public static int MakeMsg(EMsg msg)
    {
        return msg.getCode();
    }

    public static int MakeMsg(EMsg msg, Boolean protobuf)
    {
        return MakeMsg(msg.getCode(), protobuf);
    }
    
    public static int MakeMsg(int code, Boolean protobuf)
    {
        if (protobuf)
            return code | ProtoMask;

        return code;
    }
}