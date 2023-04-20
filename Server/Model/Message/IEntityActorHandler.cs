namespace Model
{
	public interface IEntityActorHandler
	{
		void Handle(Session session, Entity entity, IActorMessage message);
	}

	/// <summary>
	/// gate session �յ�����Ϣֱ��ת�����ͻ���
	/// </summary>
	public class GateSessionEntityActorHandler : IEntityActorHandler
	{
		public void Handle(Session session, Entity entity, IActorMessage message)
		{
			message.Id = 0;
			((Session)entity).Send((AMessage)message);
		}
	}

	public class CommonEntityActorHandler : IEntityActorHandler
	{
		public void Handle(Session session, Entity entity, IActorMessage message)
		{
			Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, message);
		}
	}
}