using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace NetHookAnalyzer2
{
	class ProtoBufFieldReader
	{
		public static Dictionary<int, List<object>> ReadProtobuf(Stream stream)
		{
			// try reading it as a protobuf
			var reader = ProtoReader.State.Create( stream, null );
			var fields = new Dictionary<int, List<object>>();

			while (true)
			{
				int field = reader.ReadFieldHeader();

				if (field == 0)
					break;

				object fieldValue = null;

				switch (reader.WireType)
				{
					case WireType.Varint:
					case WireType.Fixed32:
					case WireType.Fixed64:
					case WireType.SignedVarint:
						{
							try
							{
								fieldValue = reader.ReadInt64();
							}
							catch (Exception)
							{
								fieldValue = "Unable to read Variant (debugme)";
							}

							break;
						}
					case WireType.String:
						{
							try
							{
								fieldValue = reader.ReadString();
							}
							catch (Exception)
							{
								fieldValue = "Unable to read String (debugme)";
							}

							break;
						}
					default:
						{
							fieldValue = string.Format("{0} is not implemented", reader.WireType);
							break;
						}
				}

				if (fields.TryGetValue(field, out var values))
				{
					values.Add( fieldValue );
				}
				else
				{
					values = new List<object> { fieldValue };
					fields[ field ] = values;
				}
			}

			if (fields.Count > 0)
				return fields;

			return null;
		}
	}
}
