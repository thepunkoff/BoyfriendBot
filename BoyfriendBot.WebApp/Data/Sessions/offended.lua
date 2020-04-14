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
	text_message_category("offended_start")

	if math.random() < 0.2 then
		delay(math.random(2, 5), function()
			text_message("Я на тебя " .. (gender and "обиделся." or "обиделась."))
		end)
	end
end

function update(input)
	if check(input) then
		text_message_category("offended_good")
		end_session()
	else
		text_message_category("offended_bad")
	end
end
