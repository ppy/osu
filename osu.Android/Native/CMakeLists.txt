# Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
# See the LICENCE file in the repository root for full licence text.

cmake_minimum_required(VERSION 3.22.1)
project(osu.Android.Native)

include(FetchContent)

FetchContent_Declare(
    oboe
    GIT_REPOSITORY https://github.com/google/oboe
    GIT_TAG 1.10.0
)

FetchContent_MakeAvailable(oboe)

add_library(osu.Android.Native SHARED
            vulkan_renderer.cpp
            oboe_audio.cpp)

find_library(log-lib log)
find_library(android-lib android)
find_library(vulkan-lib vulkan)

target_include_directories(osu.Android.Native PRIVATE .)

target_link_libraries(osu.Android.Native
                      oboe
                      ${log-lib}
                      ${android-lib}
                      ${vulkan-lib})
