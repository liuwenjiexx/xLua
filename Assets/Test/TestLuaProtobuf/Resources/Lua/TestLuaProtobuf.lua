Proto = require("protobuf/LuaProtoBuf")
local pbdata

print('Proto:initialize')
local proto=require("Proto/Proto")
Proto:initialize({
    bytes = CS.UnityEngine.Resources.Load("Lua/Proto/Proto.pb", typeof(CS.UnityEngine.TextAsset)).bytes,
    CSCmd=proto.cs,
    SCCmd=proto.sc,
    --CSCmd = require("Proto/ProtoCSCmd"),
    --SCCmd = require("Proto/ProtoSCCmd")
})


local bytes, msgId = Proto:encode("cs", "LoginRequest", { userName = "xxx", userPwd = "yyyyy" })
print('Proto:encode',msgId,bytes)

local pb=require("pb")
local bytes2, _ = pb.encode("Example.LoginResponse",  { errorCode = 1, accessToken = "yyyyy" })
local data= Proto:decode("sc", 10002, bytes2)
print('Proto:decode',data.errorCode,data.accessToken)