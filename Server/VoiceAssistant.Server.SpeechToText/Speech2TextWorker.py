import redis.exceptions
import whisper
import redis
import json
import paramiko
import tempfile

import Config

import reliable_queue
import backoff

def redis_db():
    db = redis.Redis(
        host = Config.redis_ip,
        port = Config.redis_port,
        password = Config.redis_password,
        decode_responses = True
    )

    db.ping()
    
    return db

def get_recognition_queue(db: redis.Redis): 
    task_queue = reliable_queue.ReliableQueue(db,
        Config.redis_task_description_hash_name,
        Config.redis_task_status_hash_name,
        9,
        Config.redis_recognition_queue_name,
        Config.redis_temp_recognition_queue_name,
        Config.redis_timestamps_recognition_set_name
    )

    return task_queue

def get_command_handling_queue(db: redis.Redis): 
    task_queue = reliable_queue.ReliableQueue(db,
        Config.redis_task_description_hash_name,
        Config.redis_task_status_hash_name,
        1,
        Config.redis_command_handling_queue_name,
        Config.redis_temp_command_handling_queue_name,
        Config.redis_timestamps_command_handling_set_name
    )

    return task_queue

def get_task_description(db: redis.Redis, task_id):
    task = db.hget(Config.redis_task_description_hash_name, task_id)
    data = json.loads(task)
    return data['Id'], data['User'], data['Status'], data['Content']

def open_sftp():
    ssh_client = paramiko.SSHClient()
    ssh_client.set_missing_host_key_policy(paramiko.AutoAddPolicy())

    ssh_client.connect(
        Config.sftp_ip,
        Config.sftp_port,
        Config.sftp_username,
        Config.sftp_password
    )

    sftp = ssh_client.open_sftp()

    return ssh_client, sftp

def create_command_handling_task(task_id, user, recognized_text):
    data = {
        "Id": task_id,
        "User": user,
        "Status": 1,
        "Content": recognized_text
        }

    return json.dumps(data)

def main():
    backof = backoff.Backoff()
    db = redis_db()
    recog_queue = get_recognition_queue(db)
    handling_queue = get_command_handling_queue(db)
    
    ai_model = whisper.load_model(name = "small")

    while True:
        recog_task_id = recog_queue.dequeue()
        if recog_task_id is None:
            backof.wait()
            continue

        backof.reset()
        task_id, user, status, audio_url = get_task_description(recog_task_id)

        ssh_client, sftp = open_sftp()

        with tempfile.NamedTemporaryFile(suffix = ".mp3") as tmp:
            sftp.get(audio_url, tmp)
            tmp.flush()

            recognized_text = ai_model.transcribe(tmp.name)
            command_task = create_command_handling_task(task_id, user, recognized_text)

            enq_res = handling_queue.enqueue(task_id, command_task)
            recog_queue.mark_completed(recog_task_id)

            if enq_res > 0:
                sftp.remove(audio_url)

        sftp.close()
        ssh_client.close()

if __name__ == '__main__':
    main()