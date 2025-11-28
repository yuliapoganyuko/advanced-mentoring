using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace CartService.Infrastructure
{
	public class CartRepository : ICartRepository
	{
		private readonly CosmosClient cosmosClient;
		private readonly Container container;

		/// <summary>
		/// Creates a new CartRepository backed by Azure Cosmos DB.
		/// </summary>
		/// <param name="cosmosClient">Configured CosmosClient (DI)</param>
		/// <param name="databaseId">Target database id</param>
		/// <param name="containerId">Target container id (should be partitioned by id)</param>
		public CartRepository(CosmosClient cosmosClient, string databaseId, string containerId)
		{
			this.cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
			if (string.IsNullOrWhiteSpace(databaseId))
				throw new ArgumentException("databaseId is required", nameof(databaseId));
			if (string.IsNullOrWhiteSpace(containerId))
				throw new ArgumentException("containerId is required", nameof(containerId));

			this.container = this.cosmosClient.GetContainer(databaseId, containerId);
		}

		public async Task AddCartItemAsync(Guid cartId, CartItem item)
		{
			if (item == null) 
				throw new ArgumentNullException(nameof(item));

			string id = cartId.ToString();
			try
			{
				var readResponse = await container.ReadItemAsync<Cart>(id, new PartitionKey(id));
				Cart cart = readResponse.Resource ?? new Cart { Id = cartId, Items = new List<CartItem>() };
				cart.Items ??= new List<CartItem>();
				cart.Items.Add(item);
				await container.ReplaceItemAsync(cart, id, new PartitionKey(id));
			}
			catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
			{
				var newCart = new Cart
				{
					Id = cartId,
					Items = new List<CartItem> { item }
				};
				await container.CreateItemAsync(newCart, new PartitionKey(id));
			}
		}

		public async Task<IEnumerable<CartItem>?> GetCartItemsAsync(Guid cartId)
		{
			string id = cartId.ToString();
			try
			{
				var readResponse = await container.ReadItemAsync<Cart>(id, new PartitionKey(id));
				return readResponse.Resource?.Items;
			}
			catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
			{
				return null;
			}
		}
		
		public async Task UpdateItemsOnProductChangedAsync(int productId, string productName, string? productImageUrl, decimal productPrice)
		{
			var iterator = container.GetItemQueryIterator<Cart>();
			while (iterator.HasMoreResults)
			{
				foreach (var cart in await iterator.ReadNextAsync())
				{
					if (cart == null)
						continue;

					bool updated = false;
					if (cart.Items != null)
					{
						foreach (var item in cart.Items.Where(i => i.Id == productId))
						{
							item.Name = productName;
							item.ImageUri = productImageUrl;
							item.Price = productPrice;
							updated = true;
						}
					}
					if (updated)
					{
						await container.ReplaceItemAsync(cart, cart.Id.ToString(), new PartitionKey(cart.Id.ToString()));
					}
				}
			}
		}

		public async Task<bool> RemoveCartItemAsync(Guid cartId, int itemId)
		{
			string id = cartId.ToString();
			try
			{
				var readResponse = await container.ReadItemAsync<Cart>(id, new PartitionKey(id));
				Cart cart = readResponse.Resource;
				if (cart == null || cart.Items == null)
					return false;

				var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
				if (item == null)
					return false;

				cart.Items.Remove(item);
				ItemResponse<Cart> replaceResponse = await container.ReplaceItemAsync(cart, id, new PartitionKey(id));
				return replaceResponse.StatusCode == HttpStatusCode.OK || replaceResponse.StatusCode == HttpStatusCode.Created;
			}
			catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
			{
				return false;
			}
		}
	}
}
