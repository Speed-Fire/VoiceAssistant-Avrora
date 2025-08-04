import redis
import time

class ReliableQueue(object):
    def __init__(self, db: redis.Redis, pending_queue, processing_queue, timestamps_queue):
        self.__db = db
        self.__pending_queue = pending_queue
        self.__processing_queue = processing_queue
        self.__timestamps_queue = timestamps_queue

    def enqueue(self, item):
        self.__db.lpush(self.__pending_queue, item)

    def dequeue(self):
        _, item = self.__db.brpoplpush(self.__pending_queue, self.__processing_queue)
        now = int(time.time())
        self.__db.hset(self.__timestamps_queue, item, now)
        return item

    def mark_completed(self, item):
        self.__db.lrem(self.__processing_queue, 0, item)
        self.__db.hdel(self.__timestamps_queue, item)



