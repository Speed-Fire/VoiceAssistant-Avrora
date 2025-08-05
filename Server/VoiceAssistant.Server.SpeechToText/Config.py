
import os

redis_ip = os.getenv("REDIS_HOST")
redis_port = int(os.getenv("REDIS_PORT"))
redis_password = os.getenv("REDIS_PASSWORD")

redis_recognition_queue_name = os.getenv("REDIS_QUEUES_RECOGNITION_PENDING")
redis_temp_recognition_queue_name = os.getenv("REDIS_QUEUES_RECOGNITION_PROCESSING")
redis_timestamps_recognition_queue_name = os.getenv("REDIS_HASHES_RECOGNITION_TIMESTAMPS")

redis_command_handling_queue_name = os.getenv("REDIS_QUEUES_COMMAND_HANDLING_PENDING")
redis_temp_command_handling_queue_name = os.getenv("REDIS_QUEUES_COMMAND_HANDLING_PROCESSING")
redis_timestamps_command_handling_queue_name = os.getenv("REDIS_HASHES_COMMAND_HANDLING_TIMESTAMPS")

sftp_ip = os.getenv("SFTP_HOST")
sftp_port = int(os.getenv("SFTP_PORT"))
sftp_username = os.getenv("SFTP_USERS_AUDIO_NAME")
sftp_password = os.getenv("SFTP_USERS_AUDIO_PASSWORD")



