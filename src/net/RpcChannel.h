#ifndef NET_RPC_CHANNEL_H
#define NET_RPC_CHANNEL_H

#include <google/protobuf/service.h>
#include <boost/unordered_map.hpp>
#include <boost/asio.hpp>
#include "Base/Base.h"

namespace Hainan {

class RpcHandler;

class RpcChannel: public google::protobuf::RpcChannel
{
private:
	typedef boost::unordered_map<int32, RpcHandlerPtr> RpcCallbackMap;

	int32 id;
	RpcCallbackMap handlers;
	boost::asio::io_service& io_service;
	boost::asio::ip::tcp::socket socket;

	void AsyncConnectHandler(const boost::system::error_code& err);

	// recieve response
	void RecvMessegeSize();
	void RecvMessage(IntPtr size, const boost::system::error_code& err);
	void RecvMessageHandler(StringPtr ss, const boost::system::error_code& err);

	// send request
	void SendMessageSize(const RpcRequestPtr request, RpcHandlerPtr handler);
	void SendMessage(const RpcRequestPtr request,
			RpcHandlerPtr handler, const boost::system::error_code& err);
	void SendMessageHandler(int32 id, RpcHandlerPtr handler,
			const boost::system::error_code& err);

public:
	RpcChannel(boost::asio::io_service& service, std::string& host, int port);
	~RpcChannel();
	virtual void CallMethod(
			const google::protobuf::MethodDescriptor* method,
			google::protobuf::RpcController* controller,
			const google::protobuf::Message* request,
			google::protobuf::Message* response,
			google::protobuf::Closure* done);
};

} // namespace Hainan

#endif // NET_RPC_CHANNEL_H
