#include "oboe_audio.h"

OboeAudio::OboeAudio() {}

OboeAudio::~OboeAudio() {
    stop();
}

bool OboeAudio::initialize() {
    oboe::AudioStreamBuilder builder;
    builder.setDirection(oboe::Direction::Output)
           ->setPerformanceMode(oboe::PerformanceMode::LowLatency)
           ->setSharingMode(oboe::SharingMode::Exclusive)
           ->setFormat(oboe::AudioFormat::Float)
           ->setChannelCount(oboe::ChannelCount::Stereo)
           ->setSampleRate(48000) // Device native sample rate
           ->setDataCallback(this);

    oboe::Result result = builder.openStream(stream);
    return result == oboe::Result::OK;
}

void OboeAudio::start() {
    if (stream) stream->requestStart();
}

void OboeAudio::stop() {
    if (stream) {
        stream->requestStop();
        stream->close();
    }
}

oboe::DataCallbackResult OboeAudio::onAudioReady(oboe::AudioStream *oboeStream, void *audioData, int32_t numFrames) {
    // Fill audioData with zeros for now, or pull from a managed buffer
    float *output = static_cast<float *>(audioData);
    for (int i = 0; i < numFrames * 2; ++i) {
        output[i] = 0.0f;
    }
    return oboe::DataCallbackResult::Continue;
}

// JNI bindings
extern "C" {
    JNIEXPORT jlong JNICALL Java_osu_Android_OboeAudio_nCreate(JNIEnv* env, jobject thiz) {
        return reinterpret_cast<jlong>(new OboeAudio());
    }

    JNIEXPORT void JNICALL Java_osu_Android_OboeAudio_nDestroy(JNIEnv* env, jobject thiz, jlong ptr) {
        delete reinterpret_cast<OboeAudio*>(ptr);
    }
}
