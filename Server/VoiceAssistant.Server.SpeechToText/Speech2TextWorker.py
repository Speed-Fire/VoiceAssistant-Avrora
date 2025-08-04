import whisper
import redis
import json
import paramiko
import tempfile

import Config

import reliable_queue

def redis_db():
    db = redis.Redis(
        host = Config.redis_ip,
        port = Config.redis_port,
        password = Config.redis_password,
        decode_responses = True
    )
    
    db.ping()
    
    return db

def get_recognition_queue(): 
    task_queue = reliable_queue.ReliableQueue(db, 
        Config.redis_recognition_queue_name,
        Config.redis_temp_recognition_queue_name,
        Config.redis_timestamps_recognition_queue_name
    )

    return task_queue

def get_command_handling_queue(): 
    task_queue = reliable_queue.ReliableQueue(db, 
        Config.redis_command_handling_queue_name,
        Config.redis_temp_command_handling_queue_name,
        Config.redis_timestamps_command_handling_queue_name
    )

    return task_queue

def parse_recognition_task(task):
    data = json.loads(task)
    return data['Id'], data['User'], data['Uri']

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
        "Text": recognized_text
        }

    return json.dumps(data)

def main():
    db = redis_db()
    recog_queue = get_recognition_queue()
    handling_queue = get_command_handling_queue()
    
    ai_model = whisper.load_model(name = "small")

    while True:
        recog_task = recog_queue.dequeue()
        task_id, user, audio_url = parse_recognition_task(recog_task)

        ssh_client, sftp = open_sftp()

        with tempfile.NamedTemporaryFile(suffix = ".mp3") as tmp:
            sftp.get(audio_url, tmp)
            tmp.flush()

            recognized_text = ai_model.transcribe(tmp.name)

            command_task = create_command_handling_task(task_id, user, recognized_text)
            handling_queue.enqueue(command_task)

            recog_queue.mark_completed(recog_task)
            sftp.remove(audio_url)       

        sftp.close()
        ssh_client.close()

if __name__ == '__main__':
    main()