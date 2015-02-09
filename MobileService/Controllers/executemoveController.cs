using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;

namespace CustomAPIMobileService.Controllers
{
    public class executemoveController : ApiController
    {
        public ApiServices Services { get; set; }
        private int[,] Direction = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 1, 4, 7 }, { 2, 5, 8 }, { 3, 6, 9 }, { 1, 5, 9 }, { 3, 5, 7 } };
        private const string inconclusiveString = "inconclusive";
        private Random rnd = new Random();

        /// <summary>
        /// Takes in the input message, adds date and time and returns it.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>HttpResponseMessage.</returns>
        public HttpResponseMessage Post([FromBody]dynamic payload)
        {
            string[] TicBoard = new string[10];
            int i;
            string move = "n/a";
            int sum = 0;
            int nSpace = 0;
            int twoO = -1;
            int twoX = -1;
            int twoO_p = -1;
            int twoX_p = -1;
            string myPiece = "?";
            int move_p = -1;

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
                    sum = DetectWinner(TicBoard, i);

                    if (sum == 3 || sum == -3)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    move = "n/a",
                                    winner = (sum == 3 ? "O" : "X")
                                });
                    }
                    else if (sum == 2)
                    {
                        twoO = i;
                    }
                    else if (sum == -2)
                    {
                        twoX = i;
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

                //If we have two pieces in a row, get the positions of the empty position.
                if (twoO != -1)
                {
                    for (i = 0; i < 3; i++)
                    {
                        if (TicBoard[Direction[twoO, i]] == "?")
                        {
                            twoO_p = Direction[twoO, i];
                            break;
                        }
                    }

                    if (twoO_p == -1)
                    {
                        throw new AccessViolationException();
                    }
                }
                if (twoX != -1)
                {
                    for (i = 0; i < 3; i++)
                    {
                        if (TicBoard[Direction[twoX, i]] == "?")
                        {
                            twoX_p = Direction[twoX, i];
                            break;
                        }
                    }

                    if (twoX_p == -1)
                    {
                        throw new AccessViolationException();
                    }
                }

                //Determine my game piece.
                myPiece = (nSpace % 2 == 1) ? "X" : "O";

                //Win move
                if ((myPiece == "X" && twoX_p != -1) ||
                    (myPiece == "O" && twoO_p != -1))
                {
                    if (myPiece == "X")
                    {
                        move_p = twoX_p;
                    }
                    else
                    {
                        move_p = twoO_p;
                    }
                }
                //Block move
                else if ((myPiece == "X" && twoO_p != -1) ||
                         (myPiece == "O" && twoX_p != -1))
                {
                    if (myPiece == "X")
                    {
                        move_p = twoO_p;
                    }
                    else
                    {
                        move_p = twoX_p;
                    }
                }
                //Take center
                else if (TicBoard[5] == "?")
                {
                    move_p = 5;
                }
                else
                {
                    do
                    {
                        move_p = rnd.Next(1, 10);
                    }
                    while (TicBoard[move_p] != "?");
                }

                move = getStringFrom(move_p);
                TicBoard[move_p] = myPiece;
                nSpace--;

                if (move == "n/a")
                {
                    throw new AccessViolationException();
                }

                for (i = 0; i < 8; i++)
                {
                    sum = DetectWinner(TicBoard, i);

                    if (sum == 3 || sum == -3)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    move = move,
                                    winner = (sum == 3 ? "O" : "X")
                                });
                    }
                }

                if (nSpace == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                            new
                            {
                                move = move,
                                winner = "Tie"
                            });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    move = move,
                                    winner = "inconclusive"
                                });
            }
            catch (Exception)
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

        private int DetectWinner(string[] TicBoard, int d)
        {
            int sum = 0;

            for (int i = 0; i < 3; i++)
            {
                switch (TicBoard[Direction[d, i]])
                {
                    case "O":
                        sum += 1;
                        break;
                    case "X":
                        sum -= 1;
                        break;
                    case "?":
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return sum;
        }
    }
}
