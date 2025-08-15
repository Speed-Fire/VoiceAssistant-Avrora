# Avrora Voice Assistant – Server Component

This repository contains the **server-side** component of the **VoiceAssistant Avrora** project.

## Overview

The server is responsible for processing audio commands received from clients, interpreting them, and dispatching appropriate actions. It handles speech recognition, command understanding, and communication with other system services and clients.

## Key Features

- **gRPC Communication**: Provides a gRPC API for seamless communication with the voice assistant clients.
- **Redis Integration**: Utilized for:
  - Task queue management
  - Authentication
  - Other real-time operations
- **Speech-to-Text with Whisper**: Uses [Whisper](https://github.com/openai/whisper) to transcribe voice recordings into text.
- **Natural Language Understanding with Modern-BERT**: Applies Modern-BERT to analyze and interpret user commands accurately.
- **SFTP for Temporary Storage**: Temporarily stores incoming audio files and other data using SFTP.
- **Docker Containerization**: Fully containerized using Docker for easy deployment and scalability.

## Architecture

The server performs the following pipeline:

1. **Receives audio data** via gRPC from a client.
2. **Stores the data temporarily** using SFTP.
3. **Processes the audio** using Whisper to generate transcriptions.
4. **Analyzes the command** using Modern-BERT.
5. **Queues and executes tasks** using Redis.
6. **Sends back responses or triggers actions** (e.g., external device control or system commands).

## Part of a Bigger System

This server works together with:

- [**Client Application**](https://github.com/Speed-Fire/VoiceAssistant-Avrora-Client) – Receives user voice input and sends data to the server.
- **Device Controllers / Executors** – Carry out actions based on interpreted commands.

For the full functionality of **VoiceAssistant Avrora**, all components must be deployed and properly connected.

## Getting Started

Coming soon...

## License

All rights reserved. 2025
