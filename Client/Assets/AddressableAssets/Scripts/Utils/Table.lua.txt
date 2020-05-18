rawset(
    _G,
    "var",
    function(...)
        local vars = {...}
        local len = #vars
        local key
        for i = 1, len do
            key = vars[i]
            if type(key) ~= "string" then
                error("#" .. i .. ": " .. tostring(key) .. " is not string.")
            end
            rawset(_G, key, {})
        end
    end
)

function table.handler(t, func)
    return function(...)
        return func(t, ...)
    end
end
