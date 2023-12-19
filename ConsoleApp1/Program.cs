using ConsoleApp1;
using Nancy.Json;

 System.Timers.Timer timer1 = new System.Timers.Timer();
 HttpClient client = new HttpClient();

timer1.Interval = 30000;//30 sec
Logic logic = new Logic();
timer1.Elapsed += new System.Timers.ElapsedEventHandler(Logic.timer1_Tick);
timer1.Start();
Console.ReadLine();

public class Logic
{
    static public async void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
    {
        HttpClient client = new HttpClient();
        try
        {
            using HttpResponseMessage response = await client.GetAsync("https://localhost:44351/api/reservations");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            JavaScriptSerializer js = new JavaScriptSerializer();
            List<Reservation> reservations = js.Deserialize<List<Reservation>>(responseBody);

            using HttpResponseMessage responseParkings = await client.GetAsync("https://localhost:44351/api/parkings");
            responseParkings.EnsureSuccessStatusCode();
            string responseParkingsBody = await responseParkings.Content.ReadAsStringAsync();

            List<Parking> allParkings = js.Deserialize<List<Parking>>(responseParkingsBody);
            //endReservation if client not come, and the period is over

            // sort reservation logic
            for (int i = 0; i < allParkings.Count; i++)
            {
                var currParking = allParkings[i];
                var currParkingFutureReservations = currParking.Reservations.ToList().Where(x => x.StartReservationPeriod > DateTime.Now).ToList().OrderBy(x => x.StartReservationPeriod).ToList();
                var currParkingSpots = currParking.ParkingSpots.ToList();

                var currParkingBusySpots = currParkingSpots.Where(x => x.IsFree == false).ToList();
                var currParkingFreeSpots = currParkingSpots.Where(x => x.IsFree == true).ToList();
                List<List<Reservation>> listsWithCombinedOrderedReservations = new List<List<Reservation>>();
                List<int?> listAddedToReservationOfSpotId = new List<int?>();
                var currParkingBusySpotsOrdered = currParkingBusySpots.ToList();//.OrderBy(x => x.IsPaidTill).ToList();
                foreach (var spot in currParkingBusySpotsOrdered)
                {
                    var spotCurrentReservation = spot.Reservations.Where(x => x.IsStarted == true && x.IsEnded == false).FirstOrDefault();
                    if (currParkingFutureReservations != null && currParkingFutureReservations.Count > 0)
                    {
                        for (int k = 0; k < currParkingFutureReservations.Count; k++)
                        {
                            if (spotCurrentReservation == null || currParkingFutureReservations[k].StartReservationPeriod >= spotCurrentReservation.EndReservationPeriod.AddHours(1) && spotCurrentReservation.Is15MinOver == false || currParkingFutureReservations[k].StartReservationPeriod >= DateTime.Now.AddHours(1) && spotCurrentReservation.Is15MinOver == true)
                            {
                                if (listAddedToReservationOfSpotId == null || listAddedToReservationOfSpotId.Count == 0 || listAddedToReservationOfSpotId.Find(x => x == spot.Id) == null)
                                {
                                    var a = new List<Reservation>();
                                    a.Add(currParkingFutureReservations[k]);
                                    listsWithCombinedOrderedReservations.Add(a);
                                    listAddedToReservationOfSpotId.Add(spot.Id);
                                    currParkingFutureReservations.Remove(currParkingFutureReservations[k]);
                                    k = k - 1;
                                }
                                else
                                {
                                    var check = listAddedToReservationOfSpotId.Find(x => x == spot.Id);
                                    var indexInArray = listAddedToReservationOfSpotId.IndexOf(check);
                                    if (currParkingFutureReservations[k].StartReservationPeriod >= listsWithCombinedOrderedReservations[indexInArray].Last().EndReservationPeriod.AddHours(1))
                                    {
                                        listsWithCombinedOrderedReservations[indexInArray].Add(currParkingFutureReservations[k]);
                                        currParkingFutureReservations.Remove(currParkingFutureReservations[k]);
                                        k = k - 1;
                                    }
                                }
                            }
                        }
                    }

                }
                if (currParkingFutureReservations != null && currParkingFutureReservations.Count > 0)
                {
                    if (currParkingFreeSpots != null && currParkingFreeSpots.Count > 0)
                    {
                        foreach (var spot in currParkingFreeSpots)
                        {
                            for (int j = 0; j < currParkingFutureReservations.Count; j++)
                            {
                                if (listAddedToReservationOfSpotId == null || listAddedToReservationOfSpotId.Count == 0 || listAddedToReservationOfSpotId.Find(x => x == spot.Id) == null)
                                {
                                    var a = new List<Reservation>();
                                    a.Add(currParkingFutureReservations[j]);
                                    listsWithCombinedOrderedReservations.Add(a);
                                    listAddedToReservationOfSpotId.Add(spot.Id);
                                    currParkingFutureReservations.Remove(currParkingFutureReservations[j]);
                                }
                                else
                                {
                                    var check = listAddedToReservationOfSpotId.Find(x => x == spot.Id);
                                    var indexInArray = listAddedToReservationOfSpotId.IndexOf(check);
                                    if (currParkingFutureReservations[j].StartReservationPeriod >= listsWithCombinedOrderedReservations[indexInArray].Last().EndReservationPeriod.AddHours(1))
                                    {
                                        listsWithCombinedOrderedReservations[indexInArray].Add(currParkingFutureReservations[j]);
                                        currParkingFutureReservations.Remove(currParkingFutureReservations[j]);
                                    }
                                }
                            }
                        }
                    }

                }

                for (int j = 0; j < listAddedToReservationOfSpotId.Count; j++)
                {
                    var currSpotIndex = listAddedToReservationOfSpotId[j];
                    for (int k = 0; k < listsWithCombinedOrderedReservations[j].Count; k++)
                    {
                        var url = "https://localhost:44351/changeReservationSpot/" + listsWithCombinedOrderedReservations[j][k].Id + "/" + listAddedToReservationOfSpotId[j];
                        using HttpResponseMessage responsePost = await client.PutAsync(url, null);
                        responsePost.EnsureSuccessStatusCode();
                        string responsePostBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("responsePostBody");
                        Console.WriteLine(responsePostBody);

                    }
                }
            }
            if (reservations != null)
            {
                var notStartedReservations = reservations.Where(x => x.IsStarted == false && x.IsEnded == false).ToList();

                if (notStartedReservations != null && notStartedReservations.Count > 0)
                {
                    var notStartedReservationsWhichPeriodEnds = notStartedReservations.Where(x => x.EndReservationPeriod.AddMinutes(-2) <= DateTime.Now).ToList();
                    if (notStartedReservationsWhichPeriodEnds != null && notStartedReservationsWhichPeriodEnds.Count > 0)
                    {
                        for (int i = 0; i < notStartedReservationsWhichPeriodEnds.Count; i++)
                        {
                            using HttpResponseMessage response4 = await client.PutAsync("https://localhost:44351/endReservation/" + notStartedReservationsWhichPeriodEnds[i].Id, null);
                            Console.WriteLine(response4);
                        }
                    }
                }


                //set reservations to avaliableFromEarlier
                var futureStartReseravation = reservations.Where(x => x.IsStarted == false && x.StartReservationPeriod > DateTime.Now).ToList();
                if (futureStartReseravation != null && futureStartReseravation.Count > 0)
                {
                    for (int i = 0; i < futureStartReseravation.Count; i++)
                    {
                        if (futureStartReseravation[i].StartReservationPeriod <= DateTime.Now.AddMinutes(40))
                        {
                            if (futureStartReseravation[i].Spot.IsFree == true)
                            {
                                using HttpResponseMessage response4 = await client.PutAsync("https://localhost:44351/canStartEarly/" + futureStartReseravation[i].Id, null);
                                Console.WriteLine(response4);
                                //set reservation to avaliableFromEarlier
                            }
                        }
                    }
                }

                //set reservations to failed if spot is not free at start time
                var blockedReservations = reservations.Where(x => x.IsStarted == false && x.IsEnded == false && x.StartReservationPeriod.AddMinutes(1) <= DateTime.Now && x.Spot.IsFree == false).ToList();
                if (blockedReservations != null && blockedReservations.Count > 0)
                {
                    for (int i = 0; i < blockedReservations.Count; i++)
                    {
                        using HttpResponseMessage response4 = await client.PutAsync("https://localhost:44351/failReservation/" + blockedReservations[i].Id, null);
                        Console.WriteLine(response4);
                    }
                }
            }
            Console.ReadKey();

        }
        catch (HttpRequestException err)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", err.Message);
        }
        Console.ReadKey();
    }
}
