-- KEYS[1] - task description hash
-- KEYS[2] - task status hash
-- KEYS[3] - pending queue
--
-- ARGV[1] - task id
-- ARGV[2] - task description

local task_id_exists = redis.call('HSETNX', KEYS[1], ARGV[1], ARGV[2])
if task_id_exists == 0
	return 0
end

redis.call('HSET', KEYS[2], ARGV[1], 0)
redis.call('LPUSH', KEYS[3], ARGV[1])
return 1