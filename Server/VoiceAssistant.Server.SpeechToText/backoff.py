import time

class Backoff(object):
    
    def __init__(self, init_backoff=0.01, max_backoff=0.5):
        self.__init_backoff = init_backoff
        self.__max_backoff = max_backoff
        self.__backoff = init_backoff

    def wait(self):
        time.sleep(self.__backoff)
        self.__backoff = min(self.__max_backoff, self.__backoff * 2)

    def reset(self):
        self.__backoff = self.__init_backoff
