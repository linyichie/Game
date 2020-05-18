function class(super, fields)
    local superType = type(super)
    if superType ~= "table" then
        super = nil
    end

    local instance

    instance = {}
    if super then
        setmetatable(instance, {__index = super})
        instance.super = super
    end

    instance.__index = instance

    function instance:new(...)
        local instance = setmetatable({}, instance)
        instance.class = instance
        if instance.ctor then
            instance:ctor(...)
        end
        return instance
    end

    if fields then
        for k, v in pairs(fields) do
            instance[k] = v
        end
    end
    return instance
end
