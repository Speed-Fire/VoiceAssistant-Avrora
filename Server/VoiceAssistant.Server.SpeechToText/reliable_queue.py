import redis
import time

class ReliableQueue(object):
    def __init__(self, pending_queue, processing_queue, timestamps_queue):
        self.__pending_queue = pending_queue
        self.__processing_queue = processing_queue
        self.__timestamps_queue = timestamps_queue

    def enqueue(self, db: redis.Redis, item):
        db.lpush(self.__pending_queue, item)

    def dequeue(self, db: redis.Redis):
        _, item = db.brpoplpush(self.__pending_queue, self.__processing_queue)
        now = int(time.time())
        db.hset(self.__timestamps_queue, item, now)
        return item

    def mark_completed(self, db: redis.Redis, item):
        db.lrem(self.__processing_queue, 0, item)
        db.hdel(self.__timestamps_queue, item)



