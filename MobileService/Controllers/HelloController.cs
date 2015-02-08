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
        private int[,] Direction = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 1, 4, 7 }, { 2, 5, 8 }, { 3, 6, 9 }, { 1, 5, 9 }, { 3, 5, 7 } };
        private const string inconclusiveString = "inconclusive";

        /// <summary>
        /// Takes in the input message, adds date and time and returns it.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>HttpResponseMessage.</returns>
        public HttpResponseMessage Post([FromBody]dynamic payload)
        {
            string[] TicBoard = new string[10];
            int i;
            string winner;
            int nSpace = 0;

            try
            {
                if (payload == null ||
                    payload.one == null || payload.two == null || payload.three == null ||
                    payload.four == null || payload.five == null || payload.six == null ||
                    payload.seven == null || payload.eight == null || payload.nine == null)
                {
                    throw new KeyNotFoundException();
                }

                TicBoard[1] = payload.one; TicBoard[2] = payload.two; TicBoard[3] = payload.three;
                TicBoard[4] = payload.four; TicBoard[5] = payload.five; TicBoard[6] = payload.six;
                TicBoard[7] = payload.seven; TicBoard[8] = payload.eight; TicBoard[9] = payload.nine;

                for (i=1;i<=9;i++)
                {
                    if (TicBoard[i] != "X" && TicBoard[i] != "O" && TicBoard[i] != "?")
                    {
                        throw new FormatException();
                    }

                    if (TicBoard[i] == "?")
                    {
                        nSpace++;
                    }
                }

                for (i=0;i<8;i++)
                {
                    winner = DetectWinner(TicBoard, i);

                    if (winner != inconclusiveString)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    move = "n/a",
                                    winner = winner
                                });
                    }
                }

                if (nSpace == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                            new
                            {
                                move = "n/a",
                                winner = "Tie"
                            });
                }

                for (i=1;i<=9;i++)
                {
                    if (TicBoard[i] == "?")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                                        new
                                                        {
                                                            move = getStringFrom(i),
                                                            winner = "inconclusive"
                                                        });
                    }
                }

                throw new AccessViolationException();
            }
            catch (Exception ex)
            {
                return Request.CreateBadRequestResponse();
            }
        }

        private string getStringFrom(int number)
        {
            switch (number)
            {
                case 1:
                    return "one";
                case 2:
                    return "two";
                case 3:
                    return "three";
                case 4:
                    return "four";
                case 5:
                    return "five";
                case 6:
                    return "six";
                case 7:
                    return "seven";
                case 8:
                    return "eight";
                case 9:
                    return "nine";
                default:
                    throw new InvalidOperationException();
            }
        }

        private string DetectWinner(string[] TicBoard, int d)
        {
            if (TicBoard[Direction[d, 0]] == TicBoard[Direction[d, 1]] &&
                TicBoard[Direction[d, 1]] == TicBoard[Direction[d, 2]])
            {
                if (TicBoard[Direction[d, 0]] == "O" || TicBoard[Direction[d, 0]] == "X")
                {
                    return TicBoard[Direction[d, 0]];
                }
            }

            return inconclusiveString;
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
