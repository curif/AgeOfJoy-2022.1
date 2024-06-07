LOCAL_PATH := $(call my-dir)
SRC_DIR := ../../../source

include $(CLEAR_VARS)
LOCAL_MODULE := VulkanPlugin
LOCAL_C_INCLUDES += $(SRC_DIR)
LOCAL_LDLIBS := -llog
LOCAL_ARM_MODE := arm

LOCAL_SRC_FILES += $(SRC_DIR)/VulkanPlugin.cpp

# build
include $(BUILD_SHARED_LIBRARY)
