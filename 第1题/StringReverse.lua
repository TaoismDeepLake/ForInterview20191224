--language: Lua
local input = "Welcome to JoyCastle. Let's make an awesome game together.";
local output = "";

local function Reverse(str)
    local result = "";
    for sentence in string.gmatch(str, "[%a][%a%s']+[,%.]") do 
        --print(sentence);
        local punc = string.match(sentence, "[,%.]");
        local newStr = nil;
        for word in string.gmatch(sentence, "[%a']+") do 
            if newStr == nil then
                newStr = word;
            else
                newStr = word .. ' ' .. newStr;
            end
        end

        if (punc ~= nil) then
            newStr = newStr .. string.match(sentence, "[,%.]");
        end
        result = result .. newStr;
    end
    return result;
end

print(Reverse("Welcome to JoyCastle. Let's make an awesome game together."));
print(Reverse("Focused, hard work is the real key to success. Keep your eyes on the goal, and just keep taking the next step towards completing it."));