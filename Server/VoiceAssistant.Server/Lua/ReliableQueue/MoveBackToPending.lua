-- KEYS[1] - pending queue
-- KEYS[2] - processing queue
-- KEYS[3] - timestamps hash
--
-- ARGV[] - task ids

for item in ARGV do
	if redis.call('ZSCORE', KEYS[3], item) ~= nil then
		redis.call('LREM', KEYS[2], item)
		redis.call('ZREM', KEYS[3], item)
		redis.call('LPUSH', KEYS[1], item)
	end
end
