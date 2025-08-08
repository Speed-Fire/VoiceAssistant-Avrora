-- KEYS[1] - pending queue
-- KEYS[2] - processing queue
-- KEYS[3] - timestamps hash
--
-- ARGV[1] - task id

local task = redis.call('LMOVE', KEYS[1], KEYS[2], 'RIGHT', 'LEFT', 0)
if not task then
	return nil
end

local t = redis.call('TIME')
local time = tonumber(t[1])

redis.call('ZADD', KEYS[3], time, task)
return task