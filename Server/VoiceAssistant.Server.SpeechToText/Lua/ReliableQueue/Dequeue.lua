-- KEYS[1] - pending queue
-- KEYS[2] - processing queue
-- KEYS[3] - timestamps set
-- KEYS[4] - tasks status hash
--
-- ARGV[1] - required status to return task instead of removing

local task_id = redis.call('LMOVE', KEYS[1], KEYS[2], 'RIGHT', 'LEFT', 0)
if not task_id then
	return nil
end

local task_status = redis.call('HGET', KEYS[4], task_id)
if not task_status or task_status ~= tonumber(ARGV[1]) then
	redis.call('LREM', KEYS[2], task_id)
	return nil
end

local t = redis.call('TIME')
local time = tonumber(t[1])

redis.call('ZADD', KEYS[3], time, task_id)
return task_id