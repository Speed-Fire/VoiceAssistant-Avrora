-- KEYS[1] - pending queue
-- KEYS[2] - processing queue
-- KEYS[3] - timestamps hash
--
-- ARGV[1] - task id

if redis.call('HEXISTS', @timestampsHash, @task_id) == 1 then
	redis.call('LREM', @processingQueue, @task_id)
	redis.call('HDEL', @timestampsHash, @task_id)
	redis.call('LPUSH', @pendingQueue, @task_id)
	return 1
end
return 0