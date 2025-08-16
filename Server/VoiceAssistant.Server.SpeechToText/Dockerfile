# For more information, please refer to https://aka.ms/vscode-docker-python
FROM python:3-slim

# Keeps Python from generating .pyc files in the container
ENV PYTHONDONTWRITEBYTECODE=1

# Turns off buffering for easier container logging
ENV PYTHONUNBUFFERED=1

# Separate cache for Whisper to avoid touching pip/torch caches
ENV XDG_CACHE_HOME=/opt/whisper-cache

#RUN rm -rf /home/appuser/.cache/whisper
RUN apt-get update && apt-get install -y git && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Install pip requirements
COPY ["Server/VoiceAssistant.Server.SpeechToText/requirements.txt", "./Server/VoiceAssistant.Server.SpeechToText/"]
RUN python -m pip install --no-cache-dir -r ./Server/VoiceAssistant.Server.SpeechToText/requirements.txt

# =============================
# Pre-download Whisper model during build
# =============================
RUN python3 - <<EOF
import whisper
whisper.load_model("small")  # скачиваем модель один раз
EOF

COPY "./Server/VoiceAssistant.Server.SpeechToText/" /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-python-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# During debugging, this entry point will be overridden. For more information, please refer to https://aka.ms/vscode-docker-python-debug
CMD ["python", "Speech2TextWorker.py"]