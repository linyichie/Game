require("Scripts.Utils.Class")
require("Scripts.Utils.Table")
require("Scripts.Asset.Assets")
require("Scripts.UI.WindowController")

--local breakSocketHandle, debugXpCall = require("Scripts.LuaDebug")("localhost", 7003)

local Game = {}

function Game:Start()
    WindowController:Open("Login")
end

return Game
