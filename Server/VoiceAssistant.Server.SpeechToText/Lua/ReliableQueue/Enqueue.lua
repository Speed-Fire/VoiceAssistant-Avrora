-- KEYS[1] - task description hash
-- KEYS[2] - task status hash
-- KEYS[3] - pending queue
--
-- ARGV[1] - task id
-- ARGV[2] - task description
-- ARGV[3] - required status to enqueue task

local status = redis.call('HGET', KEYS[2])
if status ~= tonumber(ARGV[3]) then
	return 0
end

redis.call('HSET', KEYS[1], ARGV[1], ARGV[2])
redis.call('LPUSH', KEYS[3], ARGV[1])
redis.call('HINCRBY', KEYS[2], 1)
return 1