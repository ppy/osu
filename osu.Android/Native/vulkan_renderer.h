#ifndef VULKAN_RENDERER_H
#define VULKAN_RENDERER_H

#include <vulkan/vulkan.h>
#include <vulkan/vulkan_android.h>
#include <jni.h>

class VulkanRenderer {
public:
    VulkanRenderer();
    ~VulkanRenderer();

    bool initialize(ANativeWindow* window);
    void render();
    void cleanup();

private:
    VkInstance instance;
    VkSurfaceKHR surface;
    VkDevice device;
    VkQueue deviceQueue;
    VkSwapchainKHR swapchain;
    // ... more Vulkan members
};

#endif // VULKAN_RENDERER_H
