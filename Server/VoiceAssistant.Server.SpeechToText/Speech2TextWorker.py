import redis.exceptions
import whisper
import redis
import json
import paramiko
import tempfile

import Config

import workerProcessor

def make_redis_db():
    db = redis.Redis(
        host = Config.redis_ip,
        port = Config.redis_port,
        password = Config.redis_password,
        decode_responses = True
    )

    db.ping()
    
    return db

def make_worker_processor(db: redis.Redis, ai: whisper.Whisper):
    processor = workerProcessor.workerProcessor(db, ai,
                                                Config.redis_stream_audio,
                                                Config.redis_stream_audio_group,
                                                Config.redis_stream_command,
                                                Config.sftp_ip,
                                                Config.sftp_port,
                                                Config.sftp_username,
                                                Config.sftp_password)

    return processor

def make_consumer_name():
    pass

def main():
    db = make_redis_db()  
    ai_model = whisper.load_model(name = "small")

    consumer_name = make_consumer_name()
    processor = make_worker_processor(db, ai_model)

    while True:
        msgs = db.xreadgroup(Config.redis_stream_audio_group, consumer_name, {Config.redis_stream_audio: '>'}, count=1, block=5000)
        if msgs:
            [(stream, entries)] = msgs
            for msg_id, data in entries:
                payload_json = data[b'payload'].decode("utf-8")
                processor.process_task(msg_id, payload_json)
            continue

        idle_ms = 30000
        claimed = db.xautoclaim(Config.redis_stream_audio, Config.redis_stream_audio_group, consumer_name, min_idle_time=idle_ms, start_id="0-0", count=1)

        # claimed structure: (new_start_id, [(msg_id, data), ...])
        _, msgs = claimed

        for msg_id, data in msgs:
            payload_json = data[b'payload'].decode("utf-8")
            processor.process_task(msg_id, payload_json)



if __name__ == '__main__':
    main()