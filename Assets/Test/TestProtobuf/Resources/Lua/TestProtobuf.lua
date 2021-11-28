
local pb = require "pb"

print("load pb: ",pb)

--local protoc = require "protobuf/protoc"
print("load protoc: ",protoc)

--print("protoc compile proto")
--print( CS.UnityEngine.Resources.Load("Proto/Person2",typeof(CS.UnityEngine.TextAsset)).bytes)
local pbdata
--pbdata= protoc.new():compile(CS.UnityEngine.Resources.Load("Proto/Person2",typeof(CS.UnityEngine.TextAsset)).bytes)
pbdata= CS.UnityEngine.Resources.Load("Lua/Proto/Proto.pb",typeof(CS.UnityEngine.TextAsset)).bytes
print('load pb')
pb.load(pbdata)

local data={ 
    userName="xxx",
    userPwd="yyyyy"
}
--包名用 . 分隔
print('encode')
local chunk2, _ = pb.encode("Example.LoginRequest", data)
print('decode')
local data2 = pb.decode("Example.LoginRequest", chunk2)
print(data2.userName,data2.userPwd)
--protoc:load( )



-- V3
--pbdata= protoc.new():compile(CS.UnityEngine.Resources.Load("Proto/Person3",typeof(CS.UnityEngine.TextAsset)).bytes)
--pb.load(pbdata)

  data={
    errorCode=1,
    accessToken="xxxx"
}
--包名用 . 分隔
  chunk2, _ = pb.encode("Example.LoginResponse", data)
  data2 = pb.decode("Example.LoginResponse", chunk2)
print(data2.errorCode,data2.accessToken)
 