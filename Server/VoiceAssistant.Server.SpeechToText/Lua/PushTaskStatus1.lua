-- KEYS[1] - audio task stream
-- KEYS[2] - audio task stream group
-- KEYS[3] - command task stream
--
-- ARGV[1] - stream message id
-- ARGV[2] - new task

redis.call('XACKDEL', KEYS[1], KEYS[2], 'DELREF', 1, ARGV[1])
redis.call('XADD', KEYS[3], '*', 'payload', ARGV[2])