using MongoDB.Bson.Serialization.Attributes;

// �������ڲ���Ϣ Opcode��10000��ʼ

namespace Model
{
	[Message(10001)]
	[BsonIgnoreExtraElements]
	public class R2G_GetLoginKey : ARequest
	{
	}

	[Message(10002)]
	[BsonIgnoreExtraElements]
	public class G2R_GetLoginKey : AResponse
	{
		public long Key;

		public G2R_GetLoginKey(long key)
		{
			this.Key = key;
		}
	}

	[Message(10003)]
	[BsonIgnoreExtraElements]
	public class M2A_Reload : ARequest
	{
	}

	[Message(10004)]
	[BsonIgnoreExtraElements]
	public class A2M_Reload : AResponse
	{
	}
}
