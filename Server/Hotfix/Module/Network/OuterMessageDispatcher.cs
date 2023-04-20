﻿using System;
using Model;

namespace Hotfix
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
		public async void Dispatch(Session session, PacketInfo packetInfo)
		{
			Type messageType = Game.Scene.GetComponent<OpcodeTypeComponent>().GetType(packetInfo.Header.Opcode);
			IMessage message = (IMessage)session.network.MessagePacker.DeserializeFrom(messageType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);

			// gate session收到actor消息直接转发给actor自己去处理
			if (message is IActorMessage)
			{
				long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
				ActorProxy actorProxy = Game.Scene.GetComponent<ActorProxyComponent>().Get(unitId);
				actorProxy.Send(message);
				return;
			}

			// gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端
			if (message is IActorRequest aActorRequest)
			{
				long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
				ActorProxy actorProxy = Game.Scene.GetComponent<ActorProxyComponent>().Get(unitId);
				IResponse response = await actorProxy.Call(aActorRequest);
				session.Reply(packetInfo.Header.RpcId, response);
				return;
			}

			if (message != null)
			{
				Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, packetInfo.Header.RpcId, message);
				return;
			}

			throw new Exception($"message type error: {message.GetType().FullName}");
		}
	}
}
