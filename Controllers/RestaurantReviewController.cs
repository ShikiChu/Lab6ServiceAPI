using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Xml.Serialization;
using Lab6ServiceAPI.Models;
using System.Collections.Generic;
using System.Globalization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Lab6ServiceAPI.Controllers
{
    [EnableCors]
    [Route("[controller]")]
    [ApiController]
    public class RestaurantReviewController : ControllerBase
    {
        // get all the reviews
        [HttpGet]
        public List<RestaurantInfo> Get()
        {
            //read xml
            restaurant_reviews allReviews = GetRestaurantReviewsFromXml();

            List<RestaurantInfo> restaurantInfo = new List<RestaurantInfo>();
            int i = 0;
            foreach (restaurant_reviewsRestaurant restaurant in allReviews.restaurant) 
            {
                //call to create instances
                RestaurantInfo EachResInfo = GetRestaurantInfo(restaurant);
                EachResInfo.id = i;
                restaurantInfo.Add(EachResInfo);
                i++;

            }
            return restaurantInfo;
        }

        // GET api/<RestaurantReviewController>/5
        [HttpGet("{id}")]
        public RestaurantInfo Get(int id)
        {
            //read xml
            restaurant_reviews allReviews = GetRestaurantReviewsFromXml();

            List<RestaurantInfo> restaurantInfo = new List<RestaurantInfo>();
            int i = 0;
            foreach (restaurant_reviewsRestaurant restaurant in allReviews.restaurant)
            {
                //call to create instances
                RestaurantInfo EachResInfo = GetRestaurantInfo(restaurant);
                EachResInfo.id = i;
                restaurantInfo.Add(EachResInfo);
                i++;
            }
            return restaurantInfo[id];

        }

        [Route("[action]")]
        [HttpGet]
        public List<string> GetRestaurantNames()
        {
            //read xml
            restaurant_reviews allReviews = GetRestaurantReviewsFromXml();
            List<string> listOfNames = new List<string>();
            foreach (restaurant_reviewsRestaurant restaurant in allReviews.restaurant)
            {
                //call to create instances
                RestaurantInfo EachResInfo = GetRestaurantInfo(restaurant);
                string name = EachResInfo.name;
                listOfNames.Add(name);
            }
            return listOfNames;

        }

        //POST<RestaurantReviewController>
       [HttpPost]
       [EnableCors]
        public void Post([FromBody] RestaurantInfo restInfo)
        {
            //read xml
            restaurant_reviews allReviews = GetRestaurantReviewsFromXml();

            //new instance of restaurant
            restaurant_reviewsRestaurant newRestaurant = GetNewRestaurantWithRestaurantInfo(restInfo);

            //append to exisiting data
            //List<restaurant_reviewsRestaurant> restaurantList = new List<restaurant_reviewsRestaurant>(allReviews.restaurant)
            //{
            //    newRestaurant
            //};
            List<restaurant_reviewsRestaurant> restaurantList = new List<restaurant_reviewsRestaurant>(allReviews.restaurant);
            restaurantList.AddRange(new[] { newRestaurant });
            allReviews.restaurant = restaurantList.ToArray();

            // Save the updated 
            SaveRestaurantReviewsToXml(allReviews);

        }


        // PUT api/<RestaurantReviewController>
        [HttpPut]
        [EnableCors]
        public void Put([FromBody] RestaurantInfo restInfo)
        {
            // Read file
            restaurant_reviews allReviews = GetRestaurantReviewsFromXml();

            //restaurant to be updated
            restaurant_reviewsRestaurant updateRes = allReviews.restaurant[restInfo.id];

            // Update the restaurant
            UpdateRestaurantWithRestaurantInfo(updateRes, restInfo);

            SaveRestaurantReviewsToXml(allReviews);

        }

        // DELETE api/<RestaurantReviewController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            // Read file
            restaurant_reviews allReviews = GetRestaurantReviewsFromXml();

            //restaurant to be updated
            restaurant_reviewsRestaurant DeleteRes = allReviews.restaurant[id];

            List<restaurant_reviewsRestaurant> restaurantList = allReviews.restaurant.ToList();
            restaurantList.Remove(DeleteRes);
            allReviews.restaurant = restaurantList.ToArray();

            SaveRestaurantReviewsToXml(allReviews);
        }

        private restaurant_reviews GetRestaurantReviewsFromXml()
        {
            restaurant_reviews reviews = null;

            string xmlPath = Path.GetFullPath("Data/restaurant_review.xml");

            using (FileStream xs = new FileStream(xmlPath, FileMode.Open))
            {
                XmlSerializer serializor = new XmlSerializer(typeof(restaurant_reviews));
                reviews = serializor.Deserialize(xs) as restaurant_reviews;
            }
            return reviews;
        }


        private void SaveRestaurantReviewsToXml(restaurant_reviews reviews)
        {
            string xmlFilePath = Path.GetFullPath("Data/restaurant_review.xml");
            using (FileStream xs = new FileStream(xmlFilePath, FileMode.Create))
            {
                XmlSerializer serializor = new XmlSerializer(typeof(restaurant_reviews));
                serializor.Serialize(xs, reviews);
            }

        }

        private RestaurantInfo GetRestaurantInfo(restaurant_reviewsRestaurant rs)
        {
            RestaurantInfo rsInfo = new RestaurantInfo();
            rsInfo.address = new Address();
            rsInfo.rating = new Rating();
            rsInfo.cost = new Cost();

            rsInfo.name = rs.name;
            rsInfo.address.street = rs.address.street_address;
            rsInfo.address.city = rs.address.city;
            rsInfo.address.provstate = rs.address.state_province.ToString();
            rsInfo.address.postalzipcode = rs.address.zip_postal_code;
            rsInfo.summary = rs.summary;
            rsInfo.foodType = rs.food_type;
            rsInfo.rating.currentRating = rs.rating.Value;
            rsInfo.rating.minRating = rs.rating.min;
            rsInfo.rating.maxRating = rs.rating.max;
            rsInfo.cost.currentCost = rs.cost.Value;
            rsInfo.cost.minCost = rs.cost.min;
            rsInfo.cost.maxCost = rs.cost.max;

            return rsInfo;
        }

        private void UpdateRestaurantWithRestaurantInfo(restaurant_reviewsRestaurant rest, RestaurantInfo restInfo)
        {
            if (!string.IsNullOrEmpty(restInfo.name)) rest.name = restInfo.name;

            if (restInfo.address != null)
            {
                if (!string.IsNullOrEmpty(restInfo.address.street))
                    rest.address.street_address = restInfo.address.street;

                if (!string.IsNullOrEmpty(restInfo.address.city))
                    rest.address.city = restInfo.address.city;

                if (!string.IsNullOrEmpty(restInfo.address.provstate))
                    rest.address.state_province = Enum.Parse<StateProvinceType>(restInfo.address.provstate);

                if (!string.IsNullOrEmpty(restInfo.address.postalzipcode))
                    rest.address.zip_postal_code = restInfo.address.postalzipcode;
            }

            if (!string.IsNullOrEmpty(restInfo.summary)) rest.summary = restInfo.summary;

            if (!string.IsNullOrEmpty(restInfo.foodType)) rest.food_type = restInfo.foodType;

            if (restInfo.rating != null)
            {
                rest.rating.Value = (byte)restInfo.rating.currentRating;
            }

            if (restInfo.cost != null)
            {
                rest.cost.Value = (byte)restInfo.cost.currentCost;
            }
        }
        private restaurant_reviewsRestaurant GetNewRestaurantWithRestaurantInfo(RestaurantInfo restInfo)
        {
            restaurant_reviewsRestaurant rest = new restaurant_reviewsRestaurant();

            rest.name = restInfo.name;

            if (restInfo.address != null)
            {
                rest.address = new address();
                rest.address.street_address = restInfo.address.street;
                rest.address.city = restInfo.address.city;
                rest.address.state_province = Enum.Parse<StateProvinceType>(restInfo.address.provstate);
                rest.address.zip_postal_code = restInfo.address.postalzipcode;
            }
            rest.summary = restInfo.summary;
            rest.food_type = restInfo.foodType;

            if (restInfo.rating != null)
            {
                rest.rating = new RangeType();
                rest.rating.Value = (byte)restInfo.rating.currentRating;
                rest.rating.min = (byte)restInfo.rating.minRating;
                rest.rating.max = (byte)restInfo.rating.maxRating;
            }

            if (restInfo.cost != null)
            {
                rest.cost = new RangeType();
                rest.cost.Value = (byte)restInfo.cost.currentCost;
                rest.cost.min = (byte)restInfo.cost.minCost;
                rest.cost.max = (byte)restInfo.cost.maxCost;
            }
            return rest;
        }


    }
}
