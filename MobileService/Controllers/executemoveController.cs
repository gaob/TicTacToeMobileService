using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;

namespace CustomAPIMobileService.Controllers
{
    /// <summary>
    /// API executemove, and its controller class.
    /// </summary>
    public class executemoveController : ApiController
    {
        public ApiServices Services { get; set; }
        /// <summary>
        /// The two-dimensional array to store 8 directions of the board.
        /// The first dimension indicates which direction.
        /// The second dimension indicates one of the three positions in that direction.
        /// </summary>
        private int[,] Direction = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 1, 4, 7 }, { 2, 5, 8 }, { 3, 6, 9 }, { 1, 5, 9 }, { 3, 5, 7 } };
        /// <summary>
        /// The string constant to store "inconclusive".
        /// </summary>
        private const string inconclusiveString = "inconclusive";
        private Random rnd = new Random();

        /// <summary>
        /// Takes in the input message of the board, and return the move and winner.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        /// Example:
        /// {
        ///     "move": "three",
        ///     "winner": "inconclusive"
        /// }
        /// </returns>
        public HttpResponseMessage Post([FromBody]dynamic payload)
        {
            // Store the board from index 1 to index 9, each position has "O", "X", or "?".
            string[] TicBoard = new string[10];
            int i;
            // Store the move to be returned, "O", "X", or "n/a".
            string move = "n/a";
            int sum = 0;
            // # of spaces left in the board.
            int nSpace = 0;
            // Store the direction that has two O's or two X's.
            int twoO = -1;
            int twoX = -1;
            // Store the empty position of the direction that has two O's or two X's.
            int twoO_p = -1;
            int twoX_p = -1;
            // What's the symbol of my game piece?
            string myPiece = "?";
            // The index of the move.
            int move_p = -1;

            try
            {
                //Check if we have all the necessary keys.
                if (payload == null ||
                    payload.one == null || payload.two == null || payload.three == null ||
                    payload.four == null || payload.five == null || payload.six == null ||
                    payload.seven == null || payload.eight == null || payload.nine == null)
                {
                    throw new KeyNotFoundException();
                }

                //Restore the payload to the board configuration.
                TicBoard[1] = payload.one; TicBoard[2] = payload.two; TicBoard[3] = payload.three;
                TicBoard[4] = payload.four; TicBoard[5] = payload.five; TicBoard[6] = payload.six;
                TicBoard[7] = payload.seven; TicBoard[8] = payload.eight; TicBoard[9] = payload.nine;

                for (i=1;i<=9;i++)
                {
                    //Check if each position has invalid symbol.
                    if (TicBoard[i] != "X" && TicBoard[i] != "O" && TicBoard[i] != "?")
                    {
                        throw new FormatException();
                    }

                    //Calculate # of empty positions.
                    if (TicBoard[i] == "?")
                    {
                        nSpace++;
                    }
                }

                // Before move check:
                // Iterate through 8 directions.
                for (i=0;i<8;i++)
                {
                    // Get the sum of the direction.
                    sum = DetectWinner(TicBoard, i);

                    // Sum of 3 or -3 indicates there is a winner.
                    if (sum == 3 || sum == -3)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    move = "n/a",
                                    winner = (sum == 3 ? "O" : "X")
                                });
                    }
                    // If we have strict two pieces in a row, store the direction.
                    else if (sum == 2)
                    {
                        twoO = i;
                    }
                    else if (sum == -2)
                    {
                        twoX = i;
                    }
                }

                // No space left means a tied game.
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

                //Make a Win move if possible.
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
                //Block a Win if necessary.
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
                //Take center if possible.
                else if (TicBoard[5] == "?")
                {
                    move_p = 5;
                }
                //Otherwise, take a random move.
                else
                {
                    do
                    {
                        move_p = rnd.Next(1, 10);
                    }
                    while (TicBoard[move_p] != "?");
                }

                //Get the move string based on move position.
                move = getStringFrom(move_p);
                //Put the piece.
                TicBoard[move_p] = myPiece;
                nSpace--;

                if (move == "n/a")
                {
                    throw new AccessViolationException();
                }

                //After move check for Win.
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

                //After move check for tie.
                if (nSpace == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                            new
                            {
                                move = move,
                                winner = "Tie"
                            });
                }

                //Otherwise, the game is inconclusive.
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

        /// <summary>
        /// Convert position number to string.
        /// </summary>
        /// <param name="number">The index of the position</param>
        /// <returns>String representing the position</returns>
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

        /// <summary>
        /// Return the sum of the direction.
        /// </summary>
        /// <param name="TicBoard">The board with symbols</param>
        /// <param name="d">The direction</param>
        /// <returns>
        /// Sum of game pieces in the direction:
        /// Each "O" will contribute 1, Each "X" will contribute -1, and each "?" will contribute 0.
        /// </returns>
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
