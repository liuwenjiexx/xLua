local BaseNetChannelHandler = require 'Net/BaseNetChannelHandler'

local NetMgr = {}

function NetMgr:initialize( proto )
    BaseNetChannelHandler:ctor("mainChannel",proto)
    self.channelHandler = BaseNetChannelHandler
end

function NetMgr:connect(ip , port)
    self.channelHandler.IP = ip;
    self.channelHandler.Port = port;
    self.channelHandler:StartUp()
end

function NetMgr:request(msg,data,timeout)
    local resTimeout = false
    if timeout~=nil then
        resTimeout = true
    end
    timeout = timeout or 3
    local promise = Promise.new()
    local time = Timer.New(function()
        print("Error","is timeout dont receive callback: "..msg)
        if resTimeout == true then
            promise:reject("timeout")
        end
    end,timeout)
    self:sendMessage(msg,data,function(res)
        print("NetMgr","request: callback:"..msg)
        time:Stop()
        promise:resolve(res)
    end)
    return promise
end

function NetMgr:receive(msg,timeout)
    local resTimeout = false
    if timeout~=nil then
        resTimeout = true
    end
    timeout = timeout or 3
    local promise = Promise.new()
    local time = Timer.New(function()
        print("Error","is timeout dont receive callback: "..msg)
        if resTimeout == true then
            promise:reject("timeout")
        end
    end,timeout)
    self:addListener(msg,function(data)
        time:Stop()
        promise:resolve(data)
    end,true)
    time:Start()
    return promise
end

function NetMgr:sendMessage(msg,data,receiveCallback)
    self.channelHandler:sendMessage(msg,data,receiveCallback)
end

function NetMgr:addNetStateHandler(callback,rm)
    self.channelHandler:addNetStateHandler(callback,rm)
end

function NetMgr:removeNetStateHandler( callback )
    self.channelHandler:removeNetStateHandler(callback)
end

function NetMgr:addListener(msg,callback,isSingle)
    self.channelHandler:addListener(msg,callback,isSingle)
end

function NetMgr:removeListener( msg,callback )
    self.channelHandler:removeListener(msg,callback)
end


return NetMgr