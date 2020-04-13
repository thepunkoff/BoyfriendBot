local function value_contains_any(val, tab)
    for index, value in ipairs(tab) do
        if string.match(val, value) then
            return true
        end
    end

    return false
end

local function check(input, step)

	if step == 0 then
		
		local match_list = {"давай", "погнали", "ок", "го", "согласен", "согласна", "хорошо", "ладно", "поехали", "не против", "можно"}

		if value_contains_any(input, match_list) then
			return true
		else
			return false
		end
	else
		return "[check] no such step: " .. step;
	end
end

local step
local expireTime

-- add parameters for dynamic expire_time
local function update_expire_time()
	local now = os.date("%Y-%m-%d %H:%M:%S")
	local y, mon, d, h, Min, s = now:match("(%d+)-(%d+)-(%d+) (%d+):(%d+):(%d+)")

    expireTime = os.time({year = y, month = mon, day = d, hour = h, min = Min + 5, sec = s})
end

function start( ... )

	local bundle = {"хей!", 4, "го болтать"}

	step = 0

	update_expire_time()

	return bundle
end

function update(input)

	if os.difftime(os.time(), expireTime) > 0 then
		return {nil}
	end

	if step == 0 then

		if check(input, step) then

			local bundle = {"Еее) Как делв?)", 1, "дела*"}

			step = step + 1
			update_expire_time()

			return bundle
		else
			return false;
		end
	elseif step == 1 then

		local bundle = {"все понятно! Удачи!", nil}

		return bundle

	else
		return "[update] no such step: " .. step;
	end	
end