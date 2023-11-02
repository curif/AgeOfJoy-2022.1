#include "audio.h"

static handlers_t *handlers;

// audio buffer ===============

#define AUDIO_BUFFER_MAX_OCCUPANCY (1024 * 8)
#define QUEST_AUDIO_FREQUENCY 48000
#define MAX_AUDIO_BATCH_SIZE (8192)            // 8K bytes
static float AudioBatch[MAX_AUDIO_BATCH_SIZE]; // Static array
static size_t AudioBatchOccupancy = 0;

static AudioBufferLock AudioBufferLockCB;
static AudioBufferUnlock AudioBufferUnLockCB;

// Track the current position in AudioBatch
static size_t audioBufferPosition = 0;

void wrapper_audio_sample_cb(int16_t left, int16_t right) { return; }

static size_t wrapper_audio_sample_batch_cb(const int16_t *data,
                                            size_t frames) {
  if (data == NULL) {
    return 0;
  }

  if (AudioBatchOccupancy > AUDIO_BUFFER_MAX_OCCUPANCY) {
    return 0;
  }

  if (frames * 2 * sizeof(float) > MAX_AUDIO_BATCH_SIZE) {
    return 0; // Input data size exceeds the maximum AudioBatch size
  }

  //   wrapper_environment_log(
  //       RETRO_LOG_INFO,
  //       "[wrapper_audio_sample_batch_cb] frames:%i AudioBatchOccupancy:%i\n",
  //       frames, AudioBatchOccupancy);

  // Initialize inBuffer here
  float inBuffer[frames * 2]; // Since each frame has 2 samples

  for (size_t i = 0; i < frames * 2; i++) {
    float value = (float)data[i] / 32768.0f;
    inBuffer[i] = value;
  }

  double ratio =
      (double)wrapper_environment_get_sample_rate() / QUEST_AUDIO_FREQUENCY;

  AudioBufferLockCB(); // Lock the AudioBatch for synchronization

  size_t outSample = 0;
  while (1) {
    int inBufferIndex = (int)(outSample++ * ratio);
    if (inBufferIndex < (int)frames * 2) {
      AudioBatch[AudioBatchOccupancy++] = inBuffer[inBufferIndex];
      if (AudioBatchOccupancy >= MAX_AUDIO_BATCH_SIZE)
        break;
    } else
      break;
  }
  
  AudioBufferUnLockCB(); // Unlock the AudioBatch

  return frames;
}

// Function to get the buffer pointer
float *wrapper_audio_get_audio_buffer_pointer() { return AudioBatch; }

// Function to get the buffer occupancy
size_t wrapper_audio_get_audio_buffer_occupancy() {
  return AudioBatchOccupancy;
}
size_t wrapper_audio_get_audio_buffer_occupancy_bytes() {
  return AudioBatchOccupancy * sizeof(float);
}

// Function to consume the first part of the buffer and move the rest to
// position zero
void wrapper_audio_consume_buffer(size_t consumeSize) {
  //   wrapper_environment_log(
  //       RETRO_LOG_INFO,
  //       "[wrapper_audio_consume_buffer] consumeSize:%i
  //       AudioBatchOccupancy:%i\n", consumeSize, AudioBatchOccupancy);
  if (consumeSize >= AudioBatchOccupancy) {
    AudioBatchOccupancy = 0; // Reset occupancy
  } else {
    // Use memmove to move the remaining data to the beginning of the buffer
    memmove(AudioBatch, AudioBatch + consumeSize,
            (AudioBatchOccupancy - consumeSize) * sizeof(float));
    AudioBatchOccupancy -= consumeSize; // Update occupancy
  }
}
void wrapper_audio_consume_buffer_bytes(size_t consumeSizeBytes) {
  wrapper_audio_consume_buffer(consumeSizeBytes / sizeof(float));
  return;
}

void wrapper_audio_init(AudioBufferLock audioBufferLockCB,
                        AudioBufferUnlock audioBufferUnLockCB) {
  wrapper_environment_log(RETRO_LOG_INFO, "[wrapper_audio_init]\n");
  handlers = wrapper_environment_get_handlers();
  if (!handlers)
    return;

  AudioBufferUnLockCB = audioBufferUnLockCB;
  AudioBufferLockCB = audioBufferLockCB;

  handlers->retro_set_audio_sample(&wrapper_audio_sample_cb);
  handlers->retro_set_audio_sample_batch(&wrapper_audio_sample_batch_cb);
}

void wrapper_audio_free() { audioBufferPosition = 0; }
