var "WindowController"

function WindowController:Open(windowName)
    local onOpenWindow = function(window)
        self:OnOpenWindow(windowName, window)
    end
    CS.WindowController.Instance:OpenWindow(windowName, onOpenWindow)
end

function WindowController:OnOpenWindow(windowName, window)
    local classPath = "Scripts.UI.Windows." .. windowName
    local instance = Assets:Require(classPath):new()
    window:LuaBind(instance)
end

function WindowController:Close(windowName)
    CS.WindowController.Instance:CloseWindow(windowName)
end