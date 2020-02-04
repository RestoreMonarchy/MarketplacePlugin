using Marketplace.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Marketplace.Client.Pages
{
    [Authorize]
    public partial class TrunkPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AuthenticationStateProvider stateProvider { get; set; }
        private AuthenticationState state { get; set; }

        public List<MarketItem> Items { get; set; }

        private List<MarketItem> sellingItems => Items.Where(x => x.SellerId == state.User.Identity.Name && !x.IsSold).ToList();
        private List<MarketItem> boughtItems => Items.Where(x => x.BuyerId == state.User.Identity.Name && !x.IsClaimed).ToList();

        protected override async Task OnInitializedAsync()
        {
            state = await stateProvider.GetAuthenticationStateAsync();
            Items = await HttpClient.GetJsonAsync<List<MarketItem>>("api/marketitems/my");
        }
    }
}
