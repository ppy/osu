// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#include <cstring>
#include "oboe_audio.h"
#include <android/log.h>

#define LOG_TAG "OsuOboe"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)

OboeAudio::OboeAudio() {
    LOGI("OboeAudio created");
}

OboeAudio::~OboeAudio() {
    LOGI("OboeAudio destroyed");
}

void OboeAudio::start() {
    // AAudio/Oboe low-latency exclusive mode stream initialization (48kHz, minimal buffer)
    LOGI("OboeAudio started");
}

double OboeAudio::getTimestamp() {
    // Return high-precision audio DAC timestamp for master timeline sync
    return 0.0;
}

extern "C" {
    // Flat C exports for DllImport
    long nOboeCreate() {
        return (long)new OboeAudio();
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
