Proto = require("protobuf/LuaProtoBuf")

local protoc = require "protobuf/protoc"
local pbdata= protoc.new():compile(CS.UnityEngine.Resources.Load("Lua/Proto/Proto", typeof(CS.UnityEngine.TextAsset)).bytes)

Proto:initialize({
    bytes = pbdata,
    CSCmd = require("Proto/ProtoCSCmd"),
    SCCmd = require("Proto/ProtoSCCmd")
})

print('Proto:initialize')
print(require("Proto/ProtoCSCmd"))
local bytes, msgId = Proto:encode("cs", "LoginRequest", { userName = "xxx", userPwd = "yyy" })
print('encode',bytes)

local data= Proto:decode("cs", "LoginRequest", bytes)
print('data',data.userName,data.userPwd)