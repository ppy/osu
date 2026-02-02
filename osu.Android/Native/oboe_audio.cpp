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
}

long OboeAudio::getTimestamp() {
    // Return high-precision audio DAC timestamp for master timeline sync
    return 0;
}

extern "C" {
    JNIEXPORT jlong JNICALL Java_osu_Android_Native_OboeAudio_nOboeCreate(JNIEnv* env, jobject obj) {
        return (jlong)new OboeAudio();
    }

    JNIEXPORT void JNICALL Java_osu_Android_Native_OboeAudio_nOboeDestroy(JNIEnv* env, jobject obj, jlong audioPtr) {
        if (audioPtr) delete (OboeAudio*)audioPtr;
    }

    JNIEXPORT void JNICALL Java_osu_Android_Native_OboeAudio_nOboeStart(JNIEnv* env, jobject obj, jlong audioPtr) {
        OboeAudio* audio = (OboeAudio*)audioPtr;
        if (audio) audio->start();
    }

    JNIEXPORT jlong JNICALL Java_osu_Android_Native_OboeAudio_nOboeGetTimestamp(JNIEnv* env, jobject obj, jlong audioPtr) {
        OboeAudio* audio = (OboeAudio*)audioPtr;
        return audio ? (jlong)audio->getTimestamp() : 0;
    }
}
