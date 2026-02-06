// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#ifndef OBOE_AUDIO_H
#define OBOE_AUDIO_H

#include <oboe/Oboe.h>
#include <jni.h>

class OboeAudio : public oboe::AudioStreamCallback {
public:
    OboeAudio();
    ~OboeAudio();

    bool initialize();
    void start();
    void stop();
    double getTimestamp();

    oboe::DataCallbackResult onAudioReady(oboe::AudioStream *oboeStream, void *audioData, int32_t numFrames) override;

private:
    oboe::AudioStream *stream;
};

#endif // OBOE_AUDIO_H
