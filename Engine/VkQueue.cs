using Silk.NET.Vulkan;

namespace Engine
{
	public unsafe class VkQueue(uint index, Queue queue)
	{
		public static DeviceQueueCreateInfo MakeMainCreateInfo(VkInstance instance, PhysicalDevice device, out uint queueFamilyIndex)
		{
			Vk api = instance.GetApi();
			
			uint queueFamilyCount = 0u;
			api.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilyCount);
			
			QueueFamilyProperties[] queueFamilies  = new QueueFamilyProperties[queueFamilyCount];
			fixed(QueueFamilyProperties* propertiesPtr = queueFamilies )
			{
				api.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilyCount, propertiesPtr);
			}

			queueFamilyIndex = 0u;
			for (uint i = 0u; i < queueFamilies.Length; i++)
			{
				if (queueFamilies[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit) ||
				    queueFamilies[i].QueueFlags.HasFlag(QueueFlags.ComputeBit))
				{
					queueFamilyIndex = i;
					break;
				}
			}
			
			float defaultPriority = 1f;
			return new DeviceQueueCreateInfo
			{
				SType = StructureType.DeviceQueueCreateInfo,
				QueueCount = 1,
				QueueFamilyIndex = queueFamilyIndex,
				PQueuePriorities = &defaultPriority,
			};
		}
			
		public readonly uint index = index;
		public readonly Queue queue = queue;
	}
}