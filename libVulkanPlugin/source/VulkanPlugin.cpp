#include <iostream>
#include <vulkan/vulkan.h>
#include "IUnityLog.h"
#include "IUnityInterface.h"
#include "IUnityGraphics.h"
#include "IUnityGraphicsVulkan.h"

// Unity interface for Vulkan
static IUnityLog* unityLogPtr = nullptr;
static IUnityGraphicsVulkan* s_UnityVulkan = nullptr;
static UnityVulkanInstance s_VulkanInstance = {
};

#define log(a) std::cout << a << std::endl;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces * unityInterfaces)
{
	unityLogPtr = unityInterfaces->Get<IUnityLog>();
	log("UnityPluginLoad");
	s_UnityVulkan = unityInterfaces->Get<IUnityGraphicsVulkan>();

	if (s_UnityVulkan)
	{
		s_VulkanInstance = s_UnityVulkan->Instance();
	}

	log("UnityPluginLoad OK");
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	log("UnityPluginUnload");
}


// Expose Vulkan to the world !

extern "C" VkPipelineCache UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetVkPipelineCache()
{
	log("GetVkPipelineCache");
	return s_VulkanInstance.pipelineCache;
}

extern "C" VkInstance UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetVkInstance()
{
	log("GetVulkanInstance");
	return s_VulkanInstance.instance;
}

extern "C" VkPhysicalDevice UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetVkPhysicalDevice()
{
	log("GetVkPhysicalDevice");
	return s_VulkanInstance.physicalDevice;
}

extern "C" VkDevice UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetVkDevice()
{
	log("GetVkDevice");
	return s_VulkanInstance.device;
}

extern "C" VkQueue UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetVkQueue()
{
	log("GetVkQueue");
	return s_VulkanInstance.graphicsQueue;
}

extern "C" PFN_vkGetInstanceProcAddr UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetPFNvkGetInstanceProcAddr()
{
	log("GetPFNvkGetInstanceProcAddr");
	return s_VulkanInstance.getInstanceProcAddr;
}

extern "C" unsigned int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetQueueFamilyIndex()
{
	log("GetQueueFamilyIndex");
	return s_VulkanInstance.queueFamilyIndex;
}
