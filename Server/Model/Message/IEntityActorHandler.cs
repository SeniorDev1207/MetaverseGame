using System.Threading.Tasks;

namespace Model
{
	public interface IEntityActorHandler
	{
		Task<bool> Handle(Session session, Entity entity, ActorRequest message);
	}

	/// <summary>
	/// gate session �յ�����Ϣֱ��ת�����ͻ���
	/// </summary>
	public class GateSessionEntityActorHandler : IEntityActorHandler
	{
		public async Task<bool> Handle(Session session, Entity entity, ActorRequest message)
		{
			((Session)entity).Send(message.AMessage);
			return true;
		}
	}

	public class CommonEntityActorHandler : IEntityActorHandler
	{
		public async Task<bool> Handle(Session session, Entity entity, ActorRequest message)
		{
			return await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, message);
		}
	}
}