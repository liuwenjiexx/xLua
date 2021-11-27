local BaseNetChannelHandler = {
    Get = {},
    Set = {}
}


local UMTNetwork = CS.Yoozoo.Managers.NetworkV2.UMTNetwork

local ENetMgrType = {
    connectSuccess = 1,
    connectFailure = 2
}

local Listener = {
    callbacks = {}
}

function Listener:AddEventListener(nEventType, eventCallBack)
    self.callbacks[nEventType] = eventCallBack
end

function Listener:RemoveEventListener(nEventType, eventCallBack, thisObj)
    table.remove(self.callbacks, nEventType)
end
function Listener:DispatchEvent(nEventType, ...)
    local cb = self.callbacks[nEventType]
    if cb then
        cb(...)
    end
end

function Listener:RemoveAllListeners()
    self.callbacks = {}
end



---ctor
---@param channelName string
---@param protoHelper Proto | table
function BaseNetChannelHandler:ctor(...)
    --BaseNetChannelHandler.super.ctor(self)
    self.channelName, self.protoHelper = ...

    self.channel = UMTNetwork.GetChannel(self.channelName)

    self._callBacks = {}
    self._connectStates = {}
    self._listeners = Listener
    self:registerListener()
end


---IP 访问器方法， 是否可以关闭选择
function BaseNetChannelHandler.Get:IP()
    return self.channel.IP
end

---IP 访问器方法， 是否可以关闭选择
---@param value string
function BaseNetChannelHandler.Set:IP(value)
    self.channel.IP = value
end

---Port 访问器方法， 是否可以关闭选择
function BaseNetChannelHandler.Get:Port()
    return self.channel.Port
end

---Port 访问器方法， 是否可以关闭选择
---@param value number
function BaseNetChannelHandler.Set:Port(value)
    self.channel.Port = value
end

function handler(obj,method)
    return function(...)
        return method(obj,...)
    end
end

function BaseNetChannelHandler:registerListener()
    self.channel:OnNetworkChannelConnected("+", handler(self, self.OnNetworkChannelConnected))
    self.channel:OnNetworkChannelReconnected("+", handler(self, self.OnNetworkChannelReconnected))
    self.channel:OnNetworkChannelClosed("+", handler(self, self.OnNetworkChannelClosed))
    self.channel:OnNetworkChannelError("+", handler(self, self.OnNetworkChannelError))
    self.channel:OnNetworkChannelReceivePacket("+", handler(self, self.OnNetworkChannelReceivePacket))
end

function BaseNetChannelHandler:StartUp()
    self.channel:StartUp()
end

function BaseNetChannelHandler:ShutDown()
    self.channel:ShutDown()
end


function BaseNetChannelHandler:sendMessage(msg, data, receiveCallback)
    local bytes, msgId = self.protoHelper:encode("cs", msg, data)
    self.channel:SendBytes(msgId, bytes)
    if receiveCallback ~= nil then
        if self._callBacks[msgId] == nil then
            self._callBacks[msgId] = {}
        end
        table.insert(self._callBacks[msgId], receiveCallback)
    end
end



function BaseNetChannelHandler:getMsgId(msg)
    local msgId = nil
    if type(msg) == "string" then
        msgId = self.protoHelper:getSCCmd(msg)
        if msgId == nil then
            error("proto dont has this msg: " .. msg)
        end
    elseif type(msg) == "number" then
        msgId = msg
    else
        error("proto dont has this msg: " .. msg)
    end
    return msgId
end

function BaseNetChannelHandler:addListener(msg, callback, isSingle)
    local msgId = self:getMsgId(msg)
    if msgId == nil then return end
    self._listeners:AddEventListener(tostring(msgId), callback)
end

function BaseNetChannelHandler:removeListener(msg, callback)
    local msgId = self:getMsgId(msg)
    if msgId == nil then return end
    self._listeners:RemoveEventListener(tostring(msgId))

end

function BaseNetChannelHandler:removeAllListener(msg)
    if msg ~= nil then
        local msgId = self:getMsgId(msg)
        if msgId == nil then return end
        self._listeners:RemoveEventListener(tostring(msgId))
    else
        self._listeners:RemoveAllListeners()
        print(" all proto listeners removed")
    end
end


function BaseNetChannelHandler:addNetStateHandler(callback, rm)
    if rm == nil then
        rm = false
    end
    if callback then
        table.insert(self._connectStates, 1, {
            callback = callback,
            rm = rm
        });
    end
end

function BaseNetChannelHandler:removeNetStateHandler(callback)
    for i = 1, #self._connectStates, 1 do
        if self._connectStates[i].callback == callback then
            table.remove(self._connectStates, i)
        end
    end
end

function BaseNetChannelHandler:OnNetworkChannelConnected()
    if #self._connectStates < 1 then
        return
    end

    for i, v in pairs(self._connectStates) do
        self._connectStates[i].callback(ENetMgrType.connectSuccess)
        if self._connectStates[i].rm then
            self._connectStates[i] = nil
        end
    end
end

function BaseNetChannelHandler:OnNetworkChannelClosed()

    if #self._connectStates < 1 then
        return
    end
    for i, v in pairs(self._connectStates) do
        self._connectStates[i].callback(ENetMgrType.connectFailure)
        if self._connectStates[i].rm then
            self._connectStates[i] = nil
        end
    end

end

function BaseNetChannelHandler:OnNetworkChannelReconnected()

end

function BaseNetChannelHandler:OnNetworkChannelError()

end


function BaseNetChannelHandler:OnNetworkChannelReceivePacket(channel, packet)
    local rawData=packet.rawData or  packet.message

    if rawData == nil then return end

    local msgId = packet.Id
    local receivedData = self.protoHelper:decode("sc", msgId, rawData)

    local callbacksArray = self._callBacks[msgId];
    if callbacksArray ~= nil then
        for i, callback in ipairs(callbacksArray) do
            callback(receivedData)
        end
        self._callBacks[msgId] = nil
    end
    self._listeners:DispatchEvent(tostring(msgId), receivedData)
end


return BaseNetChannelHandler