﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Helper;
using WeatherApp.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WeatherApp.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainWeather : ContentPage
    {
        public MainWeather()
        {
            InitializeComponent();
            GetCurrentCoordinates();
        }

        private string Location { get; set; } = "Ireland";
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        private async void GetCurrentCoordinates()
        {
            try
            {
                //request users location
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var location = await Geolocation.GetLocationAsync(request);

                //check if users location is not null
                if(location != null)
                {
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;

                    Location = await GetCity(location);

                    GetWeatherInfo();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private async Task<string> GetCity(Location location)
        {
            var places = await Geocoding.GetPlacemarksAsync(location);
            var currentPlace = places?.FirstOrDefault();

            if(currentPlace != null)
            {
                //returns current place location
                return $"{currentPlace.Locality},{currentPlace.CountryName}";
            }
            return null;


        }

        //gets background for the app using a api
        private async void GetBackground()
        {
            var url = $"https://api.pexels.com/v1/search?query={Location}&per_page=15&page=1";
            //api key for pexels 
            var result = await CallingApi.Get(url, "563492ad6f917000010000019b48e46cdf5a4589a6a3b5cf048cd71e");

            if (result.Successful)
            {
                var imageInfo = JsonConvert.DeserializeObject<BackgroundInfo>(result.Response);
                if(imageInfo !=  null && imageInfo.photos.Length > 0)
                {
                    //randoms image everytime
                    bgImg.Source = ImageSource.FromUri(new Uri(imageInfo.photos[new Random().Next(0, imageInfo.photos.Length - 1)].src.medium));
                }
            }
        }

        private async void GetWeatherInfo()
        {
            var url = $"http://api.openweathermap.org/data/2.5/weather?q={Location}&appid=2f2f3c09845d9e9d7f35bd77245ac92e&units=metric";
            var result = await CallingApi.Get(url);

            if (result.Successful)
            {
                try
                {
                    var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);
                    descriptionTxt.Text = weatherInfo.weather[0].description.ToUpper();
                    iconImg.Source = $"w{weatherInfo.weather[0].icon}";
                    cityTxt.Text = weatherInfo.name.ToUpper();
                    temperatureTxt.Text = weatherInfo.main.temp.ToString("0");
                    humidityTxt.Text = $"{weatherInfo.main.humidity}%";
                    pressureTxt.Text = $"{weatherInfo.main.pressure} hpa";
                    windTxt.Text = $"{weatherInfo.wind.speed} m/s";
                    cloudinessTxt.Text = $"{weatherInfo.clouds.all}%";

                    var dt = new DateTime().ToUniversalTime().AddSeconds(weatherInfo.dt);
                    dateTxt.Text = dt.ToString("dddd, MMM dd").ToUpper();

                    GetWeatherForecast();
                    GetBackground();
                }
                catch (Exception e)
                {
                    await DisplayAlert("Weather Info", e.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Weather Info", "No weather information found", "OK");
            }
        }

        //gets the weather forecast
        private async void GetWeatherForecast()
        {
            var url = $"http://api.openweathermap.org/data/2.5/weather?q={Location}&appid=2f2f3c09845d9e9d7f35bd77245ac92e&units=metric";
            var result = await CallingApi.Get(url);

            if (result.Successful)
            {
                try
                {

                    var forecastInfo = JsonConvert.DeserializeObject<ForecastInfo>(result.Response);
                    List<List> allList = new List<List> ();
                    foreach(var list in forecastInfo.list)
                    {
                        //var date = DateTime.ParseExact(list.dt_txt, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
                        var date = DateTime.Parse(list.dt_txt);

                        if (date > DateTime.Now && date.Hour == 0 && date.Minute == 0 && date.Second == 0)
                            allList.Add(list);
                    }

                    dayOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dddd");
                    dateOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dd MMM");
                    iconOneImg.Source = $"w{allList[0].weather[0].icon}";
                    tempOneTxt.Text = allList[0].main.temp.ToString("0");

                    dayTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dddd");
                    dateTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dd MMM");
                    iconTwoImg.Source = $"w{allList[1].weather[0].icon}";
                    tempTwoTxt.Text = allList[1].main.temp.ToString("0");

                    dayThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dddd");
                    dateThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dd MMM");
                    iconThreeImg.Source = $"w{allList[2].weather[0].icon}";
                    tempThreeTxt.Text = allList[2].main.temp.ToString("0");

                    dayFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dddd");
                    dateFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dd MMM");
                    iconFourImg.Source = $"w{allList[3].weather[0].icon}";
                    tempFourTxt.Text = allList[3].main.temp.ToString("0");
                }
                catch(Exception e)
                {
                    await DisplayAlert("Weather Info", e.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Weather Info", "No forecast information found", "OK");
            }



        }
    }
}