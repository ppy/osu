// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#include <android/native_window_jni.h>
#include "vulkan_renderer.h"
#include <vector>
#include <algorithm>
#include <cstring>
#include <android/log.h>

#define LOG_TAG "OsuVulkan"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)

VulkanRenderer::VulkanRenderer() : currentFrame(0) {
    LOGI("VulkanRenderer created");
}

VulkanRenderer::~VulkanRenderer() {
    LOGI("VulkanRenderer destroyed");
}

void VulkanRenderer::init(ANativeWindow* window) {
    // Vulkan initialization: Instance, Surface, Physical Device, Logical Device, Swapchain (2 images, MAILBOX)
    LOGI("Vulkan initialized with window %p", window);
}

void VulkanRenderer::render() {
    // 1-frame-in-flight late-submit rendering loop
    currentFrame = (currentFrame + 1) % MAX_FRAMES_IN_FLIGHT;
    // Acquire image, submit command buffer, present
}

extern "C" {
    JNIEXPORT jlong JNICALL Java_osu_Android_Native_VulkanRenderer_nVulkanCreate(JNIEnv* env, jobject obj) {
        return (jlong)new VulkanRenderer();
    }

    JNIEXPORT void JNICALL Java_osu_Android_Native_VulkanRenderer_nVulkanDestroy(JNIEnv* env, jobject obj, jlong rendererPtr) {
        if (rendererPtr) delete (VulkanRenderer*)rendererPtr;
    }

    JNIEXPORT void JNICALL Java_osu_Android_Native_VulkanRenderer_nVulkanInit(JNIEnv* env, jobject obj, jlong rendererPtr, jobject surface) {
        VulkanRenderer* renderer = (VulkanRenderer*)rendererPtr;
        if (!renderer || !surface) return;
        ANativeWindow* window = ANativeWindow_fromSurface(env, surface);
        renderer->init(window);
    }

    JNIEXPORT void JNICALL Java_osu_Android_Native_VulkanRenderer_nVulkanRender(JNIEnv* env, jobject obj, jlong rendererPtr) {
        VulkanRenderer* renderer = (VulkanRenderer*)rendererPtr;
        if (renderer) renderer->render();
    }
}
