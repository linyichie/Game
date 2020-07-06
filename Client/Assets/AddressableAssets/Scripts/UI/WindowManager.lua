var "WindowManager"

function WindowManager:Open(windowName)
    local onOpenWindow = function(window)
        self:OnOpenWindow(windowName, window)
    end
    CS.WindowManager.Instance:OpenWindow(windowName, onOpenWindow)
end

function WindowManager:OnOpenWindow(windowName, window)
    local classPath = "Scripts.UI.Windows." .. windowName
    local instance = Assets:Require(classPath):new()
    window:LuaBind(instance)
end

function WindowManager:Close(windowName)
    CS.WindowManager.Instance:CloseWindow(windowName)
end