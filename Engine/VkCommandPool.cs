using Silk.NET.Vulkan;

namespace Engine
{
	public unsafe class VkCommandPool : IDisposable
	{
		private readonly CommandPool commandPool;

		private readonly Vk api;
		private readonly Device device;

		public VkCommandPool(VkInstance instance, VkDevice device, VkQueue queue)
		{
			api = instance.GetApi();
			this.device = device;
			
			CommandPoolCreateInfo commandPoolCreateInfo = new()
			{
				SType = StructureType.CommandPoolCreateInfo,
				QueueFamilyIndex = queue.index,
				Flags = CommandPoolCreateFlags.TransientBit | CommandPoolCreateFlags.ResetCommandBufferBit
			};

			if(api.CreateCommandPool(this.device, &commandPoolCreateInfo, null, out commandPool) != Result.Success)
			{
				throw new Exception("Could not create command pool");
			}
		}

		public void Dispose()
		{
			api.DestroyCommandPool(device, commandPool, null);
		}

		public Result Submit(SubmitInfo submitInfo, Fence fence, VkQueue queue)
		{
			return api.QueueSubmit(queue.queue, 1, &submitInfo, fence);
		}
	}
}