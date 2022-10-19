﻿using Newtonsoft.Json;
using PlantHealthAppXam.Models;
using PlantHealthAppXam.Views;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlantHealthAppXam.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private PlantHealthDeatils _selectedItem;

        public ObservableCollection<PlantHealthDeatils> ItemsList { get; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command<PlantHealthDeatils> ItemTapped { get; }

        public ItemsViewModel()
        {
            Title = "Browse";
            ItemsList = new ObservableCollection<PlantHealthDeatils>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<PlantHealthDeatils>(OnItemSelected);
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                ItemsList.Clear();
                var items = await GetDataAsync().ConfigureAwait(false);
                foreach (var item in items)
                {
                    ItemsList.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public PlantHealthDeatils SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async void OnItemSelected(PlantHealthDeatils item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.IMGURL)}={item.ImageURL}");
        }

        public async Task<List<PlantHealthDeatils>> GetDataAsync()
        {
            var client = new RestClient("http://localhost:7082/api/GetPlantHealth");
            var request = new RestRequest();
            request.Method = Method.Get;
            var response = await client.ExecuteAsync(request);
            return JsonConvert.DeserializeObject<List<PlantHealthDeatils>>(response.Content.ToString());
        }

    }
}