#include "audio.h"

// #define AUDIO_FRAME_DEBUG
// #define AUDIO_CANCEL

static handlers_t* handlers;

// audio buffer ===============

// Rate the ring buffer content is resampled TO (what the consumer drains per
// second of wall time). Historically hardcoded to 48000, but Unity's audio
// thread on Quest measurably consumes fewer frames per second than the nominal
// DSP rate (~84% on Quest 2 / Unity 2021.3: ~40320 of 48000). Producing at the
// nominal rate permanently overflows the ring and drops chunks (audible
// crackling). The C# side measures the real consumption rate and sets it here
// via wrapper_audio_set_output_rate(); default keeps the legacy behavior.
#define DEFAULT_OUTPUT_SAMPLE_RATE 48000.0
static double OutputSampleRate = DEFAULT_OUTPUT_SAMPLE_RATE;
#define MAX_AUDIO_BATCH_SIZE (1024 * 8)
static float AudioBatch[MAX_AUDIO_BATCH_SIZE];		// Downsampled audio buffer
static float inBufferL[MAX_AUDIO_BATCH_SIZE / 2];   // internal buffer for Left channel
static float inBufferR[MAX_AUDIO_BATCH_SIZE / 2];   // internal buffer for Right channel
static size_t AudioBatchOccupancy = 0;

static AudioBufferLock AudioBufferLockCB;
static AudioBufferUnlock AudioBufferUnLockCB;

void wrapper_audio_sample_cb(int16_t left, int16_t right) { return; }

static size_t wrapper_audio_sample_batch_cb(const int16_t* data,
	size_t frames) {

#ifdef AUDIO_FRAME_DEBUG
	wrapper_environment_log(
		RETRO_LOG_INFO, "[wrapper_audio_sample_batch_cb] frames: %i\n", frames);
#endif

	if (data == NULL || frames < 1) {
#ifdef AUDIO_FRAME_DEBUG
		wrapper_environment_log(RETRO_LOG_ERROR,
			"[wrapper_audio_sample_batch_cb] no data received\n");
#endif
		return 0;
	}

#ifdef AUDIO_CANCEL
	return frames;
#endif

	if (AudioBatchOccupancy >= MAX_AUDIO_BATCH_SIZE || (frames * 2) > MAX_AUDIO_BATCH_SIZE) {
#ifdef AUDIO_FRAME_DEBUG
		wrapper_environment_log(RETRO_LOG_ERROR,
			"[wrapper_audio_sample_batch_cb] buffer exceeded - "
			"frames:%i AudioBatchOccupancy:%i\n",
			frames, AudioBatchOccupancy);
#endif
		return 0;
	}

	// Copy data into the Left/Right channel input buffers, while performing normalisation (-1.0f to 1.0f)
	for (size_t i = 0; i < frames; i++) {
		inBufferL[i] = (float)data[2 * i] / 32768.0f;
		inBufferR[i] = (float)data[2 * i + 1] / 32768.0f;
	}

	AudioBufferLockCB(); // Lock the AudioBatch for synchronization

	double inputSampleRate = wrapper_environment_get_sample_rate();
	size_t consumedFrames = 0;

	if (inputSampleRate == OutputSampleRate) {
		// If sample rates match, directly copy the data to the output buffer
		for (size_t i = 0; i < frames; i++) {
			AudioBatch[AudioBatchOccupancy] = inBufferL[i];
			AudioBatch[AudioBatchOccupancy + 1] = inBufferR[i];
			AudioBatchOccupancy += 2;
			consumedFrames++;

			if (AudioBatchOccupancy >= MAX_AUDIO_BATCH_SIZE) { // Check for buffer overflow
#ifdef AUDIO_FRAME_DEBUG
				wrapper_environment_log(RETRO_LOG_ERROR,
					"[wrapper_audio_sample_batch_cb] AudioBatchOccupancy >= MAX_AUDIO_BATCH_SIZE i:%d AudioBatchOccupancy:%d\n",
					i, AudioBatchOccupancy);
#endif
				break;
			}
		}
	}
	else {
		// Perform interpolation if sample rates do not match
		double ratio = inputSampleRate / OutputSampleRate;
		size_t outSample = 0;

		while (1) {
			double inBufferIndex = outSample * ratio;
			int index = (int)inBufferIndex;

			if (index >= frames) { // We have reached the end of the input buffer
				break;
			}

			// Perform linear interpolation
			double frac = inBufferIndex - index; // Fractional part
			int upperIndex = index == frames - 1 ? index : index + 1;	// Special case, interpolation for the last sample
			float leftSample = (1 - frac) * inBufferL[index] + frac * inBufferL[upperIndex];
			float rightSample = (1 - frac) * inBufferR[index] + frac * inBufferR[upperIndex];

			// Store the interpolated samples in the combined output buffer
			AudioBatch[AudioBatchOccupancy] = leftSample;
			AudioBatch[AudioBatchOccupancy + 1] = rightSample;
			AudioBatchOccupancy += 2;
			outSample++;
			consumedFrames = index + 1;

			if (AudioBatchOccupancy >= MAX_AUDIO_BATCH_SIZE) { // Check for buffer overflow
#ifdef AUDIO_FRAME_DEBUG
				wrapper_environment_log(RETRO_LOG_ERROR,
					"[wrapper_audio_sample_batch_cb] AudioBatchOccupancy >= MAX_AUDIO_BATCH_SIZE index:%d AudioBatchOccupancy:%d MAX_AUDIO_BATCH_SIZE:%d\n",
					index, AudioBatchOccupancy, MAX_AUDIO_BATCH_SIZE);
#endif
				break;
			}
		}
	}

	AudioBufferUnLockCB(); // Unlock the AudioBatch

#ifdef AUDIO_FRAME_DEBUG
	wrapper_environment_log(
		RETRO_LOG_INFO, "[wrapper_audio_sample_batch_cb] consumedFrames: %i\n", consumedFrames);
#endif

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

// Set the rate the resampler produces for the consumer. Called by the C# side
// with the MEASURED consumption rate (frames per wall-clock second) so that
// production matches what Unity actually drains; 0 or out-of-range restores
// the default. Call after wrapper_audio_init, before or during emulation
// (takes effect on the next audio batch).
void wrapper_audio_set_output_rate(double rate) {
	if (rate < 8000.0 || rate > 192000.0)
		rate = DEFAULT_OUTPUT_SAMPLE_RATE;
	OutputSampleRate = rate;
	wrapper_environment_log(RETRO_LOG_INFO,
		"[wrapper_audio_set_output_rate] output rate: %f\n", OutputSampleRate);
}
