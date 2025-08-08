import redis
import time
import luaScript

class ReliableQueue(object):
    def __init__(self, db:redis.Redis, pending_queue, processing_queue, timestamps_hash):
        self.__db = db
        self.__pending_queue = pending_queue
        self.__processing_queue = processing_queue
        self.__timestamps_hash = timestamps_hash
        self.__deq_script = None
        self.__markcomp_script = None

        dequeue_script = self._get_file_conent('./Lua/ReliableQueue/Dequeue.lua')
        markcomplete_script = self._get_file_conent('./Lua/ReliableQueue/MarkAsCompleted.lua')

        self.__deq_script = luaScript.LuaScript(db, dequeue_script)
        self.__markcomp_script = luaScript.LuaScript(db, markcomplete_script)

    def _get_file_conent(self, path: str):
        content = None
        with open(path, 'r', encoding='utf-8') as f:
            markcomplete_script = f.read().replace('\n\r', '\n')

        if content is None or not content:
            raise FileNotFoundError(path)

    def enqueue(self, item):
        self.__db.lpush(self.__pending_queue, item)

    def dequeue(self):
        item = self.__deq_script.call([self.__pending_queue, self.__processing_queue, self.__timestamps_hash], [])
        return item

    def mark_completed(self, item):
        self.__markcomp_script.call([self.__pending_queue, self.__processing_queue, self.__timestamps_hash], [item])



