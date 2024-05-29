using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class UnityVulkan : MonoBehaviour
{
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetVkPipelineCache();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetVkInstance();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetVkPhysicalDevice();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetVkDevice();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetVkQueue();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetPFNvkGetInstanceProcAddr();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint GetQueueFamilyIndex();

    public IntPtr vkPipelineCache { get; private set; }
    public IntPtr vkInstance { get; private set; }
    public IntPtr vkPhysicalDevice { get; private set; }
    public IntPtr vkDevice { get; private set; }
    public IntPtr vkQueue { get; private set; }
    public IntPtr pfnvkGetInstanceProcAddr { get; private set; }
    public uint queueFamilyIndex { get; private set; }

    void Start()
    {
        vkPipelineCache = GetVkPipelineCache();
        ConfigManager.WriteConsole($"[UnityVulkan] vkPipelineCache {vkPipelineCache.ToString("x16")}");
        
        vkInstance = GetVkInstance();
        ConfigManager.WriteConsole($"[UnityVulkan] vkInstance {vkInstance.ToString("x16")}");
        
        vkPhysicalDevice = GetVkPhysicalDevice();
        ConfigManager.WriteConsole($"[UnityVulkan] vkPhysicalDevice {vkPhysicalDevice.ToString("x16")}");
        
        vkDevice = GetVkDevice();
        ConfigManager.WriteConsole($"[UnityVulkan] vkDevice {vkDevice.ToString("x16")}");
        
        vkQueue = GetVkQueue();
        ConfigManager.WriteConsole($"[UnityVulkan] vkQueue {vkQueue.ToString("x16")}");
        
        pfnvkGetInstanceProcAddr = GetPFNvkGetInstanceProcAddr();
        ConfigManager.WriteConsole($"[UnityVulkan] pfnvkGetInstanceProcAddr {pfnvkGetInstanceProcAddr.ToString("x16")}");
        
        queueFamilyIndex = GetQueueFamilyIndex();
        ConfigManager.WriteConsole($"[UnityVulkan] queueFamilyIndex {queueFamilyIndex}");

        LibretroVulkan.Init(vkInstance, vkPhysicalDevice, vkDevice, pfnvkGetInstanceProcAddr);
    }
}
