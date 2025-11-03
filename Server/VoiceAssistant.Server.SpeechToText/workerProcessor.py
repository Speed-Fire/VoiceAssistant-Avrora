import redis
import whisper
import redis
import json
import paramiko
import tempfile

import luaScript

class workerProcessor(object):
    
    def __init__(self, db: redis.Redis, ai: whisper.Whisper,
                 audio_stream: str,
                 audio_stream_group: str,
                 command_stream: str,
                 sftp_ip: str,
                 sftp_port: int,
                 sftp_username: str,
                 sftp_password: str):
        self.__db = db
        self.__ai = ai
        self.__audio_stream = audio_stream
        self.__audio_stream_group = audio_stream_group
        self.__command_stream = command_stream
        self.__sftp_ip = sftp_ip
        self.__sftp_port = sftp_port
        self.__sftp_username = sftp_username
        self.__sftp_password = sftp_password


        pushTaskScript = self._get_file_content('./Lua/PushTaskStatus1.lua')
        self.__script = luaScript.LuaScript(db, pushTaskScript)

    def _get_file_content(self, path: str):
        content = None
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read().replace('\n\r', '\n')

        if content is None or not content:
            raise FileNotFoundError(path)

        return content

    def _open_sftp(self):
        ssh_client = paramiko.SSHClient()
        ssh_client.set_missing_host_key_policy(paramiko.AutoAddPolicy())

        ssh_client.connect(
            self.__sftp_ip,
            self.__sftp_port,
            self.__sftp_username,
            self.__sftp_password
        )

        sftp = ssh_client.open_sftp()

        return ssh_client, sftp

    def _get_task_description(self, task_json: str):
        data = json.loads(task_json)
        return data['Id'], data['User'], data['Status'], data['Content']

    def _create_command_handling_task(self, task_id, user, recognized_text):
        data = {
            "Id": task_id,
            "User": user,
            "Status": 1,
            "Content": recognized_text
            }

        return json.dumps(data)

    def process_task(self, msg_id, task_json: str):
        task_id, user, status, audio_url = self._get_task_description(task_json)
        ssh_client, sftp = self._open_sftp()

        with tempfile.NamedTemporaryFile(suffix = ".mp3") as tmp:
            sftp.get(audio_url, tmp)
            tmp.flush()
            
            recognized_text = self.__ai.transcribe(tmp.name)
            command_task = self._create_command_handling_task(task_id, user, recognized_text)

            self.__script.call([self.__audio_stream, self.__audio_stream_group, self.__command_stream], [msg_id, command_task])

            sftp.remove(audio_url)

        sftp.close()
        ssh_client.close()





