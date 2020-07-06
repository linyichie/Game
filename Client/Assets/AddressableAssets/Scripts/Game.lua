require("Scripts.Utils.Class")
require("Scripts.Utils.Table")
require("Scripts.Asset.Assets")
require("Scripts.Event.Event")
require("Scripts.UI.WindowManager")

--local breakSocketHandle, debugXpCall = require("Scripts.LuaDebug")("localhost", 7003)

local Game = {}

function Game:Start()
    WindowManager:Open("Login")
end

return Game
