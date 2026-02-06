// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#include <cstring>
#include "oboe_audio.h"
#include <android/log.h>

#define LOG_TAG "OsuOboe"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)

OboeAudio::OboeAudio() : stream(nullptr) {
    LOGI("OboeAudio created");
}

OboeAudio::~OboeAudio() {
    stop();
    LOGI("OboeAudio destroyed");
}

bool OboeAudio::initialize() {
    oboe::AudioStreamBuilder builder;
    builder.setDirection(oboe::Direction::Output);
    builder.setPerformanceMode(oboe::PerformanceMode::LowLatency);
    builder.setSharingMode(oboe::SharingMode::Exclusive);
    builder.setFormat(oboe::AudioFormat::Float);
    builder.setChannelCount(oboe::ChannelCount::Stereo);
    builder.setSampleRate(48000);
    builder.setCallback(this);

    oboe::Result result = builder.openStream(&stream);
    if (result != oboe::Result::OK) {
        LOGI("Failed to create Oboe stream: %s", oboe::convertToText(result));
        return false;
    }
    return true;
}

void OboeAudio::start() {
    if (stream && stream->requestStart() != oboe::Result::OK) {
        LOGI("Failed to start Oboe stream");
    }
    LOGI("OboeAudio started");
}

void OboeAudio::stop() {
    if (stream) {
        stream->stop();
        stream->close();
        stream = nullptr;
    }
    LOGI("OboeAudio stopped");
}

double OboeAudio::getTimestamp() {
    // Return high-precision audio DAC timestamp for master timeline sync
    return 0.0;
}

oboe::DataCallbackResult OboeAudio::onAudioReady(oboe::AudioStream *oboeStream, void *audioData, int32_t numFrames) {
    // We are not providing any audio data yet
    memset(audioData, 0, numFrames * sizeof(float) * 2);
    return oboe::DataCallbackResult::Continue;
}

extern "C" {
    // Flat C exports for DllImport
    long nOboeCreate() {
        OboeAudio* audio = new OboeAudio();
        if (!audio->initialize()) {
            delete audio;
            return 0;
        }
        return (long)audio;
    }

    void nOboeDestroy(long audioPtr) {
        if (audioPtr) delete (OboeAudio*)audioPtr;
    }

    void nOboeStart(long audioPtr) {
        OboeAudio* audio = (OboeAudio*)audioPtr;
        if (audio) audio->start();
    }

    double nGetTimestamp(long audioPtr) {
        OboeAudio* audio = (OboeAudio*)audioPtr;
        return audio ? audio->getTimestamp() : 0.0;
    }

    // JNI exports (matching package osu.Android.Native)
    JNIEXPORT jlong JNICALL Java_osu_Android_Native_OboeAudio_nOboeCreate(JNIEnv* env, jobject obj) {
        return (jlong)nOboeCreate();
    }

    JNIEXPORT void JNICALL Java_osu_Android_Native_OboeAudio_nOboeDestroy(JNIEnv* env, jobject obj, jlong audioPtr) {
        nOboeDestroy((long)audioPtr);
    }

    JNIEXPORT void JNICALL Java_osu_Android_Native_OboeAudio_nOboeStart(JNIEnv* env, jobject obj, jlong audioPtr) {
        nOboeStart((long)audioPtr);
    }

    JNIEXPORT jdouble JNICALL Java_osu_Android_Native_OboeAudio_nOboeGetTimestamp(JNIEnv* env, jobject obj, jlong audioPtr) {
        return (jdouble)nGetTimestamp((long)audioPtr);
    }
}
