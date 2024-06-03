#include "audio.h"

// #define AUDIO_FRAME_DEBUG
// #define AUDIO_CANCEL

static handlers_t* handlers;

// audio buffer ===============

#define QUEST_AUDIO_FREQUENCY 48000
#define MAX_AUDIO_BATCH_SIZE (1024 * 8)
static float AudioBatch[MAX_AUDIO_BATCH_SIZE]; // Downsampled audio buffer
static float inBufferL[MAX_AUDIO_BATCH_SIZE / 2];   // internal transaction buffer for Left channel
static float inBufferR[MAX_AUDIO_BATCH_SIZE / 2];   // internal transaction buffer for Right channel
static size_t AudioBatchOccupancy = 0;

static AudioBufferLock AudioBufferLockCB;
static AudioBufferUnlock AudioBufferUnLockCB;

void wrapper_audio_sample_cb(int16_t left, int16_t right) { return; }

static size_t wrapper_audio_sample_batch_cb(const int16_t* data,
	size_t frames) {
	size_t consumedFrames = 0;

#ifdef AUDIO_FRAME_DEBUG
	wrapper_environment_log(
		RETRO_LOG_INFO, "[wrapper_audio_sample_batch_cb] frames: %i\n", frames);
#endif


	if (data == NULL || frames < 2) {
#ifdef AUDIO_FRAME_DEBUG
		wrapper_environment_log(RETRO_LOG_ERROR,
			"[wrapper_audio_sample_batch_cb] no data received\n");
#endif
		return 0;
	}

	if (frames % 2 != 0) {
		// Handle the case where frames is not even (e.g., by skipping the last odd
		// sample)
		frames--;
	}
	size_t totalFrames = frames * 2;

#ifdef AUDIO_CANCEL
	return frames;
#endif

	if (AudioBatchOccupancy >= MAX_AUDIO_BATCH_SIZE ||
		totalFrames > MAX_AUDIO_BATCH_SIZE) {
#ifdef AUDIO_FRAME_DEBUG
		wrapper_environment_log(RETRO_LOG_ERROR,
			"[wrapper_audio_sample_batch_cb] buffer exceeded - "
			"frames:%i AudioBatchOccupancy:%i\n",
			totalFrames, AudioBatchOccupancy);
#endif
		return 0;
	}

	// Copy data into the input buffers
	for (size_t i = 0; i < frames; i++) {
		inBufferL[i] = (float)data[2 * i] / 32768.0f;
		inBufferR[i] = (float)data[2 * i + 1] / 32768.0f;
	}

	AudioBufferLockCB(); // Lock the AudioBatch for synchronization

	double ratio = (double)wrapper_environment_get_sample_rate() / QUEST_AUDIO_FREQUENCY;
	size_t outSample = 0;

	while (1) {
		double inBufferIndex = outSample * ratio;
		int index = (int)inBufferIndex;
		double frac = inBufferIndex - index; // fractional part
		outSample++;

		if (index + 1 >= frames) {
			break;
		}

		// Linear interpolation for left channel
		float leftSample = (1 - frac) * inBufferL[index] + frac * inBufferL[index + 1];
		// Linear interpolation for right channel
		float rightSample = (1 - frac) * inBufferR[index] + frac * inBufferR[index + 1];

		// Store the interpolated samples in the combined output buffer
		AudioBatch[AudioBatchOccupancy] = leftSample;
		AudioBatchOccupancy++;
		AudioBatch[AudioBatchOccupancy] = rightSample;
		AudioBatchOccupancy++;
		consumedFrames += 2; // increment by 2 for stereo

		if (AudioBatchOccupancy >= MAX_AUDIO_BATCH_SIZE) { // Check for buffer overflow
			break;
		}
	}

	AudioBufferUnLockCB(); // Unlock the AudioBatch

	return consumedFrames;
}

// Function to get the buffer pointer
float* wrapper_audio_get_audio_buffer_pointer() { return AudioBatch; }

// Function to get the buffer occupancy
size_t wrapper_audio_get_audio_buffer_occupancy() {
	return AudioBatchOccupancy;
}

// Function to consume the first part of the buffer and move the rest to
// position zero
// The lock is locked in the caller c# function.
void wrapper_audio_consume_buffer(size_t consumeSizeFloat) {

#ifdef AUDIO_FRAME_DEBUG
	wrapper_environment_log(
		RETRO_LOG_INFO,
		"[wrapper_audio_consume_buffer] consumeSize:%i AudioBatchOccupancy:%i\n",
		consumeSizeFloat, AudioBatchOccupancy);
#endif

	size_t size;
	size_t consume = consumeSizeFloat;
	if (consume > AudioBatchOccupancy) {
		wrapper_environment_log(
			RETRO_LOG_ERROR,
			"[wrapper_audio_consume_buffer] consume size exceded occupancy: %i\n",
			consume);
		consume = AudioBatchOccupancy;
	}

	// Use memmove to move the remaining data to the beginning of the buffer
	size = (AudioBatchOccupancy - consume) * sizeof(float);
	if (size)
		memmove(AudioBatch, AudioBatch + consume, size);

	AudioBatchOccupancy -= consume; // Update occupancy
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

void wrapper_audio_free() {
	AudioBufferLockCB(); // Lock the AudioBatch for synchronization
	AudioBatchOccupancy = 0;
	AudioBufferUnLockCB(); // Unlock the AudioBatch
}
