using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BooksCatalogue.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BooksCatalogue.Controllers
{
    public class ReviewController : Controller
    {
        // private string apiEndpoint = "https://localhost:8000/api/";
        private string apiEndpoint = "https://bookscataloguedes-api.azurewebsites.net/api/";
        private readonly HttpClient _client;
        HttpClientHandler clientHandler = new HttpClientHandler();
        public ReviewController() {
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => {return true;};
            _client = new HttpClient(clientHandler);
        }

        // GET: Review/AddReview/2
        public async Task<IActionResult> AddReview(int? bookId)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();

            if (bookId == null)
            {
                return NotFound();
            }
            
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiEndpoint + "books/" + bookId);

            HttpResponseMessage response = await _client.SendAsync(request);

            switch(response.StatusCode)
            {
                case HttpStatusCode.OK:
                    string responseString = await response.Content.ReadAsStringAsync();
                    var book = JsonSerializer.Deserialize<Book>(responseString);

                    ViewData["BookId"] = bookId;
                    return View("Add");
                case HttpStatusCode.NotFound:
                    return NotFound();
                default:
                    return ErrorAction("Error. Status code = " + response.StatusCode + ": " + response.ReasonPhrase);
            }
        }

        // TODO: Tambahkan fungsi ini untuk mengirimkan atau POST data review menuju API
        // POST: Review/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview([Bind("Id,BookId,ReviewerName,Rating,Comment")][FromForm] Review review)
        {
            
            MultipartFormDataContent content = new MultipartFormDataContent();

            content.Add(new StringContent(review.BookId.ToString()), "bookId");
            content.Add(new StringContent(review.ReviewerName), "reviewerName");
            content.Add(new StringContent(review.Rating.ToString()), "rating");
            content.Add(new StringContent(review.Comment), "comment");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiEndpoint + "review/");
            request.Content = content;
            HttpResponseMessage response = await _client.SendAsync(request);

            switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.NoContent:
                    case HttpStatusCode.Created:
                        return Redirect("https://localhost:5001/Books/Details/" + review.BookId);
                    default:
                        return ErrorAction("Error. Status code = " + response.StatusCode + "; " + response.ReasonPhrase);
                }
        }

        private ActionResult ErrorAction(string message)
        {
            return new RedirectResult("/Home/Error?message=" + message);
        }
    }
}