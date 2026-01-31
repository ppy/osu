#ifndef OBOE_AUDIO_H
#define OBOE_AUDIO_H

#include <oboe/Oboe.h>

class OboeAudio : public oboe::AudioStreamDataCallback {
public:
    OboeAudio();
    ~OboeAudio();

    bool initialize();
    void start();
    void stop();

    oboe::DataCallbackResult onAudioReady(oboe::AudioStream *oboeStream, void *audioData, int32_t numFrames) override;

private:
    std::shared_ptr<oboe::AudioStream> stream;
};

#endif // OBOE_AUDIO_H
