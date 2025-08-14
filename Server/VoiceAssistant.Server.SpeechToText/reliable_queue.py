import redis
import time
import luaScript

class ReliableQueue(object):
    def __init__(self, db:redis.Redis, items_hash, items_status_hash, req_status_to_enq, pending_queue, processing_queue, timestamps_set):
        self.__db = db
        self.__items_hash = items_hash
        self.__items_status_hash = items_hash
        self.__req_status_to_enq = req_status_to_enq
        self.__pending_queue = pending_queue
        self.__processing_queue = processing_queue
        self.__timestamps_set = timestamps_set
        self.__deq_script = None
        self.__markcomp_script = None
        self.__enq_script = None

        dequeue_script = self._get_file_content('./Lua/ReliableQueue/Dequeue.lua')
        markcomplete_script = self._get_file_content('./Lua/ReliableQueue/MarkAsCompleted.lua')
        enq_script = self._get_file_content('./Lua/ReliableQueue/Enqueue.lua')

        self.__deq_script = luaScript.LuaScript(db, dequeue_script)
        self.__markcomp_script = luaScript.LuaScript(db, markcomplete_script)
        self.__enq_script = luaScript.LuaScript(db, enq_script)

    def _get_file_content(self, path: str):
        content = None
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read().replace('\n\r', '\n')

        if content is None or not content:
            raise FileNotFoundError(path)

        return content

    def enqueue(self, item, item_descr):
        return self.__enq_script.call([self.__items_hash, self.__items_status_hash, self.__pending_queue], [item, item_descr, self.__req_status_to_enq])
        #self.__db.lpush(self.__pending_queue, item)

    def dequeue(self):
        item = self.__deq_script.call([self.__pending_queue, self.__processing_queue, self.__timestamps_set], [])
        return item

    def mark_completed(self, item):
        self.__markcomp_script.call([self.__pending_queue, self.__processing_queue, self.__timestamps_set], [item])
        # if self.__db.lrem(self.__processing_queue, item) == 0:
        #     return False
        # self.__db.zrem(self.__timestamps_hash, item)
        # return True

