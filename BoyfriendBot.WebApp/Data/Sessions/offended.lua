local function value_contains_any(val, tab)
    for index, value in ipairs(tab) do
        if string.match(val, value) then
            return true
        end
    end

    return false
end

local function check(input)
		local match_list = {"извини", "прости", "извиняюсь"}

		if value_contains_any(input, match_list) then
			return true
		else
			return false
		end
end

function start( ... )
	end_other_sessions()
	text_message("это было грубо")
	delay(2, function()
		text_message("обида...")
	end)
end

function update(input)
	if check(input) then
		text_message("ладно, больше не обижаюсь")
		end_session()
	else
		text_message("я с тобой не разговариваю")
	end
end
