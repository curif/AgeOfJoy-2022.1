using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class UnityVulkan : MonoBehaviour
{
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetVkPipelineCache();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetVkInstance();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetVkPhysicalDevice();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetVkDevice();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetVkQueue();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetPFNvkGetInstanceProcAddr();
    [DllImport("libVulkanPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetQueueFamilyIndex();

    public IntPtr vkPipelineCache { get; private set; }
    public IntPtr vkInstance { get; private set; }
    public IntPtr vkPhysicalDevice { get; private set; }
    public IntPtr vkDevice { get; private set; }
    public IntPtr vkQueue { get; private set; }
    public IntPtr pfnvkGetInstanceProcAddr { get; private set; }
    public int queueFamilyIndex { get; private set; }

    void Start()
    {
        vkPipelineCache = (IntPtr)GetVkPipelineCache();
        ConfigManager.WriteConsole($"[UnityVulkan] vkPipelineCache {vkPipelineCache}");
        
        vkInstance = (IntPtr)GetVkInstance();
        ConfigManager.WriteConsole($"[UnityVulkan] vkInstance {vkInstance}");
        
        vkPhysicalDevice = (IntPtr)GetVkPhysicalDevice();
        ConfigManager.WriteConsole($"[UnityVulkan] vkPhysicalDevice {vkPhysicalDevice}");
        
        vkDevice = (IntPtr)GetVkDevice();
        ConfigManager.WriteConsole($"[UnityVulkan] vkPhysicalDevice {vkPhysicalDevice}");
        
        vkQueue = (IntPtr)GetVkQueue();
        ConfigManager.WriteConsole($"[UnityVulkan] vkQueue {vkQueue}");
        
        pfnvkGetInstanceProcAddr = (IntPtr)GetPFNvkGetInstanceProcAddr();
        ConfigManager.WriteConsole($"[UnityVulkan] pfnvkGetInstanceProcAddr {pfnvkGetInstanceProcAddr}");
        
        queueFamilyIndex = GetQueueFamilyIndex();
        ConfigManager.WriteConsole($"[UnityVulkan] queueFamilyIndex {queueFamilyIndex}");
    }
}
