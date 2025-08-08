-- KEYS[1] - pending queue
-- KEYS[2] - processing queue
-- KEYS[3] - timestamps hash
--
-- ARGV[1] - task id

redis.call('LREM', KEYS[2], ARGV[1])
redis.call('ZREM', KEYS[3], ARGV[1])