package net.steam3.packets;

import java.nio.ByteBuffer;

import com.google.protobuf.InvalidProtocolBufferException;

import steamkit.steam3.SteamLanguage.ISteamSerializableMessage;
import steamkit.steam3.SteamLanguage.MsgHdrProtoBuf;

public class ClientMsgProtobuf<X extends ISteamSerializableMessage> extends ClientMsg<X, MsgHdrProtoBuf>
{
	public ClientMsgProtobuf(Class<X> msgClass) {
		super(msgClass, MsgHdrProtoBuf.class);
	}
	
	public ClientMsgProtobuf(Class<X> msgClass, ByteBuffer input) throws InvalidProtocolBufferException {
		super(msgClass, MsgHdrProtoBuf.class, input);
	}
}
