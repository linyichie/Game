local Login = class(nil, {
})

function Login:OnInitialize()
    local onRefresh = function()
        self:Refresh()
    end
    self.btnRefresh.onClick:AddListener(onRefresh)
end

function Login:OnReadyOepn()
    self:DisplayLabel()
end

function Login:OnOpened()
    WindowManager:Close("Launch")
end

function Login:OnReadyClose()
end

function Login:OnClosed()
end

function Login:OnLanguageChange()
    self:DisplayLabel()
end

function Login:DisplayLabel()
    self.startLabel.text = CS.LanguageConfig.Get("Login").text;
end

function Login:Refresh()
    local config = CS.ExampleConfig.Get(1)
    print(config.floatValue)
    print(config.label)
    print(config.position)
    print(config.boolValue)
    for i = 0, config.intValues.Length - 1 do
        print(config.intValues[i])
    end
    for i = 0, config.floatValues.Length - 1 do
        print(config.floatValues[i])
    end
    for i = 0, config.labels.Length - 1 do
        print(config.labels[i])
    end
    for i = 0, config.positions.Length - 1 do
        print(config.positions[i])
    end
end

return Login
