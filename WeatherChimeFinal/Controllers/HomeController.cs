using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using WeatherChimeFinal.Models;

namespace WeatherChimeFinal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpPost]
        public JsonResult AutoComp(string searchText)
        {
            var result = "";

            var param = "aq?query=" + searchText;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://autocomplete.wunderground.com/");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(param).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                result = response.Content.ReadAsStringAsync().Result;

                //foreach (var d in dataObjects)
                //{
                //    Console.WriteLine("{0}", d.Name);
                //}
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

           return Json(result);
        }

        [HttpGet]
        public FileResult GetPhoto(string request, string weatherEvent)
        {
            Byte[] b;
            if (weatherEvent != null)
            {
                b = GetPhotoByWeatherEvent(weatherEvent);
            }
            else
            {
                b = GetPhotoByZmw(request);
            }

            return File(b, "image/jpeg");
        }

        public Byte[] GetPhotoByZmw(string request)
        {
            var param = "api/4b22bd927f7356b7/satellite/q/zmw:" + request + ".json";

            var model = new PerformanceViewModel();
            string imgUrl = "";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://api.wunderground.com/");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(param).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {

                string text = response.Content.ReadAsStringAsync().Result;
                JObject responseJObject = JObject.Parse(text);


                imgUrl = responseJObject["satellite"]["image_url"].ToString();
                //imgUrl = imgUrl.Replace("height=300", "height=600");
                //imgUrl = imgUrl.Replace("width=300", "width=600");
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }


            var webClient = new WebClient();

            Byte[] b = webClient.DownloadData(imgUrl);
            return b;
        }

        public Byte[] GetPhotoByWeatherEvent(string weatherEvent)
        {
            
            Byte[] b = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/Hurricane" + weatherEvent + ".jpg"));
            
            return b;
        }

        public ActionResult Performance(string request, string weatherEvent)
        {
            var model = new PerformanceViewModel();

             if (weatherEvent != null) {
                model = GetDataByWeatherEvent(weatherEvent);
            }
            else
            {
                model = GetDataByZmw(request);
            }
            return View(model);
        } 

        public PerformanceViewModel GetDataByZmw(string request)
        {
            var param = "api/4b22bd927f7356b7/conditions/q/zmw:" + request + ".json";


            var model = new PerformanceViewModel();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://api.wunderground.com/");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(param).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {

                string text = response.Content.ReadAsStringAsync().Result;
                JObject responseJObject = JObject.Parse(text);

                if (responseJObject["current_observation"] != null)
                {

                    model.Temp = responseJObject["current_observation"]["temp_c"].ToString();
                    model.Humidity = responseJObject["current_observation"]["relative_humidity"].ToString().Replace("%", "");
                    model.Pressure = responseJObject["current_observation"]["pressure_mb"].ToString();
                    model.WindSpeed = responseJObject["current_observation"]["wind_kph"].ToString();
                    model.Location = responseJObject["current_observation"]["display_location"]["full"].ToString();

                }
                //model.Humidity = xmlDoc

                //foreach (var d in dataObjects)
                //{
                //    Console.WriteLine("{0}", d.Name);
                //}
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return model;
        }

        public PerformanceViewModel GetDataByWeatherEvent(string weatherEvent)
        {

            var model = new PerformanceViewModel();

            if (weatherEvent == "Andrew")
            {
                model.Temp = "21";
                model.Humidity = "100";
                model.Pressure = "922";
                model.WindSpeed = "280";
                model.Location = "Hurricane Andrew 08/24/1992";
            }
            return model;
        }


        public ActionResult Viz()
        {
            return View();
        }
    }
}
