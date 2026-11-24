using System.Runtime.InteropServices;

using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

namespace Engine
{
	public unsafe class VkContext : IDisposable
	{
		private static List<string> EnabledInstanceExtensions
		{
			get
			{
				List<string> enabledInstanceExtensions = [ExtDebugUtils.ExtensionName];
				if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					enabledInstanceExtensions.Add("VK_LAYER_portability_enumeration");
				}
				
				return enabledInstanceExtensions;
			}
		}
		
		private static List<string> EnabledLayers => ["VK_LAYER_KHRONOS_validation"];
		
		private readonly VkInstance? instance;
		private readonly VkDevice? device;
		private readonly VkQueue? mainQueue;
		private readonly VkCommandPool? commandPool;
		
		private readonly Queue<IDisposable> disposables = new();

		public VkContext()
		{
			try
			{
				VkArrayStrings arrayStrings = new(EnabledLayers, EnabledInstanceExtensions);
				
				instance = QueueDestruction(new VkInstance(ref arrayStrings));
				
				uint mainQueueIndex = 0u;
				device = QueueDestruction(new VkDevice(ref arrayStrings, instance, ref mainQueueIndex));

				instance.GetApi().GetDeviceQueue(device, mainQueueIndex, 0, out Queue mainVkQueue);
				mainQueue = new VkQueue(mainQueueIndex, mainVkQueue);
				
				commandPool = QueueDestruction(new VkCommandPool(instance, device, mainQueue));
				
				arrayStrings.Dispose();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public void Dispose()
		{
			while(disposables.Count > 0)
			{
				disposables.Dequeue().Dispose();
			}
		}

		private T QueueDestruction<T>(T resource) where T : IDisposable
		{
			disposables.Enqueue(resource);
			return resource;
		}
	}
}