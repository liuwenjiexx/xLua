if not breakSocketHandle then
    breakSocketHandle,debugXpCall = require('LuaDebug')('localhost',7003)
end

local pb = require "pb"

print("load pb: ",pb)

local protoc = require "protobuf/protoc"
print("load protoc: ",protoc)

print("protoc compile Person2.proto")

print( CS.UnityEngine.Resources.Load("Proto/Person2",typeof(CS.UnityEngine.TextAsset)).bytes)
local pbdata= protoc.new():compile(CS.UnityEngine.Resources.Load("Proto/Person2",typeof(CS.UnityEngine.TextAsset)).bytes)
pb.load(pbdata)

local data={
    id=1,
    name="xxx",
    address="xxx city"
}
--包名用 . 分隔
local chunk2, _ = pb.encode("Example.Person2", data)
local data2 = pb.decode("Example.Person2", chunk2)
print(data2.id,data2.name,data2.address)
--protoc:load( )



-- V3
pbdata= protoc.new():compile(CS.UnityEngine.Resources.Load("Proto/Person3",typeof(CS.UnityEngine.TextAsset)).bytes)
pb.load(pbdata)

  data={
    id=2,
    name="xxx 3",
    address="xxx city 3"
}
--包名用 . 分隔
  chunk2, _ = pb.encode("Example.Person2", data)
  data2 = pb.decode("Example.Person2", chunk2)
print(data2.id,data2.name,data2.address)
 