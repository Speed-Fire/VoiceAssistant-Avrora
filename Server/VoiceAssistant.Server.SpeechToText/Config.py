
import os

redis_ip = os.getenv("REDIS_HOST")
redis_port = int(os.getenv("REDIS_PORT"))
redis_password = os.getenv("REDIS_PASSWORD")

redis_stream_audio = os.getenv("REDIS_STREAMS_STT")
redis_stream_audio_group = os.getenv("REDIS_STREAMS_STT_GROUP")
redis_stream_command = os.getenv("REDIS_STREAMS_CH")

sftp_ip = os.getenv("SFTP_HOST")
sftp_port = int(os.getenv("SFTP_PORT"))
sftp_username = os.getenv("SFTP_USERS_AUDIO_NAME")
sftp_password = os.getenv("SFTP_USERS_AUDIO_PASSWORD")
