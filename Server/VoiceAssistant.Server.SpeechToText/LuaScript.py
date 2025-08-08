import redis

class LuaScript(object):

    def __init__(self, db: redis.Redis, script: str):
        self.__db = db
        self.__script = script
        self.__sha = None

    def _load_script(self):
        self.__sha = self.__db.script_load(self.__script)

    def call(self, keys = None, args = None):
        if keys is None:
            keys = []
        if args is None:
            args = []

        if self.__sha is None:
            self._load_script()

        try:
            return self.__db.evalsha(self.__sha, len(keys), *(keys + args))
        except redis.ResponseError as e:
            if 'NOSCRIPT' in str(e):
                self._load_script()
                return self.__db.evalsha(self.__sha, len(keys), *(keys + args))
            else:
                raise

    




