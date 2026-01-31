cmake_minimum_required(VERSION 3.10.2)
project(osu.Android.Native)

add_library(osu.Android.Native SHARED
            vulkan_renderer.cpp
            oboe_audio.cpp)

find_library(log-lib log)
find_library(android-lib android)
find_library(vulkan-lib vulkan)

# Oboe needs to be added as a dependency.
# In a real project, you'd use FetchContent or a prebuilt version.
# For now, we assume Oboe is available in the include path.

target_include_directories(osu.Android.Native PRIVATE .)

target_link_libraries(osu.Android.Native
                      ${log-lib}
                      ${android-lib}
                      ${vulkan-lib})
