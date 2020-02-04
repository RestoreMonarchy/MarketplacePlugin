using Marketplace.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Marketplace.Client.Pages
{
    public partial class Index
    {
        [Inject]
        private HttpClient HttpClient { get; set; }

        private List<UnturnedItem> items;
        private List<UnturnedItem> searchItems => items.Where(x => x.ItemId.ToString()
            .Equals(searchString) || x.ItemName.ToLower().Contains(searchString)).ToList();

        string searchString = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            items = await HttpClient.GetJsonAsync<List<UnturnedItem>>("api/unturneditems");
        }
    }
}
