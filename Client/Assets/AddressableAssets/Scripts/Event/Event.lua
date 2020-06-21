var "Event"

Event = class(nil, {
    listeners = nil
})

local instanceId = 0
local instanceIdsPool = {}

local function GetInstanceId()
    if #instanceIdsPool > 0 then
        local id = instanceIdsPool[#instanceIdsPool]
        table.remove(instanceIdsPool)
        return id
    end
    instanceId = instanceId + 1
    return instanceId
end

local function ReleaseInstanceId(id)
    table.insert(instanceIdsPool, id)
end

function Event:ctor()
    self.listeners = {}
end

function Event:AddListener(listener)
    local id = GetInstanceId()
    self.listeners[id] = listener
    return id
end

function Event:RemoveListener(id)
    if self.listeners[id] then
        self.listeners[id] = nil
        ReleaseInstanceId(id)
    end
end

function Event:RemoveAllListeners()
    for k, v in pairs(self.listeners) do
        ReleaseInstanceId(k)
    end
    self.listeners = {}
end

function Event:Invoke(...)
    local ids = {}
    for k, v in pairs(self.listeners) do
        table.insert(ids, k)
    end
    local length = #ids
    for i = 1, length do
        local id = ids[i]
        local listener = self.listeners[id]
        if listener then
            listener(...)
        end
    end
end