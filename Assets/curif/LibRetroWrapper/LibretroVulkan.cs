using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static unsafe class LibretroVulkan
{
    static IntPtr vkImage;
    static bool vkImageReady;

    static IntPtr vkInstance;
    static IntPtr vkPhysicalDevice;
    static IntPtr vkDevice;
    static IntPtr pfnvkGetInstanceProcAddr;
    static VulcanImageCBHandler vulcanImageCBHandler;

    // vulkan
    private delegate void VulcanImageCBHandler(IntPtr vkImage);

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_init_vulkan(IntPtr vkInstance, IntPtr vkPhysicalDevice, IntPtr vkDevice, IntPtr vkGetInstanceProcAddr, VulcanImageCBHandler vulcanImageCBHandler);

    public static void Init(IntPtr _vkInstance, IntPtr _vkPhysicalDevice, IntPtr _vkDevice, IntPtr _pfnvkGetInstanceProcAddr)
    {
        ConfigManager.WriteConsole("[LibretroVulkan.Init]" +
            $" vkInstance:{_vkInstance.ToString("x16")}" +
            $" vkPhysicalDevice:{_vkPhysicalDevice.ToString("x16")}" +
            $" vkDevice:{_vkDevice.ToString("x16")}" +
            $" pfnvkGetInstanceProcAddr:{_pfnvkGetInstanceProcAddr.ToString("x16")}");
        vkInstance = _vkInstance;
        vkPhysicalDevice = _vkPhysicalDevice;
        vkDevice = _vkDevice;
        pfnvkGetInstanceProcAddr = _pfnvkGetInstanceProcAddr;
        vulcanImageCBHandler = new VulcanImageCBHandler(VulcanImageCB);
        vkImage = IntPtr.Zero;
        vkImageReady = false;
    }

    public static void WrapperInit()
    {
        ConfigManager.WriteConsole("[LibretroVulkan.WrapperInit]" +
            $" vkInstance:{vkInstance.ToString("x16")}" +
            $" vkPhysicalDevice:{vkPhysicalDevice.ToString("x16")}" +
            $" vkDevice:{vkDevice.ToString("x16")}" +
            $" pfnvkGetInstanceProcAddr:{pfnvkGetInstanceProcAddr.ToString("x16")}");
        wrapper_init_vulkan(vkInstance, vkPhysicalDevice, vkDevice, pfnvkGetInstanceProcAddr, vulcanImageCBHandler);
    }
    public static bool isVkImageReady()
    {
        return vkImageReady;
    }

    public static IntPtr GetVkImage()
    {
        ConfigManager.WriteConsole($"[LibretroVulkan.GetVkImage] {vkImage.ToString("x16")}");
        vkImageReady = false;
        return vkImage;
    }

    [AOT.MonoPInvokeCallback(typeof(VulcanImageCBHandler))]
    static void VulcanImageCB(IntPtr _vkImage)
    {
        ConfigManager.WriteConsole($"[LibretroVulkan.VulcanImageCB] received {_vkImage.ToString("x16")}");
        vkImage = _vkImage;
        vkImageReady = true;
    }
}
