Assets = class(nil, {
    modules = {}
})

function Assets:Require(path)
    if self.modules[path] == nil then
        local m = require(path)
        self.modules[path] = m
    end
    return self.modules[path]
end

return Assets