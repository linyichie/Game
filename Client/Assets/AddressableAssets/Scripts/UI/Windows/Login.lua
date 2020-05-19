local Login = class()

function Login:OnInitialize()
    local onSwitchCN = function()
        CS.Localization.Set("CN")
    end
    self.languages[1].onClick:AddListener(onSwitchCN)
    local onSwitchEN = function()
        CS.Localization.Set("EN")
    end
    self.languages[2].onClick:AddListener(onSwitchEN)
    local onSwitchJP = function()
        CS.Localization.Set("JP")
    end
    self.languages[3].onClick:AddListener(onSwitchJP)
    local onSwitchKR = function()
        CS.Localization.Set("KR")
    end
    self.languages[4].onClick:AddListener(onSwitchKR)

    CS.Localization.languageChange = table.handler(self, self.OnLanguageChange)

    WindowController:Close("Launch")
end

function Login:OnReadyOepn()
    self:DisplayLabel();
end

function Login:OnOpened()
end

function Login:OnReadyClose()
end

function Login:OnClosed()
end

function Login:OnLanguageChange()
    self:DisplayLabel()
end

function Login:DisplayLabel()
    self.startLabel.text = CS.Language.Get("Login");
end

return Login
