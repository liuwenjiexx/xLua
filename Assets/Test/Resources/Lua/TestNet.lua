print("hello TestNet") 
NetMgr = require 'Net/NetMgr'

Proto = require("protobuf/LuaProtoBuf")

local protoc = require "protobuf/protoc"
local pbdata= protoc.new():compile(CS.UnityEngine.Resources.Load("Lua/Proto/Proto", typeof(CS.UnityEngine.TextAsset)).bytes)

Proto:initialize({
    bytes = pbdata,
    CSCmd = require("Proto/ProtoCSCmd"),
    SCCmd = require("Proto/ProtoSCCmd")
})

NetMgr:initialize(Proto)
NetMgr:addNetStateHandler(function(state) 
    if state==1 then
        print("connected") 
    end
end, true)
NetMgr:addListener("ErrorInfo",function(data) 
    print("Recive An Error Message ".. data.error_code) 
end)

NetMgr:connect("192.168.0.2", "8081")

---目前callback为使用messageId对应，相同messageId认为为callback，如果你有其他字段可用于确定业务回调，如Order，则需要继承BaseNetChannelHelper来使用
NetMgr:sendMessage("ActorLevelUpRequest",{ actor_guid == "" ,level_to_up = 1 },function(data) end )