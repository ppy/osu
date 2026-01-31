#include "oboe_audio.h"
#include <android/log.h>

#define LOG_TAG "OsuOboeAudio"
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

OboeAudio::OboeAudio() {}

OboeAudio::~OboeAudio() {
    stop();
}

bool OboeAudio::initialize() {
    oboe::AudioStreamBuilder builder;
    builder.setDirection(oboe::Direction::Output)
           ->setPerformanceMode(oboe::PerformanceMode::LowLatency)
           ->setSharingMode(oboe::SharingMode::Exclusive)
           ->setUsage(oboe::Usage::Game)
           ->setFormat(oboe::AudioFormat::Float)
           ->setChannelCount(oboe::ChannelCount::Stereo)
           // Let Oboe choose the native sample rate if not specified,
           // but user recommended 48000. We'll use Unspecified to get the best one.
           ->setSampleRate(oboe::SampleRate::Unspecified)
           ->setDataCallback(this);

    oboe::Result result = builder.openStream(stream);
    if (result != oboe::Result::OK) {
        LOGE("Error opening stream: %s", oboe::convertToText(result));
        return false;
    }

    // Optimization: Set buffer size to 2x burst frames for low latency
    int32_t framesPerBurst = stream->getFramesPerBurst();
    stream->setBufferSizeInFrames(framesPerBurst * 2);

    return true;
}

void OboeAudio::start() {
    if (stream) {
        stream->requestStart();
    }
}

void OboeAudio::stop() {
    if (stream) {
        stream->requestStop();
        stream->close();
        stream.reset();
    }
}

oboe::DataCallbackResult OboeAudio::onAudioReady(oboe::AudioStream *oboeStream, void *audioData, int32_t numFrames) {
    // Fill audioData with zeros for now.
    // In a real implementation, we would pull from the game's audio buffer.
    float *output = static_cast<float *>(audioData);
    memset(output, 0, numFrames * oboeStream->getChannelCount() * sizeof(float));

    // The master timeline would be updated here using oboeStream->getTimestamp()

    return oboe::DataCallbackResult::Continue;
}

// Native bindings
extern "C" {
    JNIEXPORT jlong JNICALL nOboeCreate() {
        return reinterpret_cast<jlong>(new OboeAudio());
    }

    JNIEXPORT void JNICALL nOboeDestroy(jlong ptr) {
        delete reinterpret_cast<OboeAudio*>(ptr);
    }

    // JNI fallback
    JNIEXPORT jlong JNICALL Java_osu_Android_Native_OboeAudio_nCreate(JNIEnv* env, jobject thiz) {
        return nOboeCreate();
    }

    JNIEXPORT void JNICALL Java_osu_Android_Native_OboeAudio_nDestroy(JNIEnv* env, jobject thiz, jlong ptr) {
        nOboeDestroy(ptr);
    }
}
