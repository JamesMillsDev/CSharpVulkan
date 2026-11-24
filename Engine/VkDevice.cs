using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

using System.Runtime.InteropServices;

namespace Engine
{
	public unsafe class VkDevice : IDisposable
	{
		public PhysicalDevice Physical { get; }

		private readonly Device device;
		private readonly Vk api;

		public VkDevice(ref VkArrayStrings arrayStrings, VkInstance instance, ref uint queueFamilyIndex)
		{
			api = instance.GetApi();
			
			IReadOnlyCollection<PhysicalDevice> devices = api.GetPhysicalDevices(instance);
			bool foundSuitable = false;

			foreach(PhysicalDevice gpu in devices)
			{
				PhysicalDeviceProperties properties = api.GetPhysicalDeviceProperties(gpu);
				if(properties.DeviceType == PhysicalDeviceType.DiscreteGpu)
				{
					Physical = gpu;
					foundSuitable = true;
				}
			}

			// No suitable GPU's found, so use the first one.
			if(!foundSuitable)
			{
				Physical = devices.First();
			}
			
			List<string> enabledDeviceExtensions = []; //empty for now!
			if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				enabledDeviceExtensions.Add("VK_KHR_portability_subset");
			}

			arrayStrings.ppEnabledDeviceExtensions = new VkArrayStrings.PointerString
			{
				value = (byte**) SilkMarshal.StringArrayToPtr([.. enabledDeviceExtensions]),
				length = (uint) enabledDeviceExtensions.Count
			};
			
			DeviceQueueCreateInfo queueCreateInfo = VkQueue.MakeMainCreateInfo(instance, Physical, out queueFamilyIndex);
			
			DeviceCreateInfo deviceCreateInfo = new()
			{
				SType = StructureType.DeviceCreateInfo,
				EnabledLayerCount = arrayStrings.ppEnabledLayers.length,
				PpEnabledLayerNames = arrayStrings.ppEnabledLayers.value,
				EnabledExtensionCount = (uint) enabledDeviceExtensions.Count,
				PpEnabledExtensionNames = arrayStrings.ppEnabledDeviceExtensions.value,
				QueueCreateInfoCount = 1,
				PQueueCreateInfos = &queueCreateInfo
			};

			if(api.CreateDevice(Physical, &deviceCreateInfo, null, out device) != Result.Success)
			{
				throw new Exception("Failed to create device");
			}
		}

		public void Dispose()
		{
			api.DestroyDevice(device, null);
		}

		public static implicit operator Device(VkDevice device) => device.device;
	}
}