#ifndef BASE_TYPEDEFS_H
#define BASE_TYPEDEFS_H
#include <boost/cstdint.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/thread.hpp>
#include <google/protobuf/service.h>

namespace Egametang {

typedef boost::int8_t   int8;
typedef boost::int16_t  int16;
typedef boost::int32_t  int32;
typedef boost::int64_t  int64;
typedef boost::uint8_t  uint8;
typedef boost::uint16_t uint16;
typedef boost::uint32_t uint32;
typedef boost::uint64_t uint64;

// smart_ptr typedef

typedef boost::shared_ptr<int> IntPtr;
typedef boost::shared_ptr<std::string> StringPtr;

// boost
typedef boost::shared_ptr<boost::thread> ThreadPtr;

// google
typedef boost::shared_ptr<google::protobuf::Service> ProtobufServicePtr;
typedef boost::shared_ptr<google::protobuf::Message> ProtobufMessagePtr;

// Egametang
class RpcSession;
class RpcRequest;
class RpcChannel;
class ThreadPool;
class ThreadPoolIf;
typedef boost::shared_ptr<RpcSession> RpcSessionPtr;
typedef boost::shared_ptr<RpcRequest> RpcRequestPtr;
typedef boost::shared_ptr<RpcChannel> RpcChannelPtr;
typedef boost::shared_ptr<ThreadPool> ThreadPoolPtr;

} // namespace Egametang

#endif // BASE_TYPEDEFS_H
