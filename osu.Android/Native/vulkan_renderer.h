// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#ifndef VULKAN_RENDERER_H
#define VULKAN_RENDERER_H

#include <vulkan/vulkan.h>
#include <vulkan/vulkan_android.h>
#include <jni.h>
#include <android/native_window.h>
#include <vector>

class VulkanRenderer {
public:
    struct ShaderConstants {
        float resolutionScale = 1.0f;
    };

    VulkanRenderer();
    ~VulkanRenderer();

    bool initialize(ANativeWindow* window);
    void render();
    void cleanup();

private:
    VkInstance instance;
    VkSurfaceKHR surface;
    VkPhysicalDevice physicalDevice;
    VkDevice device;
    VkQueue deviceQueue;
    VkSwapchainKHR swapchain;
    VkRenderPass renderPass;
    VkPipelineLayout pipelineLayout;
    VkPipelineCache pipelineCache;
    VkCommandPool commandPool;
    std::vector<VkCommandBuffer> commandBuffers;

    // Synchronization
    std::vector<VkSemaphore> imageAvailableSemaphores;
    std::vector<VkSemaphore> renderFinishedSemaphores;
    std::vector<VkFence> inFlightFences;
    uint32_t currentFrame;
    const int MAX_FRAMES_IN_FLIGHT = 1;

    // Feature Flags
    uint32_t apiVersion;
    bool hasVulkan14;
    bool hasVulkan13;
    bool hasTimelineSemaphores;
    bool hasDynamicRendering;
    bool has16BitStorage;
    bool hasHostQueryReset;
    bool hasLowLatency2;

    uint32_t findMemoryType(uint32_t typeFilter, VkMemoryPropertyFlags properties);
    void createRenderPass();
    void createPipelineLayout();
    void createPipelineCache();
    void createSyncObjects();
    void createCommandPool();
    void createCommandBuffers();
    void recordCommandBuffer(VkCommandBuffer commandBuffer, uint32_t imageIndex);
    void createBuffer(VkDeviceSize size, VkBufferUsageFlags usage, VkMemoryPropertyFlags properties, VkBuffer& buffer, VkDeviceMemory& bufferMemory);
    void queryFeatures();
    bool checkDeviceExtensionSupport(VkPhysicalDevice device, const char* extensionName);
};

#endif // VULKAN_RENDERER_H
