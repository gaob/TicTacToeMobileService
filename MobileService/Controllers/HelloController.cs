using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;

namespace CustomAPIMobileService.Controllers
{
    public class HelloController : ApiController
    {
        public ApiServices Services { get; set; }

        /// <summary>
        /// Takes in the input message, adds date and time and returns it.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>HttpResponseMessage.</returns>
        public HttpResponseMessage Post([FromBody]dynamic payload)
        {
            try
            {
                if (payload == null ||
                    payload.one == null || payload.two == null || payload.three == null ||
                    payload.four == null || payload.five == null || payload.six == null ||
                    payload.seven == null || payload.eight == null || payload.nine == null)
                {
                    throw new KeyNotFoundException();
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                                                new
                                                {
                                                    move = "four",
                                                    winner = "inconclusive"
                                                });
            }
            catch (Exception ex)
            {
                return Request.CreateBadRequestResponse();
            }
        }

        /// <summary>
        /// Gets the date time
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                new
                {
                    message = "Hello World! (GET) " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                });
        }

    }
}
